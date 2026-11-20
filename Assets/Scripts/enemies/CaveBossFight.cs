using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Random = UnityEngine.Random;

// Stationary cave boss for the boss fight room. Glides in, attacks with tentacles while the player
// hits it with CaveBossOrbs, grows guard tentacles after each hit and retreats to the left after the last hit.
// Hits needed = number of stages; stage index = hits taken so far.
public class CaveBossFight : MonoBehaviour
{
    [Serializable]
    public class Stage {
        [Tooltip("Guard tentacles that grow out when this stage starts.")]
        public CaveBossGuardTentacle[] guardTentacles;
        [Min(0.1f)] public float minAttackInterval = 3f;
        [Min(0.1f)] public float maxAttackInterval = 5f;
        [Tooltip("Optional, together with Attack Range Max Y. Lowest Y attack tentacles can spawn at during this stage. " +
                 "Targets outside the range are attacked at this Y.")]
        public Transform attackRangeMinY;
        [Tooltip("Optional, together with Attack Range Min Y. Highest Y attack tentacles can spawn at during this stage.")]
        public Transform attackRangeMaxY;

        public bool HasAttackRange => attackRangeMinY != null && attackRangeMaxY != null;
    }

    private enum State { Idle, Entering, Fighting, HitReaction, Retreating }

    private const float NormalWiggleSpeed = 1f;
    private const float MinimumFadeDuration = 0.01f;
    // The continuous rumble re-applies a camera shake of this length every frame, so it stops shortly after the rumble is stopped
    private const float ContinuousRumbleShakeRefreshDuration = 0.1f;

    [Header("Movement")]
    [Tooltip("Where the boss stops after entering. Only X is used.")]
    [SerializeField] private Transform _fightPosition;
    [SerializeField] private float _enterSpeed = 2f;
    [SerializeField] private float _retreatSpeed = 1.5f;
    [Tooltip("Distance the boss retreats to the left before despawning.")]
    [SerializeField] private float _retreatDistance = 15f;

    [Header("Stages (index = hits taken)")]
    [SerializeField] private Stage[] _stages = new Stage[3];

    [Header("Attack")]
    [SerializeField] private CaveBossAttackTentacle _attackTentaclePrefab;
    [Tooltip("Attack tentacles spawn at this X (inside the body), at the target's Y.")]
    [SerializeField] private Transform _attackRoot;

    [Header("Static Tentacles")]
    [Tooltip("Tentacles that stay attached to the body. Each target keeps its offset from the body, so the tentacles follow it when it moves.")]
    [SerializeField] private CaveBoss.TentacleSlot[] _tentacles;
    [Tooltip("Wiggle speed multiplier of the static tentacles at the peak of a hit.")]
    [SerializeField, Min(0f)] private float _tentacleHitWiggleSpeed = 3f;
    [Tooltip("Seconds for the hit wiggle to fade back to normal.")]
    [SerializeField, Min(MinimumFadeDuration)] private float _tentacleHitWiggleFadeDuration = 1.5f;
    [Tooltip("Wiggle speed multiplier kept from the final hit until the boss despawns.")]
    [SerializeField, Min(0f)] private float _tentacleDefeatedWiggleSpeed = 3f;
    [Tooltip("How quickly the wiggle speed reacts to changes. Higher = snappier.")]
    [SerializeField, Min(0f)] private float _tentacleWiggleResponse = 8f;

    [Header("Hurtbox")]
    [Tooltip("Trigger collider the orbs have to hit.")]
    [SerializeField] private Collider2D _hurtbox;

    [Header("Hit Reaction")]
    [Tooltip("Shaken on hit. Body and heads should be children of it, the attack root and tentacles shouldn't.")]
    [SerializeField] private Transform _visual;
    [SerializeField] private SpriteRenderer[] _flashRenderers;
    [SerializeField] private Material _flashMaterial;
    [SerializeField, Min(1)] private int _flashCount = 3;
    [SerializeField] private float _flashDuration = 0.08f;
    [Tooltip("Time the boss shakes and doesn't attack after a hit.")]
    [SerializeField] private float _hitReactionDuration = 1.2f;
    [SerializeField] private float _hitShakeStrength = 0.3f;
    [SerializeField] private float _retreatShakeStrength = 0.15f;
    [SerializeField] private int _shakeVibrato = 20;
    [Tooltip("Floating heads and body parts. Agitated on hit, kept agitated after defeat. Optional.")]
    [SerializeField] private CaveBossFloatingParts _floatingParts;

    [Header("Rumble")]
    [Tooltip("Seconds between rumbles while fighting.")]
    [SerializeField, Min(0f)] private float _rumbleMinInterval = 15f;
    [SerializeField, Min(0f)] private float _rumbleMaxInterval = 25f;
    [SerializeField, Min(0f)] private float _rumbleDuration = 2f;
    [SerializeField] private float _rumbleShakeAmplitude = 1.94f;
    [SerializeField] private float _rumbleShakeFrequency = 1.84f;
    [SerializeField] private EventReference _earthquakeStinger;

    [Header("Debug")]
    [Tooltip("When on, the boss's tentacles and other kill triggers can't kill the player. Can be toggled in Play Mode.")]
    [SerializeField] private bool _debugPlayerInvulnerable;

    public event Action Defeated;
    // Invoked with the index of the stage that just ended, when the boss takes a hit (also for the final hit)
    public event Action<int> StageCleared;

    private State _state = State.Idle;
    private int _hits;
    private readonly List<CaveBossAttackTentacle> _attackTentacles = new List<CaveBossAttackTentacle>();
    private Material[] _originalMaterials;
    private Vector3 _visualStartPosition;
    private Coroutine _attackRoutine;
    private Coroutine _rumbleRoutine;
    private EventInstance _rumbleInstance;
    private Coroutine _continuousRumbleRoutine;
    private EventInstance _continuousRumbleInstance;
    private bool _isStageStartHeld;
    private float _tentacleHitWeight;
    private bool _isTentacleWiggleDefeated;
    private float _currentTentacleWiggleSpeed = NormalWiggleSpeed;

    private Stage CurrentStage => _stages[Mathf.Min(_hits, _stages.Length - 1)];

    void Awake() {
        _originalMaterials = new Material[_flashRenderers.Length];
        for (int i = 0; i < _flashRenderers.Length; i++)
            _originalMaterials[i] = _flashRenderers[i].sharedMaterial;
        _visualStartPosition = _visual.localPosition;

        Vector2 body = transform.position;
        foreach (var slot in _tentacles)
            if (slot.target != null) slot.offset = (Vector2)slot.target.position - body;

        CaveBossOrb.SetGrabbingEnabled(false);
        ApplyDebugPlayerInvulnerability();
    }

    void Update() {
        UpdateTentacleWiggleSpeed();
        UpdateTentacles();
    }

    // Fades the hit wiggle spike, holds the defeated speed once set, and applies the smoothed result to the static tentacles
    private void UpdateTentacleWiggleSpeed() {
        _tentacleHitWeight = Mathf.Max(0f, _tentacleHitWeight - Time.deltaTime / _tentacleHitWiggleFadeDuration);
        float targetSpeed = Mathf.Lerp(NormalWiggleSpeed, _tentacleHitWiggleSpeed, Mathf.SmoothStep(0f, 1f, _tentacleHitWeight));
        if (_isTentacleWiggleDefeated) targetSpeed = Mathf.Max(targetSpeed, _tentacleDefeatedWiggleSpeed);
        // Smoothed so the wiggle eases into the faster speed instead of snapping
        float response = 1f - Mathf.Exp(-_tentacleWiggleResponse * Time.deltaTime);
        _currentTentacleWiggleSpeed = Mathf.Lerp(_currentTentacleWiggleSpeed, targetSpeed, response);
        foreach (var slot in _tentacles)
            if (slot.tentacle != null) slot.tentacle.SetWiggleSpeed(_currentTentacleWiggleSpeed);
    }

    // Keeps each static tentacle target at its offset from the body and points the tentacle at it
    private void UpdateTentacles() {
        Vector2 body = transform.position;
        foreach (var slot in _tentacles) {
            if (slot.target == null) continue;
            slot.target.position = new Vector3(body.x + slot.offset.x, body.y + slot.offset.y, slot.target.position.z);
            if (slot.tentacle != null) slot.tentacle.SetTarget(slot.target.position);
        }
    }

    // Lets the debug toggle be changed from the Inspector during Play Mode
    void OnValidate() {
        if (Application.isPlaying) ApplyDebugPlayerInvulnerability();
    }

    // Updates every kill trigger under the boss (guard tentacles, spawned attack tentacles, body)
    private void ApplyDebugPlayerInvulnerability() {
        foreach (var killTrigger in GetComponentsInChildren<KillTrigger>(true))
            killTrigger.CanKillPlayer = !_debugPlayerInvulnerable;
    }

    void OnDisable() {
        AudioUtils.SafeStop(ref _rumbleInstance);
        StopContinuousRumble();
    }

    void OnDestroy() {
        _visual.DOKill();
    }

    public bool IsHurtbox(Collider2D other) => other == _hurtbox;

    public void PlaceAtFightPosition() {
        SetX(_fightPosition.position.x);
    }

    public IEnumerator Enter() {
        _state = State.Entering;
        yield return MoveToX(_fightPosition.position.x, _enterSpeed);
        _state = State.Idle;
    }

    public void StartFighting(float firstAttackDelay) {
        _state = State.Fighting;
        ActivateStage(_hits);
        CaveBossOrb.SetGrabbingEnabled(true);
        _attackRoutine = StartCoroutine(AttackLoop(firstAttackDelay));
        if (_rumbleRoutine == null) _rumbleRoutine = StartCoroutine(RumbleLoop());
    }

    // Returns false if the boss can't be hit right now
    public bool TryTakeHit() {
        if (_state != State.Fighting) return false;
        StartCoroutine(HitReaction());
        return true;
    }

    /// <summary>
    /// Keeps the next stage from starting, and the boss from attacking, after the current hit reaction until ReleaseStageStart is called.
    /// Call it from StageCleared, e.g. while the room changes for the next stage.
    /// </summary>
    public void HoldStageStart() {
        _isStageStartHeld = true;
    }

    /// <summary>Lets the next stage start after HoldStageStart. Starts right away if the hit reaction is already over.</summary>
    public void ReleaseStageStart() {
        _isStageStartHeld = false;
    }

    /// <summary>Starts the rumble (camera shake and earthquake sound) and keeps it going until StopContinuousRumble is called.</summary>
    public void StartContinuousRumble() {
        if (_continuousRumbleRoutine != null) return;
        _continuousRumbleInstance = SoundFXManager.obj.CreateAttachedInstance(_earthquakeStinger, gameObject);
        _continuousRumbleRoutine = StartCoroutine(ContinuousRumble());
    }

    /// <summary>Stops the rumble started with StartContinuousRumble. The camera shake ends shortly after and the sound fades out.</summary>
    public void StopContinuousRumble() {
        if (_continuousRumbleRoutine != null) {
            StopCoroutine(_continuousRumbleRoutine);
            _continuousRumbleRoutine = null;
        }
        AudioUtils.SafeStop(ref _continuousRumbleInstance);
    }

    private IEnumerator ContinuousRumble() {
        _continuousRumbleInstance.start();
        bool hasSoundStartedPlaying = false;
        while (true) {
            CameraShakeManager.obj.ShakeCamera(_rumbleShakeAmplitude, _rumbleShakeFrequency, ContinuousRumbleShakeRefreshDuration);
            // Restarts the sound if it's a one-shot that ended before the rumble was stopped.
            // Only once it has been playing, since the playback state can still read as stopped on the frames right after start
            bool isSoundPlaying = AudioUtils.IsPlaying(_continuousRumbleInstance);
            if (isSoundPlaying) hasSoundStartedPlaying = true;
            else if (hasSoundStartedPlaying) {
                _continuousRumbleInstance.start();
                hasSoundStartedPlaying = false;
            }
            yield return null;
        }
    }

    private IEnumerator HitReaction() {
        _state = State.HitReaction;
        _hits++;
        bool defeated = _hits >= _stages.Length;
        if (defeated) CaveBossOrb.SetGrabbingEnabled(false);
        StageCleared?.Invoke(_hits - 1);

        // The final hit keeps the static tentacles wiggling fast until the boss despawns
        _tentacleHitWeight = 1f;
        if (defeated) _isTentacleWiggleDefeated = true;

        if (_attackRoutine != null) StopCoroutine(_attackRoutine);
        _attackTentacles.RemoveAll(t => t == null);
        foreach (var tentacle in _attackTentacles) tentacle.Retract();

        StartCoroutine(Flash());
        if (_floatingParts != null) _floatingParts.Agitate();
        Shake(_hitShakeStrength, _hitReactionDuration, false);
        yield return new WaitForSeconds(_hitReactionDuration);

        if (defeated) {
            yield return Retreat();
            yield break;
        }

        while (_isStageStartHeld)
            yield return null;

        ActivateStage(_hits);
        _state = State.Fighting;
        _attackRoutine = StartCoroutine(AttackLoop(GetAttackInterval()));
    }

    private IEnumerator Retreat() {
        _state = State.Retreating;
        if (_floatingParts != null) _floatingParts.SetDefeated();
        foreach (var stage in _stages)
            foreach (var guard in stage.guardTentacles) guard.Retract();

        Shake(_retreatShakeStrength, 1f, true);
        yield return MoveToX(transform.position.x - _retreatDistance, _retreatSpeed);
        Defeated?.Invoke();
        Destroy(gameObject);
    }

    private IEnumerator AttackLoop(float firstDelay) {
        yield return new WaitForSeconds(firstDelay);
        while (true) {
            SpawnAttack();
            yield return new WaitForSeconds(GetAttackInterval());
        }
    }

    private void SpawnAttack() {
        Vector2 target = ApplyAttackRange(GetAttackTarget());
        Vector3 root = new Vector3(_attackRoot.position.x, target.y, _attackRoot.position.z);
        CaveBossAttackTentacle tentacle = Instantiate(_attackTentaclePrefab, root, Quaternion.identity, transform);
        _attackTentacles.RemoveAll(t => t == null);
        _attackTentacles.Add(tentacle);
        foreach (var killTrigger in tentacle.GetComponentsInChildren<KillTrigger>(true))
            killTrigger.CanKillPlayer = !_debugPlayerInvulnerable;
        tentacle.Launch(target);
    }

    // The orb in play if there is one, otherwise the active player
    private Vector2 GetAttackTarget() {
        if (CaveBossOrb.InPlay != null) return CaveBossOrb.InPlay.transform.position;
        Transform player = PlayerManager.obj.GetPlayerTransform(PlayerManager.obj.GetActivePlayerType());
        Collider2D collider = player.GetComponent<Collider2D>();
        return collider != null ? (Vector2)collider.bounds.center : (Vector2)player.position;
    }

    // Keeps the target if it's within the current stage's attack range, otherwise attacks at the bottom of the range.
    // Only the height is limited, the tentacle still lunges towards the target's X.
    private Vector2 ApplyAttackRange(Vector2 target) {
        Stage stage = CurrentStage;
        if (!stage.HasAttackRange) return target;
        float minY = Mathf.Min(stage.attackRangeMinY.position.y, stage.attackRangeMaxY.position.y);
        float maxY = Mathf.Max(stage.attackRangeMinY.position.y, stage.attackRangeMaxY.position.y);
        if (target.y >= minY && target.y <= maxY) return target;
        return new Vector2(target.x, minY);
    }

    private float GetAttackInterval() {
        return Random.Range(CurrentStage.minAttackInterval, CurrentStage.maxAttackInterval);
    }

    private void ActivateStage(int index) {
        if (index >= _stages.Length) return;
        foreach (var guard in _stages[index].guardTentacles) guard.Activate();
    }

    private IEnumerator Flash() {
        for (int i = 0; i < _flashCount; i++) {
            SetFlash(true);
            yield return new WaitForSeconds(_flashDuration);
            SetFlash(false);
            yield return new WaitForSeconds(_flashDuration);
        }
    }

    private void SetFlash(bool on) {
        for (int i = 0; i < _flashRenderers.Length; i++)
            _flashRenderers[i].sharedMaterial = on ? _flashMaterial : _originalMaterials[i];
    }

    private void Shake(float strength, float duration, bool loop) {
        _visual.DOKill();
        _visual.localPosition = _visualStartPosition;
        Tween shake = _visual.DOShakePosition(duration, new Vector3(strength, strength * 0.5f, 0f), _shakeVibrato, 90f, false, false)
            .OnComplete(() => _visual.localPosition = _visualStartPosition);
        if (loop) shake.SetLoops(-1);
    }

    private IEnumerator MoveToX(float x, float speed) {
        while (transform.position.x != x) {
            SetX(Mathf.MoveTowards(transform.position.x, x, speed * Time.deltaTime));
            yield return null;
        }
    }

    private void SetX(float x) {
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
        // Movement runs in coroutines after Update, so refresh the targets right away
        UpdateTentacles();
    }

    // Only counts down while fighting
    private IEnumerator RumbleLoop() {
        while (true) {
            float wait = Random.Range(_rumbleMinInterval, _rumbleMaxInterval);
            for (float t = 0f; t < wait; t += _state == State.Fighting ? Time.deltaTime : 0f)
                yield return null;
            CameraShakeManager.obj.ShakeCamera(_rumbleShakeAmplitude, _rumbleShakeFrequency, _rumbleDuration);
            _rumbleInstance = SoundFXManager.obj.CreateAttachedInstance(_earthquakeStinger, gameObject, null);
            _rumbleInstance.start();
            _rumbleInstance.release();
            yield return new WaitForSeconds(_rumbleDuration);
            AudioUtils.SafeStop(ref _rumbleInstance);
        }
    }
}
