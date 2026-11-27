using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager obj;

    //State carried over from the current character to the new one when switching character, to make the switch seamless
    public struct SwitchState {
        public Vector2 velocity;
        public bool isGrounded;
        public bool isOnMoveable;
        public Rigidbody2D moveableRigidbody;
        public bool jumpHeld;
        public bool endedJumpEarly;
        public Vector2 movementInput;
        //Base layer animator state (0 when not transferable) and its normalized time, so the new character continues the same animation
        public int animatorStateHash;
        public float animatorNormalizedTime;
    }

    private const int ANIMATOR_BASE_LAYER = 0;

    //Movement states that exist with the same name in both Eli's (Player.controller) and Dee's (ShadowTwinPlayer.controller) animators.
    //Only these are carried over on a switch; anything else (powers, cutscene states) falls back to the animator parameters
    private static readonly int[] TRANSFERABLE_ANIMATOR_STATES = {
        Animator.StringToHash("main_character_with_cape_idle"),
        Animator.StringToHash("start_running"),
        Animator.StringToHash("main_character_with_cloak_running"),
        Animator.StringToHash("stop_running"),
        Animator.StringToHash("turn_around"),
        Animator.StringToHash("main_character_with_cape_jump"),
        Animator.StringToHash("jump_to_fall"),
        Animator.StringToHash("falling"),
        Animator.StringToHash("fall_to_land"),
        Animator.StringToHash("jump_to_land"),
    };

    /// <summary>Stores the outgoing character's base layer animation state in the switch state, if it's a transferable movement state.</summary>
    public static void CaptureAnimatorState(Animator animator, ref SwitchState state) {
        state.animatorStateHash = 0;
        state.animatorNormalizedTime = 0f;
        if(animator == null || !animator.isActiveAndEnabled)
            return;
        //Mid transition, the destination state is what the player is about to see
        AnimatorStateInfo stateInfo = animator.IsInTransition(ANIMATOR_BASE_LAYER)
            ? animator.GetNextAnimatorStateInfo(ANIMATOR_BASE_LAYER)
            : animator.GetCurrentAnimatorStateInfo(ANIMATOR_BASE_LAYER);
        if(System.Array.IndexOf(TRANSFERABLE_ANIMATOR_STATES, stateInfo.shortNameHash) < 0)
            return;
        state.animatorStateHash = stateInfo.shortNameHash;
        //Looping states keep their cycle phase. Finished one-shot states stay on their last frame
        state.animatorNormalizedTime = stateInfo.loop ? Mathf.Repeat(stateInfo.normalizedTime, 1f) : Mathf.Min(stateInfo.normalizedTime, 1f);
    }

    /// <summary>
    /// Puts the incoming character's animator directly into the captured state at the same time, instead of starting from idle.
    /// Call after the animator parameters are updated. Does nothing if no state was captured or the animator lacks it.
    /// </summary>
    public static void RestoreAnimatorState(Animator animator, SwitchState state) {
        if(animator == null || state.animatorStateHash == 0 || !animator.HasState(ANIMATOR_BASE_LAYER, state.animatorStateHash))
            return;
        animator.Play(state.animatorStateHash, ANIMATOR_BASE_LAYER, state.animatorNormalizedTime);
        //Evaluate now so the first rendered frame already shows the new pose
        animator.Update(0f);
    }

    [Tooltip("Minimum time in seconds after a twin switch before an automatic switch (using the other twin's power) is allowed again. Prevents spamming auto switches.")]
    [SerializeField] private float _autoSwitchCooldown = 0.2f;
    private float _lastTwinSwitchTime = float.MinValue;

    /// <summary>Records that the active twin just changed. Starts the automatic switch cooldown.</summary>
    public void RegisterTwinSwitch() {
        _lastTwinSwitchTime = Time.time;
    }

    /// <summary>True when an automatic switch is allowed: single player, merged twins, merged switching unlocked and the cooldown has passed.</summary>
    public bool CanAutoSwitch() {
        return !IsCoopActive
            && !IsSeparated
            && PlayerPowersManager.obj.CanSwitchBetweenTwinsMerged
            && Time.time - _lastTwinSwitchTime >= _autoSwitchCooldown;
    }

    //Reads the actual button state of an action's controls. Used when an action was already held when its PlayerInput got enabled,
    //since button actions don't do an initial state check and therefore won't fire canceled when released
    public static bool IsActionPressed(PlayerInput playerInput, string actionName) {
        InputAction action = playerInput != null && playerInput.currentActionMap != null ? playerInput.currentActionMap.FindAction(actionName) : null;
        if(action == null)
            return false;
        foreach(InputControl control in action.controls) {
            if(control.IsPressed())
                return true;
        }
        return false;
    }

    /// <summary>True if the action on an enabled PlayerInput was performed this frame. Used to poll the other twin's power actions for automatic switching.</summary>
    public static bool WasActionPerformedThisFrame(PlayerInput playerInput, string actionName) {
        if(playerInput == null || !playerInput.enabled || playerInput.currentActionMap == null)
            return false;
        InputAction action = playerInput.currentActionMap.FindAction(actionName);
        return action != null && action.enabled && action.WasPerformedThisFrame();
    }

    public enum PlayerDirection {
        RIGHT,
        LEFT,
        UP,
        DOWN,
        NO_DIRECTION
    }

    public enum PlayerType {
        HUMAN,
        BLOB,
        SHADOW_TWIN, 
        NONE
    }

    public bool IsSeparated = false;
    public bool IsCoopActive = false;

    public PlayerType elisLastForm = PlayerType.HUMAN;

    public PlayerType GetPlayerTypeFromCollision(Collision2D collision) {
        PlayerIdentity playerIdentity = collision.gameObject.GetComponent<PlayerIdentity>();
        if(playerIdentity.id == 1) {
            return PlayerType.HUMAN;
        } else if(playerIdentity.id == 2) {
            return PlayerType.SHADOW_TWIN;
        } else if(playerIdentity.id == 3) {
            return PlayerType.BLOB;
        } else {
            Debug.Log("Unknown player identity id: " + playerIdentity.id);
            return PlayerType.HUMAN;
        }
    }

    public PlayerType GetPlayerTypeFromCollider(Collider2D collider) {
        PlayerIdentity playerIdentity = collider.gameObject.GetComponent<PlayerIdentity>();
        if(playerIdentity.id == 1) {
            return PlayerType.HUMAN;
        } else if(playerIdentity.id == 2) {
            return PlayerType.SHADOW_TWIN;
        } else if(playerIdentity.id == 3) {
            return PlayerType.BLOB;
        } else {
            Debug.Log("Unknown player identity id: " + playerIdentity.id);
            return PlayerType.HUMAN;
        }
    }
    
    public bool IsEliInBlobForm() {
        return elisLastForm == PlayerType.BLOB;
    }

    //Who turned into the blob. SHADOW_TWIN when Dee pressed down to turn into the blob, HUMAN for every other way into blob form
    private PlayerType _blobOwner = PlayerType.HUMAN;
    //Eli's form right before Dee turned into the blob, restored when that blob turns back into Dee
    private PlayerType _elisFormBeforeDeeBlob = PlayerType.HUMAN;

    /// <summary>Records that Eli turned into the blob (or the blob was restored for Eli). Turning back up returns to Eli.</summary>
    public void RegisterEliTurnedIntoBlob() {
        _blobOwner = PlayerType.HUMAN;
        elisLastForm = PlayerType.BLOB;
    }

    /// <summary>Records that Dee turned into the blob herself. Turning back up returns to Dee.</summary>
    public void RegisterDeeTurnedIntoBlob() {
        _elisFormBeforeDeeBlob = elisLastForm;
        _blobOwner = PlayerType.SHADOW_TWIN;
        elisLastForm = PlayerType.BLOB;
    }

    /// <summary>True when the current blob was made by Dee pressing down, so it should turn back into Dee.</summary>
    public bool IsBlobOwnedByDee() {
        return _blobOwner == PlayerType.SHADOW_TWIN;
    }

    /// <summary>Called when the blob turns back into Dee. Restores Eli's form from before Dee turned into the blob.</summary>
    public void RegisterBlobReturnedToDee() {
        if(_blobOwner == PlayerType.SHADOW_TWIN)
            elisLastForm = _elisFormBeforeDeeBlob;
        _blobOwner = PlayerType.HUMAN;
    }

    /// <summary>Called when the blob turns into human Eli. Clears any Dee ownership of the blob.</summary>
    public void RegisterBlobReturnedToEli() {
        _blobOwner = PlayerType.HUMAN;
        elisLastForm = PlayerType.HUMAN;
    }
    
    public void DisablePlayerMovement(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            PlayerMovement.obj.DisablePlayerMovement();
        else if(playerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.DisablePlayerMovement();
        else if(playerType == PlayerType.SHADOW_TWIN)
            ShadowTwinMovement.obj.DisablePlayerMovement();
    }

    public void EnablePlayerMovement(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            PlayerMovement.obj.EnablePlayerMovement();
        else if(playerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.EnablePlayerMovement();
        else if(playerType == PlayerType.SHADOW_TWIN)
            ShadowTwinMovement.obj.EnablePlayerMovement();
    }

    public bool IsFrozen(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            return PlayerMovement.obj.IsFrozen();
        else if(playerType == PlayerType.BLOB)
            return PlayerBlobMovement.obj.IsFrozen();
        else if(playerType == PlayerType.SHADOW_TWIN)
            return ShadowTwinMovement.obj.IsFrozen();
        return false;
    }

    public void KillPlayerGeneric(PlayerType playerType, float genericDeathAnimationTime) {
        if(playerType == PlayerType.HUMAN) {
            PlayerPush.obj.ResetBuiltUpPower();
            PlayerMovement.obj.Freeze(genericDeathAnimationTime);
            Player.obj.FadeOutPlayerLight();
            Player.obj.PlayGenericDeathAnimation();
        }
        else if(playerType == PlayerType.BLOB) {
            PlayerBlobMovement.obj.Freeze(genericDeathAnimationTime);
            PlayerBlob.obj.FadeOutPlayerLight();
            PlayerBlob.obj.PlayGenericDeathAnimation();
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinMovement.obj.EndLatchPull();
            ShadowTwinPull.obj.CancelPulling();
            ShadowTwinMovement.obj.Freeze(genericDeathAnimationTime);
            ShadowTwinPlayer.obj.FadeOutPlayerLight();
            ShadowTwinPlayer.obj.PlayGenericDeathAnimation();
            ShadowLashBeamManager.obj.StopBeam();
        }
    }

    public void KillAllPlayersGeneric(float genericDeathAnimationTime) {
        if(Player.obj != null && Player.obj.gameObject.activeSelf) {
            PlayerPush.obj.ResetBuiltUpPower();
            PlayerMovement.obj.Freeze(genericDeathAnimationTime);
            Player.obj.FadeOutPlayerLight();
            Player.obj.PlayGenericDeathAnimation();
        }
        if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf) {
            PlayerBlobMovement.obj.Freeze(genericDeathAnimationTime);
            PlayerBlob.obj.FadeOutPlayerLight();
            PlayerBlob.obj.PlayGenericDeathAnimation();
        }
        if(ShadowTwinPlayer.obj != null && ShadowTwinPlayer.obj.gameObject.activeSelf) {
            ShadowTwinMovement.obj.EndLatchPull();
            ShadowTwinPull.obj.CancelPulling();
            ShadowTwinMovement.obj.Freeze(genericDeathAnimationTime);
            ShadowTwinPlayer.obj.FadeOutPlayerLight();
            ShadowTwinPlayer.obj.PlayGenericDeathAnimation();
            ShadowLashBeamManager.obj.StopBeam();
        }
    }

    public void KillPlayerShadow(PlayerType playerType, float shadowDeathAnimationTime) {
        if(playerType == PlayerType.HUMAN) {
            PlayerPush.obj.ResetBuiltUpPower();
            PlayerMovement.obj.Freeze(shadowDeathAnimationTime);
            Player.obj.FadeOutPlayerLight();
            Player.obj.PlayShadowDeathAnimation();
        }
        else if(playerType == PlayerType.BLOB) {
            PlayerBlobMovement.obj.Freeze(shadowDeathAnimationTime);
            PlayerBlob.obj.FadeOutPlayerLight();
            PlayerBlob.obj.PlayGenericDeathAnimation();
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinMovement.obj.EndLatchPull();
            ShadowTwinPull.obj.CancelPulling();
            ShadowTwinMovement.obj.Freeze(shadowDeathAnimationTime);
            ShadowTwinPlayer.obj.FadeOutPlayerLight();
            ShadowTwinPlayer.obj.PlayShadowDeathAnimation();
            ShadowLashBeamManager.obj.StopBeam();
        }
    }

    public void SetPlayerGameObjectInactive(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            Player.obj.gameObject.SetActive(false);
        }
        else if(playerType == PlayerType.BLOB) {
            PlayerBlob.obj.gameObject.SetActive(false);
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinPlayer.obj.gameObject.SetActive(false);
        }
    }

    public void SetAllPlayerGameObjectsInactive() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
            Player.obj.gameObject.SetActive(false);
        }
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
            PlayerBlob.obj.gameObject.SetActive(false);
        }
        if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf) {
            ShadowTwinPlayer.obj.gameObject.SetActive(false);
        }
    }

    public void EnableAllPlayers() {
        //TODO might need to handle Eli/blob case here
        Player.obj.gameObject.SetActive(true);
        ShadowTwinPlayer.obj.gameObject.SetActive(true);
    }

    public void EnablePlayerGameObject(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            Player.obj.gameObject.SetActive(true);
        }
        else if(playerType == PlayerType.BLOB) {
            PlayerBlob.obj.gameObject.SetActive(true);
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinPlayer.obj.gameObject.SetActive(true);
        }
    }

    public PlayerType GetActivePlayerType() {
        if(PlayerSwitcher.obj.IsEliActive())
            return PlayerType.HUMAN;
        else if(PlayerSwitcher.obj.IsDeeActive())
            return PlayerType.SHADOW_TWIN;
        else if(PlayerSwitcher.obj.IsBlobActive())
            return PlayerType.BLOB;
        Debug.LogWarning("No active player type.");
        return PlayerType.NONE;
    }

    public void SetTransitioningBetweenLevels(PlayerType playerType) {
        switch(playerType)
        {
            case PlayerType.HUMAN: 
                PlayerMovement.obj.SetTransitioningBetweenLevels();
                break;
            case PlayerType.BLOB:
                PlayerBlobMovement.obj.SetTransitioningBetweenLevels();
                break;
            case PlayerType.SHADOW_TWIN:
                ShadowTwinMovement.obj.SetTransitioningBetweenLevels();
                break;
        }
    }

    public void TransitionToNextRoom(PlayerDirection direction, PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            PlayerMovement.obj.TransitionToNextRoom(direction);
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinMovement.obj.TransitionToNextRoom(direction);
        } else if(playerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.TransitionToNextRoom(direction);
    }

    public void TeleportSecondPlayerToTarget(PlayerType playerType, Collider2D target) {
        if(playerType == PlayerType.SHADOW_TWIN) {
            if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
                PlayerMovement.obj.TeleportToNextRoom(target);
            }
            else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
                PlayerBlobMovement.obj.TeleportToNextRoom(target);
            }
        } else if(playerType == PlayerType.HUMAN) {
            ShadowTwinMovement.obj.TeleportToNextRoom(target);
        }
    }

    public float GetPlayerVerticalVelocity(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            if(Player.obj != null && Player.obj.gameObject.activeSelf)
                return Player.obj.rigidBody.velocity.y;
        }
        else if(playerType == PlayerType.BLOB) {
            if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
                return PlayerBlob.obj.rigidBody.velocity.y;
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            if(ShadowTwinPlayer.obj != null && ShadowTwinPlayer.obj.gameObject.activeSelf)
                return ShadowTwinPlayer.obj.rigidBody.velocity.y;
        }
        return 0f;    
    }

    public Transform GetPlayerTransform(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            return Player.obj.transform;
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            return ShadowTwinPlayer.obj.transform;
        } else if(playerType == PlayerType.BLOB) {
            return PlayerBlob.obj.transform;
        } else {
            Debug.Log("Unknown player type: " + playerType);
            return null;
        }
    }

    public bool IsPlayerGrounded(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            return PlayerMovement.obj.isGrounded;
        } else if(playerType == PlayerType.BLOB) {
            return PlayerBlobMovement.obj.isGrounded;
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            return ShadowTwinMovement.obj.isGrounded;
        } else {
            Debug.Log("Unknown player type: " + playerType);
            return false;
        }
    }

    public bool IsPlayerFacingLeft(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            return PlayerMovement.obj.IsFacingLeft();
        } else if(playerType == PlayerType.BLOB) {
            return PlayerBlobMovement.obj.IsFacingLeft();
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            return ShadowTwinMovement.obj.IsFacingLeft();
        }
        return false;
    }

    public void FlipPlayer(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            PlayerMovement.obj.FlipPlayer();
        } else if(playerType == PlayerType.BLOB) {
            PlayerBlobMovement.obj.FlipPlayer();
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinMovement.obj.FlipPlayer();
        }
    }

    public void PlaySpawn(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            Player.obj.PlaySpawn();
        } else if(playerType == PlayerType.BLOB) {
            PlayerBlob.obj.PlaySpawn();
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinPlayer.obj.PlaySpawn();
        }
    }

    public Transform GetLeftAvatarTarget(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            return Player.obj.GetLeftAvatarTarget();
        else if(playerType == PlayerType.SHADOW_TWIN)
            return ShadowTwinPlayer.obj.GetLeftAvatarTarget();
        else if(playerType == PlayerType.BLOB)
            return PlayerBlob.obj.GetLeftAvatarTarget();
        return null;
    }

    public Transform GetRightAvatarTarget(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            return Player.obj.GetRightAvatarTarget();
        else if(playerType == PlayerType.SHADOW_TWIN)
            return ShadowTwinPlayer.obj.GetRightAvatarTarget();
        else if(playerType == PlayerType.BLOB)
            return PlayerBlob.obj.GetRightAvatarTarget();
        return null;
    }

    public void FreezePlayer(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            PlayerMovement.obj.Freeze();
        else if(playerType == PlayerType.SHADOW_TWIN)
            ShadowTwinMovement.obj.Freeze();
        else if(playerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.Freeze();
    }

    public void UnfreezePlayer(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            PlayerMovement.obj.UnFreeze();
        else if(playerType == PlayerType.SHADOW_TWIN)
            ShadowTwinMovement.obj.UnFreeze();
        else if(playerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.UnFreeze();
    }

    void Awake()
    {
        obj = this;
    }

    void OnDestroy()
    {
        obj = null;
    }
}
