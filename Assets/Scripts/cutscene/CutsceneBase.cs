using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Base class for scripted, skippable cutscenes.
/// A subclass only writes Sequence() using the step helpers below. The base class handles guards
/// (completed event, cave timeline), freezing the active player, skip registration, cleanup, event
/// registration, saving and resuming gameplay.
///
/// Skipping fast-forwards the rest of Sequence() synchronously with IsSkipping == true: every helper
/// applies its end state instantly instead of animating, so a skipped cutscene ends in the same state
/// as one that played to the end.
///
/// Rules for Sequence():
/// - Only yield step helpers, nested IEnumerators, null or Unity yield instructions. Never yield StartCoroutine(...).
/// - Hand-written loops must include !IsSkipping in their condition, otherwise skipping cannot finish them.
/// - Purely cosmetic hand-written code (one-shot sounds, particles) should be wrapped in if (!IsSkipping).
/// - Permanent state changes (SetActive on world objects, unlocking powers) can be written directly;
///   they also run while skipping.
/// </summary>
public abstract class CutsceneBase : MonoBehaviour, ISkippable
{
    private const int MaxSkipIterations = 10000;
    private const float DefaultSootSpeed = 5f;
    private const float SootArrivalDistance = 0.1f;
    private const float SootArrivalExtraSeconds = 1f;
    private const float DefaultMoveTimeoutSeconds = 10f;
    private const float DefaultJumpHoldSeconds = 0.3f;

    [Header("Cutscene")]
    [Tooltip("Registered when the cutscene finishes or is skipped. The cutscene never plays again once this event exists.")]
    [SerializeField] private GameEventId _completedEvent;
    [Tooltip("Cave timelines this cutscene plays in. Leave empty to play in every timeline.")]
    [SerializeField] private CaveTimelineId.Id[] _allowedTimelines;
    [SerializeField] private bool _saveOnComplete = true;
    [SerializeField] private bool _isSkippable = true;

    /// <summary>True from StartCutscene until the cutscene has finished or been skipped.</summary>
    public bool IsPlaying { get; private set; }

    /// <summary>True while the rest of the sequence is being fast-forwarded after a skip request.</summary>
    protected bool IsSkipping { get; private set; }

    /// <summary>The character that was active when the cutscene started. It is frozen for the whole cutscene.</summary>
    protected PlayerManager.PlayerType ControlledPlayer { get; private set; } = PlayerManager.PlayerType.NONE;

    /// <summary>The controlled character's transform.</summary>
    protected Transform PlayerTransform => PlayerManager.obj.GetPlayerTransform(ControlledPlayer);

    /// <summary>True if the completed event has already been registered in this save.</summary>
    public bool HasCompleted => _completedEvent != null && GameManager.obj.HasEvent(_completedEvent);

    /// <summary>Raised once when the cutscene ends. The argument is true if it was skipped.</summary>
    public event Action<bool> OnCutsceneFinished;

    private readonly Stack<IEnumerator> _sequenceStack = new Stack<IEnumerator>();
    private readonly List<Action> _cleanupActions = new List<Action>();
    private readonly List<Action> _skipActions = new List<Action>();
    private readonly List<EventInstance> _trackedSounds = new List<EventInstance>();
    private Coroutine _runnerCoroutine;
    private ConversationManager _activeConversation;
    private Action _activeConversationEndHandler;
    private bool _isPlayerMovingScripted;
    private Transform _pendingSootTarget;

    /// <summary>
    /// The cutscene steps. Called once per playthrough of the cutscene.
    /// </summary>
    protected abstract IEnumerator Sequence();

    /// <summary>
    /// Optional hook that runs after cleanup on both normal end and skip, before the event is registered.
    /// Use it for custom final state that the step helpers don't cover.
    /// </summary>
    protected virtual void OnCutsceneEnded(bool wasSkipped) { }

    /// <summary>
    /// Starts the cutscene if it is allowed to play. Hook this up to a PlayerTrigger's OnPlayerEntered event.
    /// </summary>
    public void StartCutscene() {
        if (IsPlaying || !CanPlay()) {
            return;
        }

        ControlledPlayer = PlayerManager.obj.GetActivePlayerType();
        IsPlaying = true;
        IsSkipping = false;
        _cleanupActions.Clear();
        _skipActions.Clear();
        _trackedSounds.Clear();
        _pendingSootTarget = null;

        PlayerManager.obj.FreezePlayer(ControlledPlayer);
        if (_isSkippable) {
            PauseMenuManager.obj.RegisterSkippable(this);
        }

        _sequenceStack.Clear();
        _sequenceStack.Push(Sequence());
        _runnerCoroutine = StartCoroutine(RunSequence());
    }

    /// <summary>
    /// Guard checked by StartCutscene. Override to add room-specific conditions (call base first).
    /// </summary>
    protected virtual bool CanPlay() {
        if (HasCompleted) {
            return false;
        }
        if (_allowedTimelines != null && _allowedTimelines.Length > 0) {
            CaveTimelineId.Id timeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
            if (Array.IndexOf(_allowedTimelines, timeline) < 0) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Called by PauseMenuManager after it has faded the screen out. Fast-forwards the sequence to its end state.
    /// </summary>
    public void RequestSkip() {
        if (!IsPlaying) {
            // Skipped in the same frame the cutscene ended: the screen is black, so only fade back in.
            StartCoroutine(FadeInAfterSkip(false));
            return;
        }

        IsSkipping = true;
        if (_runnerCoroutine != null) {
            StopCoroutine(_runnerCoroutine);
            _runnerCoroutine = null;
        }

        RunActions(_skipActions);
        SnapSootToPendingTarget();
        FastForwardSequence();
        FinishCutscene(true);
    }

    #region Sequence runner

    private IEnumerator RunSequence() {
        while (_sequenceStack.Count > 0) {
            IEnumerator currentStep = _sequenceStack.Peek();
            if (!currentStep.MoveNext()) {
                _sequenceStack.Pop();
                continue;
            }
            if (currentStep.Current is IEnumerator nestedStep) {
                _sequenceStack.Push(nestedStep);
                continue;
            }
            yield return currentStep.Current;
        }

        _runnerCoroutine = null;
        FinishCutscene(false);
    }

    private void FastForwardSequence() {
        int iterations = 0;
        while (_sequenceStack.Count > 0) {
            if (++iterations > MaxSkipIterations) {
                Debug.LogError($"{GetType().Name}: skip did not finish after {MaxSkipIterations} steps. A hand-written loop in Sequence() is probably missing '!IsSkipping'.", this);
                _sequenceStack.Clear();
                break;
            }

            IEnumerator currentStep = _sequenceStack.Peek();
            bool hasMoreSteps;
            try {
                hasMoreSteps = currentStep.MoveNext();
            } catch (Exception exception) {
                Debug.LogException(exception, this);
                hasMoreSteps = false;
            }

            if (!hasMoreSteps) {
                _sequenceStack.Pop();
            } else if (currentStep.Current is IEnumerator nestedStep) {
                _sequenceStack.Push(nestedStep);
            }
            // Other yield instructions (WaitForSeconds, null, ...) are ignored while skipping.
        }
    }

    private void FinishCutscene(bool wasSkipped) {
        StopPlayerMoving();
        UnsubscribeActiveConversation();
        RunActions(_cleanupActions);
        ReleaseTrackedSounds(wasSkipped);
        OnCutsceneEnded(wasSkipped);

        if (_completedEvent != null) {
            GameManager.obj.RegisterEvent(_completedEvent);
        }
        if (_saveOnComplete) {
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        }
        if (_isSkippable) {
            PauseMenuManager.obj.UnregisterSkippable();
        }

        IsPlaying = false;
        IsSkipping = false;

        if (wasSkipped) {
            StartCoroutine(FadeInAfterSkip(true));
        } else {
            PlayerManager.obj.UnfreezePlayer(ControlledPlayer);
        }

        OnCutsceneFinished?.Invoke(wasSkipped);
    }

    private IEnumerator FadeInAfterSkip(bool unfreezePlayer) {
        SceneFadeManager.obj.StartFadeIn();
        while (SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        if (unfreezePlayer) {
            PlayerManager.obj.UnfreezePlayer(ControlledPlayer);
        }
        GameManager.obj.IsPauseAllowed = true;
    }

    private static void RunActions(List<Action> actions) {
        foreach (Action action in actions) {
            try {
                action();
            } catch (Exception exception) {
                Debug.LogException(exception);
            }
        }
        actions.Clear();
    }

    /// <summary>Registers an action that runs when the cutscene ends, whether it finished or was skipped.</summary>
    protected void AddCleanup(Action cleanupAction) {
        _cleanupActions.Add(cleanupAction);
    }

    /// <summary>Registers an action that runs only if the cutscene is skipped, before the rest of the sequence is fast-forwarded.</summary>
    protected void AddSkipAction(Action skipAction) {
        _skipActions.Add(skipAction);
    }

    #endregion

    #region Timing

    /// <summary>Waits for the given time. Returns instantly while skipping.</summary>
    protected IEnumerator Wait(float seconds) {
        float endTime = Time.time + seconds;
        while (!IsSkipping && Time.time < endTime) {
            yield return null;
        }
    }

    /// <summary>Waits until the condition is true. Returns instantly while skipping.</summary>
    protected IEnumerator WaitFor(Func<bool> condition) {
        while (!IsSkipping && !condition()) {
            yield return null;
        }
    }

    #endregion

    #region Camera and screen

    /// <summary>Activates a cutscene camera (Cutscene camera prefab). It is deactivated automatically when the cutscene ends.</summary>
    protected void ShowCamera(GameObject cutsceneCamera) {
        AddCleanup(() => cutsceneCamera.SetActive(false));
        if (!IsSkipping) {
            cutsceneCamera.SetActive(true);
        }
    }

    /// <summary>Deactivates a cutscene camera, blending back to the room camera.</summary>
    protected void HideCamera(GameObject cutsceneCamera) {
        cutsceneCamera.SetActive(false);
    }

    /// <summary>Shakes the active camera. Stopped if the cutscene is skipped.</summary>
    protected void Shake(float amplitude, float frequency, float seconds) {
        if (IsSkipping) {
            return;
        }
        CameraShakeManager.obj.ShakeCamera(amplitude, frequency, seconds);
        AddSkipAction(() => CameraShakeManager.obj.ShakeCamera(0, 0, 0));
    }

    /// <summary>Fades the screen to black and waits for it. Skipped while skipping.</summary>
    protected IEnumerator FadeOut() {
        if (IsSkipping) {
            yield break;
        }
        SceneFadeManager.obj.StartFadeOut();
        while (!IsSkipping && SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }
    }

    /// <summary>Fades the screen in from black and waits for it. Skipped while skipping (the skip resume fades in).</summary>
    protected IEnumerator FadeIn() {
        if (IsSkipping) {
            yield break;
        }
        SceneFadeManager.obj.StartFadeIn();
        while (!IsSkipping && SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
    }

    #endregion

    #region Player

    /// <summary>
    /// Moves the controlled character horizontally until it reaches the target's x position.
    /// Eli can use the slow walk animation; Dee always runs; Blob is not supported and is placed directly.
    /// While skipping, the character is placed at the target.
    /// </summary>
    protected IEnumerator MovePlayerTo(Transform target, bool useWalkAnimation = true, float timeoutSeconds = DefaultMoveTimeoutSeconds) {
        if (!IsSkipping) {
            float directionX = Mathf.Sign(target.position.x - PlayerTransform.position.x);
            if (StartPlayerMoving(directionX, useWalkAnimation)) {
                float endTime = Time.time + timeoutSeconds;
                while (!IsSkipping && !HasReachedX(directionX, target.position.x) && Time.time < endTime) {
                    yield return null;
                }
                if (!IsSkipping && Time.time >= endTime) {
                    Debug.LogWarning($"{GetType().Name}: player did not reach {target.name} within {timeoutSeconds}s. Placing it there.", this);
                    PlacePlayerAt(target);
                }
                StopPlayerMoving();
            } else {
                PlacePlayerAt(target);
            }
        }
        if (IsSkipping) {
            StopPlayerMoving();
            PlacePlayerAt(target);
        }
    }

    /// <summary>
    /// Starts moving the controlled character without waiting (use with Wait/PlayerJump for scripted jumps).
    /// Returns false for Blob, which has no scripted movement input. Stop it with StopPlayerMoving.
    /// </summary>
    protected bool StartPlayerMoving(float directionX, bool useWalkAnimation = true) {
        if (IsSkipping) {
            return false;
        }
        Vector2 movementInput = new Vector2(Mathf.Sign(directionX), 0);
        switch (ControlledPlayer) {
            case PlayerManager.PlayerType.HUMAN:
                if (useWalkAnimation) {
                    PlayerMovement.obj.StartWalking();
                }
                PlayerMovement.obj.SetMovementInput(movementInput);
                break;
            case PlayerManager.PlayerType.SHADOW_TWIN:
                ShadowTwinMovement.obj.SetMovementInput(movementInput);
                break;
            default:
                Debug.LogWarning($"{GetType().Name}: scripted movement is not supported for {ControlledPlayer}.", this);
                return false;
        }
        _isPlayerMovingScripted = true;
        return true;
    }

    /// <summary>Stops scripted movement and freezes the character again (SetMovementInput clears the freeze flag).</summary>
    protected void StopPlayerMoving() {
        if (!_isPlayerMovingScripted) {
            return;
        }
        _isPlayerMovingScripted = false;
        switch (ControlledPlayer) {
            case PlayerManager.PlayerType.HUMAN:
                PlayerMovement.obj.SetMovementInput(Vector2.zero);
                PlayerMovement.obj.StopWalking();
                break;
            case PlayerManager.PlayerType.SHADOW_TWIN:
                ShadowTwinMovement.obj.SetMovementInput(Vector2.zero);
                break;
        }
        PlayerManager.obj.FreezePlayer(ControlledPlayer);
    }

    /// <summary>Presses jump, holds it for the given time and releases it (Eli and Dee). Skipped while skipping.</summary>
    protected IEnumerator PlayerJump(float holdSeconds = DefaultJumpHoldSeconds) {
        if (IsSkipping || !SimulateJump(true)) {
            yield break;
        }
        yield return Wait(holdSeconds);
        SimulateJump(false);
    }

    /// <summary>Teleports the controlled character to the target and grounds it. Also used as the skip end state.</summary>
    protected void PlacePlayerAt(Transform target) {
        PlayerTransform.position = target.position;
        switch (ControlledPlayer) {
            case PlayerManager.PlayerType.HUMAN:
                PlayerMovement.obj.CancelJumping();
                PlayerMovement.obj.SetStartingOnGround();
                PlayerMovement.obj.isGrounded = true;
                Player.obj.ResetAnimator();
                break;
            case PlayerManager.PlayerType.SHADOW_TWIN:
                ShadowTwinMovement.obj.CancelJumping();
                ShadowTwinMovement.obj.SetStartingOnGround();
                ShadowTwinMovement.obj.isGrounded = true;
                ShadowTwinPlayer.obj.ResetAnimator();
                break;
            case PlayerManager.PlayerType.BLOB:
                PlayerBlobMovement.obj.CancelJumping();
                PlayerBlobMovement.obj.SetStartingOnGround();
                PlayerBlobMovement.obj.isGrounded = true;
                PlayerBlob.obj.ResetAnimator();
                break;
        }
    }

    /// <summary>Turns the controlled character to face left or right. Also applied while skipping.</summary>
    protected void FacePlayer(bool faceLeft) {
        if (PlayerManager.obj.IsPlayerFacingLeft(ControlledPlayer) != faceLeft) {
            PlayerManager.obj.FlipPlayer(ControlledPlayer);
        }
    }

    /// <summary>Flashes the controlled character. Aborted if the cutscene is skipped.</summary>
    protected void FlashPlayer(float seconds) {
        if (IsSkipping) {
            return;
        }
        switch (ControlledPlayer) {
            case PlayerManager.PlayerType.HUMAN:
                Player.obj.FlashFor(seconds);
                AddSkipAction(() => Player.obj.AbortFlash());
                break;
            case PlayerManager.PlayerType.SHADOW_TWIN:
                ShadowTwinPlayer.obj.FlashFor(seconds);
                AddSkipAction(() => ShadowTwinPlayer.obj.AbortFlash());
                break;
            case PlayerManager.PlayerType.BLOB:
                PlayerBlob.obj.FlashFor(seconds);
                AddSkipAction(() => PlayerBlob.obj.AbortFlash());
                break;
        }
    }

    private bool SimulateJump(bool isJumpHeld) {
        switch (ControlledPlayer) {
            case PlayerManager.PlayerType.HUMAN:
                PlayerMovement.obj.SimulateJumpInput(isJumpHeld, Time.time);
                return true;
            case PlayerManager.PlayerType.SHADOW_TWIN:
                ShadowTwinMovement.obj.SimulateJumpInput(isJumpHeld, Time.time);
                return true;
            default:
                Debug.LogWarning($"{GetType().Name}: scripted jumping is not supported for {ControlledPlayer}.", this);
                return false;
        }
    }

    private bool HasReachedX(float directionX, float targetX) {
        float playerX = PlayerTransform.position.x;
        return directionX > 0 ? playerX >= targetX : playerX <= targetX;
    }

    #endregion

    #region Soot (CaveAvatar)

    /// <summary>
    /// Flies Soot to the target. If waitForArrival is true the step waits until Soot arrives.
    /// While skipping, Soot is placed at the target.
    /// </summary>
    protected IEnumerator MoveSootTo(Transform target, float speed = DefaultSootSpeed, bool waitForArrival = true) {
        if (!IsSkipping) {
            CaveAvatar.obj.SetTarget(target, speed);
            // Only the latest unfinished move matters on skip; a later move or follow replaces it.
            _pendingSootTarget = waitForArrival ? null : target;
            if (!waitForArrival) {
                yield break;
            }
            float distance = Vector2.Distance(CaveAvatar.obj.GetHeadTransform().position, target.position);
            float endTime = Time.time + distance / Mathf.Max(speed, 0.01f) + SootArrivalExtraSeconds;
            while (!IsSkipping && Time.time < endTime
                   && Vector2.Distance(CaveAvatar.obj.GetHeadTransform().position, target.position) > SootArrivalDistance) {
                yield return null;
            }
        }
        if (IsSkipping) {
            CaveAvatar.obj.SetPosition(target.position, false);
        }
    }

    /// <summary>Makes Soot follow the active player again. Also applied while skipping.</summary>
    protected void SootFollowPlayer() {
        _pendingSootTarget = null;
        CaveAvatar.obj.FollowPlayer();
    }

    private void SnapSootToPendingTarget() {
        if (_pendingSootTarget != null) {
            CaveAvatar.obj.SetPosition(_pendingSootTarget.position, false);
            _pendingSootTarget = null;
        }
    }

    /// <summary>Flips Soot's head sprite. Also applied while skipping.</summary>
    protected void SetSootFacingLeft(bool faceLeft) {
        CaveAvatar.obj.SetFlipX(faceLeft);
    }

    #endregion

    #region Conversation

    /// <summary>
    /// Plays a conversation and waits until it ends. Not started while skipping; hard-stopped if skipped mid-conversation.
    /// An empty ConversationManager is skipped with a warning so the cutscene doesn't hang.
    /// Set cleanUpWhenDone to false when another conversation follows immediately and the dialogue box should stay up.
    /// </summary>
    protected IEnumerator PlayConversation(ConversationManager conversation, bool cleanUpWhenDone = true) {
        if (IsSkipping) {
            yield break;
        }
        if (!conversation.HasDialogue) {
            Debug.LogWarning($"{GetType().Name}: {conversation.name} has no dialogue assigned. Skipping the conversation step.", this);
            yield break;
        }

        bool hasEnded = false;
        _activeConversation = conversation;
        _activeConversationEndHandler = () => hasEnded = true;
        if (!conversation.enabled) {
            conversation.enabled = true;
        }
        conversation.OnConversationEnd += _activeConversationEndHandler;
        conversation.StartConversation();

        while (!IsSkipping && !hasEnded) {
            yield return null;
        }

        UnsubscribeActiveConversation();
        if (IsSkipping) {
            conversation.HardStopConversation();
            conversation.CleanUp();
        } else if (cleanUpWhenDone) {
            conversation.CleanUp();
        }
    }

    private void UnsubscribeActiveConversation() {
        if (_activeConversation != null && _activeConversationEndHandler != null) {
            _activeConversation.OnConversationEnd -= _activeConversationEndHandler;
        }
        _activeConversation = null;
        _activeConversationEndHandler = null;
    }

    #endregion

    #region Audio

    /// <summary>Plays a 2D one-shot. Not played while skipping.</summary>
    protected void PlaySfx(EventReference sfx) {
        if (!IsSkipping) {
            SoundFXManager.obj.Play2D(sfx);
        }
    }

    /// <summary>Plays a positional one-shot. Not played while skipping.</summary>
    protected void PlaySfxAt(EventReference sfx, Vector3 position) {
        if (!IsSkipping) {
            SoundFXManager.obj.PlayAtPosition(sfx, position);
        }
    }

    /// <summary>
    /// Starts a tracked sound (stinger, rumble, loop) attached to the emitter (this GameObject by default).
    /// On normal end it is released and plays out; on skip it is stopped immediately.
    /// </summary>
    protected EventInstance PlayTrackedSfx(EventReference sfx, GameObject emitter = null) {
        if (IsSkipping) {
            return default;
        }
        EventInstance instance = SoundFXManager.obj.CreateAttachedInstance(sfx, emitter != null ? emitter : gameObject);
        instance.start();
        _trackedSounds.Add(instance);
        return instance;
    }

    /// <summary>Stops a sound started with PlayTrackedSfx, with fade-out.</summary>
    protected void StopTrackedSfx(EventInstance instance) {
        _trackedSounds.Remove(instance);
        AudioUtils.SafeStop(ref instance);
    }

    /// <summary>Plays a music track. Also applied while skipping, so the music ends up in its final state.</summary>
    protected void PlayMusic(MusicTrack track) {
        MusicManager.obj.Play(track);
    }

    /// <summary>Stops the current music track. Also applied while skipping.</summary>
    protected void StopMusic() {
        MusicManager.obj.Stop();
    }

    /// <summary>Starts an ambience track. Also applied while skipping.</summary>
    protected void PlayAmbience(AmbienceTrack track) {
        AmbienceManager.obj.Play(track);
    }

    /// <summary>Stops one ambience track, or all ambience when track is null. Also applied while skipping.</summary>
    protected void StopAmbience(AmbienceTrack track = null) {
        if (track == null) {
            AmbienceManager.obj.StopAll();
        } else {
            AmbienceManager.obj.Stop(track);
        }
    }

    private void ReleaseTrackedSounds(bool stopImmediately) {
        for (int i = 0; i < _trackedSounds.Count; i++) {
            EventInstance instance = _trackedSounds[i];
            if (stopImmediately) {
                AudioUtils.SafeStop(ref instance, FMOD.Studio.STOP_MODE.IMMEDIATE);
            } else if (instance.isValid()) {
                instance.release();
            }
        }
        _trackedSounds.Clear();
    }

    #endregion

    #region Animation

    /// <summary>
    /// Sets an animator trigger. While skipping, the trigger is not set; if skipStateName is given,
    /// that state is played instead so the animator ends up in its final pose.
    /// </summary>
    protected void SetAnimatorTrigger(Animator animator, string trigger, string skipStateName = null) {
        if (!IsSkipping) {
            animator.SetTrigger(trigger);
            if (!string.IsNullOrEmpty(skipStateName)) {
                AddSkipAction(() => animator.Play(skipStateName));
            }
        } else if (!string.IsNullOrEmpty(skipStateName)) {
            animator.Play(skipStateName);
        }
    }

    #endregion

    protected virtual void OnDestroy() {
        if (!IsPlaying) {
            return;
        }
        // Scene unloaded mid-cutscene: don't leave dangling subscriptions, sounds or skip registration behind.
        UnsubscribeActiveConversation();
        ReleaseTrackedSounds(true);
        if (_isSkippable && PauseMenuManager.obj != null) {
            PauseMenuManager.obj.UnregisterSkippable();
        }
    }
}
