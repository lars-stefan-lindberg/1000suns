using System.Collections;
using Cinemachine;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // --- Jump Kick Start fields ---
    // private bool _isJumpKickActive = false;
    // private float _jumpKickTimer = 0f;
    // public float _jumpKickDuration = 0.1f; // seconds
    // public float _jumpKickHorizontal = 4f; // tune as needed
    // private float _jumpKickDirection = 1f;
    public SurfaceTypeManager.SurfaceType surface = SurfaceTypeManager.SurfaceType.Default;
 
    [Header("Pixel Snap")]
    [SerializeField] private bool _snapHorizontalToPixelGrid = true;
    [SerializeField] private int _pixelsPerUnit = 8;
    // --------------------------------
    public static PlayerMovement obj;

    [SerializeField] private ScriptableStats _stats;
    [SerializeField] private GameObject _playerBlob;
    [SerializeField] private GameObject _playerTwin;
    [SerializeField] private GhostTrailManager _ghostTrail;
    [SerializeField] private GameObject _soulVfx;
    [SerializeField] private ParticleSystem _shadowJumpParticles;
    [SerializeField] private ParticleSystem _shadowJumpExplosionParticles;
    [SerializeField] private int _shadowJumpExplosionParticlesCount;
    
    public SpriteRenderer spriteRenderer;
    public GameObject anchor;
    private BoxCollider2D _collider;
    private Animator _animator;
    private PlayerInput _playerInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;
    private SharedCharacterAudio _sharedPlayerAudio;
    private EliAudio _eliAudio;

    // --- Quick Turn Detection fields ---
    // [SerializeField] private float _minSpeedForQuickTurn = 5f;   // units/sec threshold to consider the player "moving"
    // [SerializeField] private float _quickTurnDebounce = 0.15f;   // seconds to suppress duplicate logs
    // private float _lastQuickTurnTime = -100f;

    private float _time;
    private bool _jumpHeldInput; 
    private Vector2 _movementInput;

    public bool _isDashing = false;
    public float dashDecelerationTime = 160f;
    public float initialDashSpeed = 40f;

    //Moveables are used to move the player along with the moveable. Like if a floating platform or block is moving and the player is on top
    public bool isOnMoveable = false;
    public Rigidbody2D moveableRigidbody;
    public JumpThroughPlatform jumpThroughPlatform;
    public bool IsControlledProgrammatically = false;

    private bool _isWalking = false;
    [SerializeField] private float _walkSpeed = 2f;

    private void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _ceilingLayerMasks = LayerMask.GetMask(new[] { "Ground", "Default", "Block" });
        _playerInput = GetComponent<PlayerInput>();
        _sharedPlayerAudio = GetComponent<SharedCharacterAudio>();
        _eliAudio = GetComponent<EliAudio>();
    }

    private void OnDestroy()
    {
        obj = null;
    }

    private void OnEnable() {
        //Reset transform from any previous squeeze
        anchor.transform.localScale = Vector3.one;
    }

    private void FixedUpdate()
    {
        _roundedCeilingCornerThisFrame = false;
        
        if(!_stopCollisions)
            CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();

        if(_stopMovement) {
            _frameVelocity = new Vector2(0,0);
        }

        ApplyMovement();
    }

    private void Update()
    {
        _time += Time.deltaTime;
        UpdateAnimator();
        FlipPlayer(_movementInput.x);
        if (_mergeSplitHeld)
        {
            _mergeSplitHoldTimer += Time.deltaTime;
            if (_mergeSplitHoldTimer >= _mergeSplitHoldDuration)
            {
                _mergeSplitHeld = false;
                _mergeSplitHoldTimer = 0f;
                PerformMergeSplit();
            }
        }
        // Update jump kick timer
        //Note: disabled for now due to horizontal scrolling gets weird since camera is following player rigidly
        // if (_isJumpKickActive)
        // {
        //     if(_jumpKickDirection != _movementInput.x){
        //         _isJumpKickActive = false;
        //     } else {
        //         _jumpKickTimer -= Time.deltaTime;
        //         if (_jumpKickTimer <= 0f)
        //             _isJumpKickActive = false;
        //     }
        // }
    }

    private void LateUpdate()
    {
        if (_snapHorizontalToPixelGrid)
        {
            SnapHorizontalToPixelGrid();
        }
    }

    private void SnapHorizontalToPixelGrid()
    {
        if (_pixelsPerUnit <= 0)
            return;

        float step = 1f / _pixelsPerUnit;
        float worldX = transform.position.x;
        float snappedWorldX = Mathf.Round(worldX / step) * step;
        float deltaX = snappedWorldX - worldX;

        Transform visualTarget = anchor != null ? anchor.transform : (spriteRenderer != null ? spriteRenderer.transform : null);
        if (visualTarget == null)
            return;

        Vector3 localPos = visualTarget.localPosition;
        localPos.x = deltaX;
        visualTarget.localPosition = localPos;
    }

    private void FlipPlayer(float _xValue)
    {
        if (_xValue < 0)
            spriteRenderer.flipX = true;
        else if (_xValue > 0)
            spriteRenderer.flipX = false;
    }

    public void FlipPlayer()
    {
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    public bool IsFacingLeft()
    {
        return spriteRenderer.flipX;
    }

    public float _poweredDashMultiplier = 1.2f;
    public float partialDashPower = 35;
    public void ExecuteDash(PlayerPush.ChargePowerType chargePower)
    {
        //Reset potential force push jump
        _cameFromForcePushJump = false;
        isForcePushJumping = false;
        jumpedWhileForcePushJumping = false;
        
        _isDashing = true;
        float speed = 0;
        if(chargePower == PlayerPush.ChargePowerType.Powered) {
            speed = initialDashSpeed * _poweredDashMultiplier;
            Player.obj.SetHasPowerUp(false);
        } else if(chargePower == PlayerPush.ChargePowerType.Partial) {
            speed = partialDashPower;
        } else if(chargePower == PlayerPush.ChargePowerType.Full) {
            speed = initialDashSpeed;
        }
        _frameVelocity.x = IsFacingLeft() ? -speed : speed;
        _ghostTrail.ShowGhosts();
    }

    public void TriggerForcePushAnimation() {
        if(_movementInput.x != 0 && isGrounded)
            _animator.SetTrigger("forcePushWhileRunning");
        else
            _animator.SetTrigger("forcePush");
    }

    public void TriggerFallToKnees() {
        _animator.SetTrigger("fallToKnees");
    }

    private bool _isBreathingOnKnees = false;
    public void SetBreathingOnKnees(bool isBreathingOnKnees) {
        _animator.SetBool("isBreathingOnKnees", isBreathingOnKnees);
        _isBreathingOnKnees = isBreathingOnKnees;
    }

    public void ExecuteForcePushJump() {
        isForcePushJumping = true;
        forcePushJumpOnGroundTimer = 0;
        _cameFromForcePushJump = true;
        _frameVelocity.x = IsFacingLeft() ? -initialForcePushJumpSpeed : initialForcePushJumpSpeed;
        PlayerPush.obj.ResetBuiltUpPower();
        PlayerPush.obj.ExecuteForcePushVfx(PlayerPush.ChargePowerType.Powered);
        _ghostTrail.ShowGhosts();
    }

    public void ExecuteForcePushWithProjectile(PlayerPush.ChargePowerType chargePowerType) {
        if(!isGrounded) {
            if(chargePowerType == PlayerPush.ChargePowerType.Partial) {
                _frameVelocity.x = IsFacingLeft() ? partialForcePushPushBackSpeed : -partialForcePushPushBackSpeed;
            } else if(chargePowerType == PlayerPush.ChargePowerType.Full) {
                _frameVelocity.x = IsFacingLeft() ? normalForcePushPushBackSpeed : -normalForcePushPushBackSpeed;
            } else if(chargePowerType == PlayerPush.ChargePowerType.Powered) {
                _frameVelocity.x = IsFacingLeft() ? fullyPoweredForcePushPushBackSpeed : -fullyPoweredForcePushPushBackSpeed;
                Player.obj.SetHasPowerUp(false);
            }
        } else if(isGrounded) {
            if(chargePowerType == PlayerPush.ChargePowerType.Powered) {
                _frameVelocity.x = IsFacingLeft() ? fullyPoweredForcePushGroundedPushBackSpeed : -fullyPoweredForcePushGroundedPushBackSpeed;
                Player.obj.SetHasPowerUp(false);
            } else if(chargePowerType == PlayerPush.ChargePowerType.Full) {
                _frameVelocity.x = IsFacingLeft() ? partialForcePushPushBackSpeed : -partialForcePushPushBackSpeed;
            }
        }
    }

    public bool isFalling = false;
    public bool isMoving = false;
    // Treat player as moving if velocity exceeds a tiny epsilon
    //[SerializeField] private float _movingVelocityEpsilon = 0.05f;
    private bool _cameFromForcePushJump = false;
    private void UpdateAnimator()
    {
        _animator.SetBool("isDashing", _isDashing);
        _animator.SetBool("isGrounded", isGrounded);
        // Keep moving during short grace to avoid triggering stop animation on quick direction changes
        // Velocity is not enough to check though, since player can have velocity, but there's no movement input
        //isMoving = Mathf.Abs(Player.obj.rigidBody.velocity.x) > _movingVelocityEpsilon || _movementInput.x != 0;
        isMoving = _movementInput.x != 0;
        _animator.SetBool("isMoving", isMoving);
        _animator.SetBool("isWalking", _isWalking);
        isFalling = _frameVelocity.y < -_stats.MinimumFallAnimationSpeed;
        _animator.SetBool("isFalling", isFalling);
        if (_landed)
        {
            DustParticleMgr.obj.CreateDust(PlayerManager.PlayerType.HUMAN);
            _sharedPlayerAudio.PlayLand(surface);
            if(_cameFromForcePushJump) {
                _eliAudio.PlayForcePushLand();
                _cameFromForcePushJump = false;
            }
            StartCoroutine(JumpSqueeze(_landedSqueezeX, _landedSqueezeY, _landedSqueezeTime));
            _landed = false;
        }
        _animator.SetBool("isForcePushJumping", _isShadowJumping);
    }

    private bool _freezePlayer = false;
    private bool _stopMovement = false;
    private bool _stopCollisions = false;
    public void Freeze(float freezeDuration) {
        DisablePlayerMovement();
        _freezePlayer = true;
        _isDashing = false; //Stop any dash when frozen
        _movementInput = new Vector2(0,0);
        StartCoroutine(FreezeDuration(freezeDuration));
    }

    public bool IsFrozen() {
        return _freezePlayer;
    }
    
    public void Freeze() {
        DisablePlayerMovement();
        _freezePlayer = true;
        _isDashing = false; //Stop any dash when frozen
        _movementInput = new Vector2(0,0);
    }

    public void SetIsBalancing(bool isBalancing) {
        _animator.SetTrigger("balance");
        _animator.SetBool("isBalancing", isBalancing);
        _isBalancing = isBalancing;
        
        if (isBalancing) {
            _balancingTimer = 0f;
            _balancingInBurst = true;
        }
    }

    public void StartWalking() {
        _animator.SetTrigger("walk");
        _animator.SetBool("isWalking", true);
        _isWalking = true;
    }

    public void StopWalking() {
        _animator.SetBool("isWalking", false);
        _isWalking = false;
    }

    public void UnFreeze() {
        _freezePlayer = false;
        EnablePlayerMovement();
    }

    private bool _isTransitioningBetweenLevels = false;
    public void SetTransitioningBetweenLevels() {
        //Special case since we want to handle "shoot" action separately. You should still be able to charge, but not release in between levels
        if(_playerInput != null && _playerInput.currentActionMap != null) {
            _playerInput.currentActionMap.FindAction("Movement").Disable();
            _playerInput.currentActionMap.FindAction("Jump").Disable();
        }
        _freezePlayer = true;
        _movementInput = new Vector2(0,0);
        
        _isTransitioningBetweenLevels = true;
        
        _stopMovement = true;
        _stopCollisions = true;
        Player.obj.rigidBody.gravityScale = 0;
        _animator.speed = 0;
    }

    public void TeleportToNextRoom(Collider2D target) {
        StartCoroutine(TeleportToNextRoomCoroutine(target));
    }

    private IEnumerator TeleportToNextRoomCoroutine(Collider2D target) {
        spriteRenderer.enabled = false;
        
        GameObject soul = Instantiate(_soulVfx, transform.position, transform.rotation);
        PrisonerSoul playerSoul = soul.GetComponent<PrisonerSoul>();
        playerSoul.Target = target.transform.position;
        while(!playerSoul.IsTargetReached) {
            yield return null;
        }
        Destroy(playerSoul.gameObject);
        transform.position = target.transform.position;
        SetStartingOnGround();
        isGrounded = true;
        spriteRenderer.enabled = true;

        EnablePlayerAfterLevelTransition();

        yield return null;
    }

    public void EnablePlayerAfterLevelTransition() {
        UnFreeze();
        Player.obj.rigidBody.gravityScale = 1;
        _animator.speed = 1;
        _stopMovement = false;
        _stopCollisions = false;

        _isTransitioningBetweenLevels = false;
    }

    public bool IsTransitioningBetweenLevels() {
        return _isTransitioningBetweenLevels;
    }

    public void TransitionToNextRoom(PlayerManager.PlayerDirection direction) {
        StartCoroutine(TransitionToNextRoomCoroutine(direction));
    }

    private float _transitionDistanceX = 1;
    private float _transitionDistanceUp = 3f;
    private float _transitionDistanceDown = 1.5f;
    [SerializeField] private float _levelTransitionMaxMoveTime = 1.25f; // safety timeout in seconds for move loops
    private IEnumerator TransitionToNextRoomCoroutine(PlayerManager.PlayerDirection direction) {
        float target = 0;
        if(direction == PlayerManager.PlayerDirection.LEFT || direction == PlayerManager.PlayerDirection.RIGHT) {
            if(direction == PlayerManager.PlayerDirection.RIGHT)
                target = transform.position.x + _transitionDistanceX;
            if(direction == PlayerManager.PlayerDirection.LEFT)
                target = transform.position.x - _transitionDistanceX;
            float startTime = Time.time;
            bool timedOut = false;
            while(!Mathf.Approximately(transform.position.x, target)) {
                transform.position = new Vector2(Mathf.MoveTowards(transform.position.x, target, Time.deltaTime * 5f), transform.position.y);
                if(Time.time - startTime > _levelTransitionMaxMoveTime) { 
                    timedOut = true; break; 
                }
                yield return null;
            }
            // Snap to target on success or timeout to ensure completion
            transform.position = new Vector2(target, transform.position.y);
            if(timedOut) Debug.LogWarning("TransitionToNextRoom horizontal move timed out; snapping to target.");
        } else if(direction == PlayerManager.PlayerDirection.UP || direction == PlayerManager.PlayerDirection.DOWN) {
            if(direction == PlayerManager.PlayerDirection.UP) {
                //"Hack" to make sure jump animation is played out while transitioning upwards. Before hack the character was just idle while being pushed upwards, and did not look good
                _animator.speed = 1;
                isGrounded = false;
                _isShadowJumping = false;
                target = transform.position.y + _transitionDistanceUp;
            }
            if(direction == PlayerManager.PlayerDirection.DOWN)
                target = transform.position.y - _transitionDistanceDown;
            float startTime = Time.time;
            bool timedOut = false;
            while(!Mathf.Approximately(transform.position.y, target)) {
                transform.position = new Vector2(transform.position.x, Mathf.MoveTowards(transform.position.y, target, Time.deltaTime * 5f));
                if(Time.time - startTime > _levelTransitionMaxMoveTime) { 
                    timedOut = true; break; 
                }
                yield return null;
            }
            // Snap to target on success or timeout to ensure completion
            transform.position = new Vector2(transform.position.x, target);
            if(timedOut) Debug.LogWarning("TransitionToNextRoom vertical move timed out; snapping to target.");
        }

        yield return new WaitForSeconds(0.5f);
        EnablePlayerAfterLevelTransition();
        yield return null;
    }

    [ContextMenu("Get new power")]
    public void SetNewPower() {
        _animator.SetTrigger("isNewPower");
        Freeze();
    }

    [ContextMenu("New power received")]
    public void SetNewPowerReceived() {
        _animator.SetTrigger("newPowerReceived");
    }

    public void DisablePlayerMovement() {
        if(_playerInput != null && _playerInput.currentActionMap != null) {
            _playerInput.currentActionMap.FindAction("Movement").Disable();
            _playerInput.currentActionMap.FindAction("Jump").Disable();
            _playerInput.currentActionMap.FindAction("Shoot").Disable();
        }
    }

    public void EnablePlayerMovement() {
        if(_playerInput != null && _playerInput.currentActionMap != null) {
            _playerInput.currentActionMap.FindAction("Movement").Enable();
            _playerInput.currentActionMap.FindAction("Jump").Enable();
            _playerInput.currentActionMap.FindAction("Shoot").Enable();
        }
    }

    private IEnumerator FreezeDuration(float freezeDuration) {
        yield return new WaitForSeconds(freezeDuration);
        _freezePlayer = false;
        EnablePlayerMovement();
    }

    public void SetMovementInput(Vector2 movementInput) {
        _freezePlayer = false;
        _movementInput = movementInput;
    }

    public Vector2 GetMovementInput() {
        return _movementInput;
    }

    public void SetPlayerInputDevice(PlayerSlot slot) {
        _playerInput.enabled = true;
        _playerInput.SwitchCurrentControlScheme(slot.device is Keyboard ? "Keyboard" : "Gamepad", slot.device);
    }

    public bool isTransformingToBlob = false;
    public void OnMovement(InputAction.CallbackContext value)
    {
        _movementInput = value.ReadValue<Vector2>();
        _movementInput.x = GetHorizontalInput(_movementInput.x);
        if (_stats.SnapInput)
        {
            _movementInput.y = Mathf.Abs(_movementInput.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_movementInput.y);
        }
        
        // Quick horizontal turn detection (based on input vs velocity and/or quick input sign flip)
        // Commenting this for now. Turn around animation didn't feel right, might revisit in the future
        // float currentSign = Mathf.Approximately(_movementInput.x, 0f) ? 0f : Mathf.Sign(_movementInput.x);
        // float velocityX = Player.obj.rigidBody.velocity.x;
        // float velSign = Mathf.Approximately(velocityX, 0f) ? 0f : Mathf.Sign(velocityX);
        // bool quickTurnTriggered = false;
        
        // // Pressing opposite direction to current velocity while moving fast enough
        // if (!quickTurnTriggered && currentSign != 0f && velSign != 0f && currentSign == -velSign && Mathf.Abs(velocityX) >= _minSpeedForQuickTurn)
        // {
        //     if (Time.time - _lastQuickTurnTime >= _quickTurnDebounce)
        //     {
        //         _animator.SetBool("isTurning", true);
        //         StartCoroutine(QuickTurnCancelCoroutine());
        //         _lastQuickTurnTime = Time.time;
        //     }
        // }
        
        if(PlayerPowersManager.obj.EliCanTurnFromHumanToBlob) {
            if(_movementInput.y < 0 && value.performed) {
                if(isTransformingToBlob)
                    return;
                _eliAudio.PlayShapeshiftToBlob();
                isTransformingToBlob = true;
                PlayerPush.obj.ResetBuiltUpPower();
                PlayerPush.obj.DisableChargeFor(0.2f);
                Player.obj.PlayToBlobAnimation();
            }
        }
    } 

    // private IEnumerator QuickTurnCancelCoroutine() {
    //     yield return new WaitForSeconds(0.05f);
    //     _animator.SetBool("isTurning", false);
    // }

    private float GetHorizontalInput(float originInput) {
        if(_stats.SnapInput) {
            // only flip when a strong push happens
            if (spriteRenderer.flipX)
            {
                if(originInput > _stats.HorizontalStrongDeadZoneThreshold) {
                    return 1;
                } else if(originInput < -_stats.HorizontalDeadZoneThreshold) {
                    return -1;
                } else {
                    return 0;
                }
            }
            else if (!spriteRenderer.flipX)
            {
                if(originInput < -_stats.HorizontalStrongDeadZoneThreshold) {
                    return -1;
                } else if(originInput > _stats.HorizontalDeadZoneThreshold) {
                    return 1;
                } else {
                    return 0;
                }
            }    
        } 
        return originInput;
    }
    

    public void NudgePlayer() {
        Player.obj.rigidBody.AddForce(new Vector2(-1000, 0));
    }

    public void ToBlob() {
        ICinemachineCamera activeVirtualCamera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if(activeVirtualCamera != null && activeVirtualCamera.Follow == transform) {
            activeVirtualCamera.Follow = _playerBlob.transform;
        }

        Player.obj.rigidBody.velocity = new Vector2(0, 0);
        _frameVelocity = new Vector2(0, 0);
        gameObject.SetActive(false);
        _playerBlob.transform.position = transform.position - new Vector3(0, 0.5f, 0);
        PlayerBlobMovement playerBlobMovement = _playerBlob.GetComponent<PlayerBlobMovement>();
        playerBlobMovement.spriteRenderer.flipX = IsFacingLeft();
        _playerBlob.SetActive(true);
        PlayerManager.obj.elisLastForm = PlayerManager.PlayerType.BLOB;
        PlayerSwitcher.obj.SwitchToBlob();
        if(isGrounded) {
            playerBlobMovement.SetStartingOnGround();
            playerBlobMovement.isGrounded = true;
        } else {
            playerBlobMovement.isGrounded = false;
        }
        if(IsFrozen()) {
            playerBlobMovement.Freeze();
        } else {
            playerBlobMovement.UnFreeze();
        }
        isTransformingToBlob = false;
    }

    public void ToTwin() {
        ICinemachineCamera activeVirtualCamera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if(activeVirtualCamera != null && activeVirtualCamera.Follow == transform) {
            activeVirtualCamera.Follow = _playerTwin.transform;
        }

        Player.obj.rigidBody.velocity = new Vector2(0, 0);
        _frameVelocity = new Vector2(0, 0);
        gameObject.SetActive(false);
        _playerTwin.transform.position = transform.position;
        _playerTwin.GetComponent<ShadowTwinMovement>().spriteRenderer.flipX = IsFacingLeft();
        _playerTwin.SetActive(true);
        PlayerSwitcher.obj.SwitchToDee();
        if(isGrounded) {
            _playerTwin.GetComponent<ShadowTwinMovement>().SetStartingOnGround();
            _playerTwin.GetComponent<ShadowTwinMovement>().isGrounded = true;
        } else {
            _playerTwin.GetComponent<ShadowTwinMovement>().isGrounded = false;
        }
        if(IsFrozen()) {
            _playerTwin.GetComponent<ShadowTwinMovement>().Freeze();
        } else {
            _playerTwin.GetComponent<ShadowTwinMovement>().UnFreeze();
        }
        ShadowTwinPull.obj.EnablePull();
        isTransformingToTwin = false;
    }

    public void Split(Vector3 splitTarget) {
        ICinemachineCamera activeVirtualCamera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if(activeVirtualCamera != null && activeVirtualCamera.Follow == transform) {
            activeVirtualCamera.Follow = _playerTwin.transform;
        }

        Player.obj.rigidBody.velocity = new Vector2(0, 0);
        _frameVelocity = new Vector2(0, 0);
        _playerTwin.transform.position = splitTarget;
        
        _playerTwin.GetComponent<ShadowTwinMovement>().spriteRenderer.flipX = IsFacingLeft();
        if(isGrounded) {
            _playerTwin.GetComponent<ShadowTwinMovement>().SetStartingOnGround();
            _playerTwin.GetComponent<ShadowTwinMovement>().isGrounded = true;
        } else {
            _playerTwin.GetComponent<ShadowTwinMovement>().isGrounded = false;
        }
        _playerTwin.SetActive(true);

        //Need to reset animator. For some reason it starts playing jump animation
        ShadowTwinPlayer.obj.ResetAnimator();
        
        PlayerSwitcher.obj.SwitchToDee();
        
        ShadowTwinPull.obj.EnablePull();

        PlayerManager.obj.IsSeparated = true;
    }

    public bool IsHorizontalInput() {
        return _movementInput.x != 0;
    }

    public bool GetJumpHeldInput() {
        return _jumpHeldInput;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_isBalancing) return;
            
            if(jumpThroughPlatform != null &&
                _movementInput.y < 0) {
                jumpThroughPlatform.PassThrough();
                return;
            }
            
            if(PlayerPowersManager.obj.EliCanShadowJump && _movementInput.x != 0 && PlayerPush.obj.IsChargedEnoughForShadowJump(_stats.ShadowJumpChargeMargin) && (isGrounded || CanUseCoyote)) {
                ExecuteShadowJump();
                _jumpHeldInput = true;
                return;
            }
            if(PlayerPowersManager.obj.EliCanForcePushJump && _movementInput.x != 0 && PlayerPush.obj.IsChargedEnoughForShadowJump(_stats.ShadowJumpChargeMargin) && (isGrounded || CanUseCoyote)) {
                _jumpToConsume = true;
                _isDashing = false;
                ExecuteForcePushJump();
            }
            if (isGrounded || CanUseCoyote) {
                _jumpToConsume = true;
            }
            else
            {
                _airJumpToConsume = true;
            }
            _jumpHeldInput = true;
            _timeJumpWasPressed = _time;
        }
        else if (context.canceled)
        {
            _jumpHeldInput = false;
        }
    }

    public bool isTransformingToTwin = false;
    public void OnSwitch(InputAction.CallbackContext context)
    {
        if(PlayerManager.obj.IsCoopActive) {
            return;
        }
        if (context.performed)
        {
            HandleSwitchCharacter();
        }
    }

    [SerializeField] private float _mergeSplitHoldDuration = 0.5f;
    private bool _mergeSplitHeld = false;
    private float _mergeSplitHoldTimer = 0f;
    private EventInstance _mergeSplitSfxInstance;

    public void OnMergeSplit(InputAction.CallbackContext context)
    {
        if(PlayerManager.obj.IsCoopActive) {
            return;
        }
        if(!PlayerPowersManager.obj.CanSeparate) {
            return;
        }
        if (context.started)
        {
            //Check if twin is close enough to merge
            if(PlayerManager.obj.IsSeparated && !CloseEnoughToMerge()) {
                HandleSwitchCharacter();
                return;
            }
            _mergeSplitHeld = true;
            _mergeSplitHoldTimer = 0f;
            _sharedPlayerAudio.PlayMergeSplit(ref _mergeSplitSfxInstance);
            Player.obj.StartChargeFlash();
        }
        else if (context.canceled)
        {
            _mergeSplitHeld = false;
            _mergeSplitHoldTimer = 0f;

            //Only abort flash and sfx if eli is active. If not, the split happened and we want to finish the vfx and sfx
            if(PlayerSwitcher.obj.IsEliActive()) {
                if(AudioUtils.IsPlaying(_mergeSplitSfxInstance)) {
                    AudioUtils.SafeStop(ref _mergeSplitSfxInstance);
                }
                Player.obj.AbortFlash();
            }
        }
    }

    private bool CloseEnoughToMerge() {
        return isGrounded && _playerTwin.GetComponent<ShadowTwinMovement>().isGrounded && Vector3.Distance(_playerTwin.transform.position, transform.position) <= 1.5f;
    }

    private void HandleSwitchCharacter()
    {
        if(isTransformingToTwin)
            return;

        if(PlayerPowersManager.obj.CanSwitchBetweenTwinsMerged && !PlayerManager.obj.IsSeparated)
        {    
            //Switch "form" to shadow twin
            _sharedPlayerAudio.PlayShapeshift();
            isTransformingToTwin = true;
            PlayerPush.obj.ResetBuiltUpPower();
            PlayerPush.obj.DisableCharge();
            Player.obj.PlayToShadowTwinAnimation();
        }
        else if(PlayerPowersManager.obj.CanSeparate) {
            StartCoroutine(SwitchToDee());
        }
    }

    private IEnumerator SwitchToDee() {
        //Disable all controls
        PlayerSwitcher.obj.DisableAll();
        GameObject soul = Instantiate(_soulVfx, transform.position, transform.rotation);
        PrisonerSoul prisonerSoul = soul.GetComponent<PrisonerSoul>();
        prisonerSoul.Target = _playerTwin.transform.position;
        while (!prisonerSoul.IsTargetReached) {
            yield return null;
        }
        ShadowTwinPlayer.obj.FlashOnce();
        if(ShadowTwinPull.obj.HoldPull) {
            ShadowTwinPull.obj.HoldPull = false;  //Reset any current pulling
            ShadowTwinPull.obj.OnShootButtonCanceled();
        }
        PlayerSwitcher.obj.SwitchToDee();
        Destroy(soul);
        yield return null;
    }

    private void PerformMergeSplit()
    {
        if(PlayerPowersManager.obj.CanSeparate) {
            if(PlayerManager.obj.IsSeparated) {
                StartCoroutine(MergeVfx());
            } else {
                Vector3 splitTarget;
                if(IsFacingLeft()) {
                    splitTarget = transform.position + new Vector3(-1, 0, 0);
                } else {
                    splitTarget = transform.position + new Vector3(1, 0, 0);
                }
                StartCoroutine(SplitVfx(splitTarget));
            }
        }
    }

    private IEnumerator MergeVfx() {
        _playerTwin.SetActive(false);
        GameObject soul = Instantiate(_soulVfx, _playerTwin.transform.position, _playerTwin.transform.rotation);
        PrisonerSoul prisonerSoul = soul.GetComponent<PrisonerSoul>();
        prisonerSoul.Target = transform.position;
        while (!prisonerSoul.IsTargetReached) {
            yield return null;
        }
        PlayerManager.obj.IsSeparated = false;
        Destroy(soul);
        yield return null;
    }

    private IEnumerator SplitVfx(Vector3 target) {
        GameObject soul = Instantiate(_soulVfx, transform.position, transform.rotation);
        PrisonerSoul prisonerSoul = soul.GetComponent<PrisonerSoul>();
        prisonerSoul.Target = target;
        while (!prisonerSoul.IsTargetReached) {
            yield return null;
        }
        Split(target);
        Destroy(soul);
        yield return null;
    }

    public void CancelJumping() {
        _jumpToConsume = false;
        _isShadowJumping = false;
    }

    private IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds)
    {
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        float time = 0f;
        while (time <= 1.0)
        {
            time += Time.deltaTime / seconds;
            anchor.transform.localScale = Vector3.Lerp(originalSize, newSize, time);
            yield return null;
        }
        time = 0f;
        while(time <= 1.0)
        {
            time += Time.deltaTime / seconds;
            anchor.transform.localScale = Vector3.Lerp(newSize, originalSize, time);
            yield return null;
        }
    }

    #region Collisions

    private float _frameLeftGrounded = float.MinValue;
    public bool isGrounded;
    private bool _isBalancing = false;
    
    [Header("Balancing Movement")]
    [SerializeField] private float _balancingMoveSpeed = 2f;
    [SerializeField] private float _balancingBurstDuration = 0.3f;
    [SerializeField] private float _balancingPauseDuration = 0.2f;
    private float _balancingTimer = 0f;
    private bool _balancingInBurst = true;
    
    public bool startingOnGround = true;
    private float _landedSqueezeX = 1.25f;
    private float _landedSqueezeY = 0.65f;
    private float _landedSqueezeTime = 0.08f;
    private bool _landed = false;
    [SerializeField] private LayerMask _groundLayerMasks;
    private LayerMask _moveableLayerMasks;
    private LayerMask _ceilingLayerMasks;
    private bool _roundedCeilingCornerThisFrame = false;

    private bool _startingOnGroundFalseCoroutineStarted;
    private IEnumerator SetStartingOnGroundToFalse() {
        yield return new WaitForSeconds(0.1f);
        startingOnGround = false;
    }

    public void SetStartingOnGround() {
        startingOnGround = true;
        _startingOnGroundFalseCoroutineStarted = false;
    }

    public void DisableCollider() {
        _collider.enabled = false;
    }

    public void EnableCollider() {
        _collider.enabled = true;
    }

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        RaycastHit2D groundRaycastResult = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.down, _stats.GrounderDistance, _groundLayerMasks);
        bool groundHit = groundRaycastResult.collider != null;

        if(groundHit) {
            surface = SurfaceTypeManager.GetSurfaceType(groundRaycastResult.collider.gameObject.tag);
            if(!isOnMoveable) {
                Moveable moveable = groundRaycastResult.collider.GetComponent<Moveable>();
                if(moveable != null) {
                    isOnMoveable = true;
                    moveableRigidbody = moveable.GetRigidbody();
                    if(groundRaycastResult.collider.gameObject.CompareTag("FloatingPlatform")) {
                        PlayerPush.obj.platform = groundRaycastResult.collider.gameObject.GetComponentInParent<FloatyPlatform>();
                    }
                }
            }
        }
        
        //Corner case when spawning
        if(startingOnGround) {
            groundHit = true;
            if(!_startingOnGroundFalseCoroutineStarted) {
                _startingOnGroundFalseCoroutineStarted = true;
                StartCoroutine(SetStartingOnGroundToFalse());
            }
        }

        if(!isGrounded) {
            bool ceilingHit = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.up, _stats.RoofDistance, _ceilingLayerMasks);
            // Hit a Ceiling - only handle if moving upward
            if (ceilingHit && !groundHit && _frameVelocity.y > 0)
            {
                HandleCeilingCollisions();
            }
        }

        // Landed on the Ground
        if (!isGrounded && groundHit && Player.obj.rigidBody.velocity.y <= 0.05f)
        {
            isGrounded = true;
            _coyoteUsable = true;
            _endedJumpEarly = false;
            _numberOfAirJumps = 0;
            _airJumpToConsume = false;
            _landed = true;
            jumpedWhileForcePushJumping = false;
            isFalling = false;

            if (_isShadowJumping)
            {
                _isShadowJumping = false;
                _eliAudio.PlayForcePushLand();
            }

            //To avoid "double grounded". Sometimes when player barely reaches up on edge it gets grounded, but still has upwards velocity, and lands again.
            _frameVelocity.y = 0; 
        }
        // Left the Ground
        else if (isGrounded && !groundHit)
        {
            isGrounded = false;
            isOnMoveable = false;
            moveableRigidbody = null;
            PlayerPush.obj.platform = null;
            _frameLeftGrounded = _time;
        }

        HandleMicroLedges();

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private float stepHeight = 0.02f;
    private float stepSmooth = 0.02f;
    private float feetCastOffset = 0.05f; //Sometimes player collider hovers slightly above ground. If casting from feet we need to do it lower down than expected
    private float microLedgeForwardCastDistance = 0.1f;
    private void HandleMicroLedges() {
        if(_movementInput.x > 0) {
            bool wallHit = Physics2D.Raycast(_collider.bounds.center + new Vector3(_collider.size.x / 2, -_collider.size.y / 2 - feetCastOffset), Vector2.right, microLedgeForwardCastDistance, _groundLayerMasks);
            if(wallHit) {
                bool stepHeightWallHit = Physics2D.Raycast(_collider.bounds.center + new Vector3(_collider.size.x / 2, -_collider.size.y / 2 + stepHeight), Vector2.right, microLedgeForwardCastDistance, _groundLayerMasks);
                if(!stepHeightWallHit) {
                    Player.obj.rigidBody.position += Vector2.up * stepSmooth;
                }
            }
        } else if(_movementInput.x < 0) {
            bool wallHit = Physics2D.Raycast(_collider.bounds.center + new Vector3(-_collider.size.x / 2, -_collider.size.y / 2 - feetCastOffset), Vector2.left, microLedgeForwardCastDistance, _groundLayerMasks);
            if(wallHit) {
                bool stepHeightWallHit = Physics2D.Raycast(_collider.bounds.center + new Vector3(-_collider.size.x / 2, -_collider.size.y / 2 + stepHeight), Vector2.left, microLedgeForwardCastDistance, _groundLayerMasks);
                if(!stepHeightWallHit) {
                    Player.obj.rigidBody.position += Vector2.up * stepSmooth;
                }
            }
        }
    }

    private void HandleCeilingCollisions() {
        //Check for ceiling hits from top right and left corner of collider
        Bounds playerBounds = _collider.bounds;
        Vector2 topRight = new Vector2(playerBounds.max.x, playerBounds.max.y);
        Vector2 topLeft = new Vector2(playerBounds.min.x, playerBounds.max.y);
        
        bool ceilingHitRight = Physics2D.Raycast(topRight, Vector2.up, _stats.RoofDistance, _ceilingLayerMasks);
        bool ceilingHitLeft = Physics2D.Raycast(topLeft, Vector2.up, _stats.RoofDistance, _ceilingLayerMasks);

        // Debug visualization
        // Debug.DrawRay(topRight, Vector2.up * _stats.RoofDistance, Color.red, 2f);
        // Debug.DrawRay(topLeft, Vector2.up * _stats.RoofDistance, Color.red, 2f);

        // Check if player has minimal horizontal velocity (jumping straight up)
        bool hasMinimalHorizontalVelocity = Mathf.Abs(_frameVelocity.x) < 0.1f;

        if(ceilingHitRight && ceilingHitLeft) {
            _frameVelocity.y *= _stats.CeilingBounceBackSpeed;
        } else if(ceilingHitRight) {
            // Only apply corner nudge logic if player is moving vertically without horizontal velocity
            if(hasMinimalHorizontalVelocity) {
                bool isAirToTheLeft = !Physics2D.Raycast(topRight - new Vector2(0.25f, 0), Vector2.up, _stats.RoofDistance, _ceilingLayerMasks);
                if(isAirToTheLeft) {
                    transform.position = new Vector2(transform.position.x - 0.125f, transform.position.y);
                    _roundedCeilingCornerThisFrame = true;
                } else {
                    _frameVelocity.y *= _stats.CeilingBounceBackSpeed;
                }
            } else {
                _frameVelocity.y *= _stats.CeilingBounceBackSpeed;
            }
        } else if(ceilingHitLeft) {
            // Only apply corner nudge logic if player is moving vertically without horizontal velocity
            if(hasMinimalHorizontalVelocity) {
                bool isAirToTheRight = !Physics2D.Raycast(topLeft + new Vector2(0.25f, 0), Vector2.up, _stats.RoofDistance, _ceilingLayerMasks);
                if(isAirToTheRight) {
                    transform.position = new Vector2(transform.position.x + 0.125f, transform.position.y);
                    _roundedCeilingCornerThisFrame = true;
                } else {
                    _frameVelocity.y *= _stats.CeilingBounceBackSpeed;
                }
            } else {
                _frameVelocity.y *= _stats.CeilingBounceBackSpeed;
            }
        }
    }

    #endregion

    #region Jumping
    public bool isForcePushJumping = false;
    public bool jumpedWhileForcePushJumping = false;
    public float jumpedWhileForcePushJumpingModifier = 0.6f;
    public float initialForcePushJumpSpeed = 30f;
    public float initialPoweredForcePushGroundedSpeed = 30f;
    public float fullyPoweredForcePushPushBackSpeed = 10f;
    public float fullyPoweredForcePushGroundedPushBackSpeed = 10f;
    public float partialForcePushPushBackSpeed = 5f;
    public float normalForcePushPushBackSpeed = 10f;
    public float forcePushJumpOnGroundDuration = 0.01f;
    public float forcePushJumpOnGroundTimer = 0f;
    private bool _jumpToConsume;
    private float _timeJumpWasPressed = -100;  //To avoid having buffered jump from the start
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private bool _airJumpToConsume = false;
    private int _numberOfAirJumps = 0;
    private const int MAX_NUMBER_OF_AIR_JUMPS = 1;
    
    private bool _isShadowJumping = false;
    private float _shadowJumpStartY = 0f;
    private float _shadowJumpStartX = 0f;
    private float _shadowJumpHorizontalDistanceTraveled = 0f;
    private bool _shadowJumpOppositeDirectionPressed = false;

    private bool CanUseJump => (isGrounded || CanUseCoyote) && _jumpToConsume;
    private bool HasBufferedJump => _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !isGrounded && _time < _frameLeftGrounded + _stats.CoyoteTime;
    private bool CanUseAirJump =>
        GameManager.obj.isDevMode &&
        !isGrounded &&
        _time > _frameLeftGrounded + _stats.CoyoteTime &&
        _numberOfAirJumps < MAX_NUMBER_OF_AIR_JUMPS &&
        _airJumpToConsume;

    private void HandleJump()
    {
        if (_isBalancing) return;
        
        if (!_endedJumpEarly && !isGrounded && !_jumpHeldInput && Player.obj.rigidBody.velocity.y > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !CanUseAirJump && !HasBufferedJump) return;

        if(HasBufferedJump && isGrounded && !_jumpToConsume) {
            ExecuteRegularJump();
            return;
        }

        if (CanUseJump) ExecuteRegularJump();

        if (CanUseAirJump) ExecuteAirJump();
    }

    private float _jumpSqueezeX = 0.8f;
    private float _jumpSqueezeY = 1.2f;
    private float _jumpSqueezeTime = 0.12f;


    private void ExecuteRegularJump()
    {
        ExecuteJump(_stats.JumpPower);
        
        // Activate jump kick start
        // Disabled for now due to horizontal scrolling issue
        // if(!isForcePushJumping && !_isDashing && Mathf.Abs(_frameVelocity.x) >= _stats.MaxSpeed) {
        //     _isJumpKickActive = true;
        //     _jumpKickTimer = _jumpKickDuration;
        //     _jumpKickDirection = isFacingLeft() ? -1f : 1f;
        // }

        if(_isDashing) {
            //Reset the high speed of the dash
            if(_frameVelocity.x > _stats.MaxSpeed) {
                _frameVelocity.x = _stats.MaxSpeed;  
            } else if(_frameVelocity.x < -_stats.MaxSpeed) {
                _frameVelocity.x = -_stats.MaxSpeed;
            }
            _isDashing = false;
        }
        
        DustParticleMgr.obj.CreateDust(PlayerManager.PlayerType.HUMAN);
        _sharedPlayerAudio.PlayJump();

        StartCoroutine(JumpSqueeze(_jumpSqueezeX, _jumpSqueezeY, _jumpSqueezeTime));
        _jumpToConsume = false;
    }

    public void JumpSqueeze() {
        StartCoroutine(JumpSqueeze(_jumpSqueezeX, _jumpSqueezeY, _jumpSqueezeTime));
    }

    private void ExecuteAirJump()
    {
        ExecuteJump(_stats.JumpPower);
        _airJumpToConsume = false;
        _numberOfAirJumps++;
    }

    private void ExecuteShadowJump()
    {
        isOnMoveable = false;
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _coyoteUsable = false;
        _frameVelocity.y = _stats.ShadowJumpPower;
        _isShadowJumping = true;
        _jumpToConsume = false;
        _shadowJumpStartY = transform.position.y;
        _shadowJumpStartX = transform.position.x;
        _shadowJumpHorizontalDistanceTraveled = 0f;
        _shadowJumpOppositeDirectionPressed = false;
        
        PlayerPush.obj.ResetBuiltUpPower();
        
        _shadowJumpParticles.Play();
        _shadowJumpExplosionParticles.Emit(_shadowJumpExplosionParticlesCount);
        StartCoroutine(JumpSqueeze(_jumpSqueezeX, _jumpSqueezeY, _jumpSqueezeTime));
        
        PlayerPush.obj.ExecuteShadowJumpVfx();
        _eliAudio.PlayForcePushJump();
        _ghostTrail.ShowGhosts();
    }

    private void ExecuteJump(float jumpPower)
    {
        isOnMoveable = false;
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _coyoteUsable = false;
        _frameVelocity.y = jumpPower;
    }

    #endregion

    #region Horizontal

    private void HandleDirection()
    {
        if(_freezePlayer) {
            _frameVelocity.x = 0;
            return;
        }
        
        if (isForcePushJumping) {
            forcePushJumpOnGroundTimer += Time.fixedDeltaTime;
            if(forcePushJumpOnGroundTimer > forcePushJumpOnGroundDuration) 
                isForcePushJumping = false;
        }
        
        
        if(jumpedWhileForcePushJumping) {
            //Time to defy physics and keep horizontal velocity, except if you hit a wall
            if(Player.obj.rigidBody.velocity.x < 0.01 && Player.obj.rigidBody.velocity.x > -0.01)
                jumpedWhileForcePushJumping = false;
        } else if (_isShadowJumping) {
            _shadowJumpHorizontalDistanceTraveled = Mathf.Abs(transform.position.x - _shadowJumpStartX);
            bool shouldApplyDeceleration = _shadowJumpHorizontalDistanceTraveled >= _stats.ShadowJumpDecelerationStartDistance;
            
            if (_movementInput.x == 0)
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, _stats.ShadowJumpHorizontalDeceleration * Time.fixedDeltaTime);
            }
            else
            {
                if (!_shadowJumpOppositeDirectionPressed)
                {
                    bool isPressingOppositeDirection = (_frameVelocity.x > 0 && _movementInput.x < 0) || (_frameVelocity.x < 0 && _movementInput.x > 0);
                    if (isPressingOppositeDirection)
                    {
                        _shadowJumpOppositeDirectionPressed = true;
                    }
                }
                
                if (_shadowJumpOppositeDirectionPressed)
                {
                    if (_movementInput.x == 0)
                    {
                        _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, _stats.AirDeceleration * Time.fixedDeltaTime);
                    }
                    else
                    {
                        _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _movementInput.x * _stats.MaxSpeed, _stats.ShadowJumpInterruptedDeceleration * Time.fixedDeltaTime);
                    }
                }
                else if (shouldApplyDeceleration)
                {
                    _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _movementInput.x * _stats.MaxSpeed, _stats.ShadowJumpHorizontalDecelerationPower * Time.fixedDeltaTime);
                }
                else
                {
                    _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _movementInput.x * _stats.ShadowJumpMaxHorizontalSpeed, _stats.ShadowJumpHorizontalAcceleration * Time.fixedDeltaTime);
                }
            }
        } else {
            if (_isBalancing) {
                HandleBalancingMovement();
            } else if (_isWalking) {
                if (_movementInput.x == 0)
                {
                    _frameVelocity.x = 0;
                }
                else
                {
                    _frameVelocity.x = _movementInput.x * _walkSpeed;
                }
            } else if(_isDashing) {
                if (_movementInput.x == 0)
                {
                    _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, dashDecelerationTime * Time.fixedDeltaTime);
                    if(_frameVelocity.x == 0)
                        _isDashing = false;
                }
                else
                {
                    _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _movementInput.x * _stats.MaxSpeed, dashDecelerationTime * Time.fixedDeltaTime);
                    if(_frameVelocity.x == _movementInput.x * _stats.MaxSpeed)
                        _isDashing = false;
                }
            } else {
                if (_movementInput.x == 0)
                {
                    var deceleration = isGrounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                    _frameVelocity.x = isOnMoveable && moveableRigidbody != null ?
                        moveableRigidbody.velocity.x :
                        Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
                }
                else
                {
                    _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, (_movementInput.x * _stats.MaxSpeed) + (isOnMoveable && moveableRigidbody != null ? moveableRigidbody.velocity.x : 0), _stats.Acceleration * Time.fixedDeltaTime);
                }
            }
            
        }
        

        // Apply jump kick boost to horizontal frame velocity if active
        // if (_isJumpKickActive)
        // {
        //     _frameVelocity.x += _jumpKickHorizontal * _jumpKickDirection;
        // }
    }
    
    private void HandleBalancingMovement()
    {
        _balancingTimer += Time.fixedDeltaTime;
        
        if (_balancingInBurst)
        {
            if (_balancingTimer >= _balancingBurstDuration)
            {
                _balancingInBurst = false;
                _balancingTimer = 0f;
            }
            
            if (_movementInput.x == 0)
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, _stats.GroundDeceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _movementInput.x * _balancingMoveSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            if (_balancingTimer >= _balancingPauseDuration)
            {
                _balancingInBurst = true;
                _balancingTimer = 0f;
            }
            
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, _stats.GroundDeceleration * Time.fixedDeltaTime);
        }
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if(IsControlledProgrammatically) {
            _frameVelocity.y = 0;
            return;
        }
        if(isOnMoveable && moveableRigidbody != null && !_isShadowJumping) {
            _frameVelocity.y = moveableRigidbody.velocity.y;
            return;
        }
        if (isGrounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = _stats.GroundingForce;
        }
        else
        {
            if (_isDashing)
            {
                //Just keep horizontal movement
                _frameVelocity.y = 0;
            }
            else if (_isShadowJumping)
            {
                // Skip gravity deceleration if we just rounded a ceiling corner while moving upward
                if (!_roundedCeilingCornerThisFrame)
                {
                    bool isAboveStartingPosition = transform.position.y >= _shadowJumpStartY;
                    if (isAboveStartingPosition)
                    {
                        var shadowGravity = _frameVelocity.y > 0 ? _stats.ShadowJumpVerticalDeceleration : _stats.ShadowJumpVerticalAcceleration;
                        if (_endedJumpEarly && _frameVelocity.y > 0)
                            shadowGravity *= _stats.ShadowJumpEndEarlyGravityModifier;
                        _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.ShadowJumpMaxFallSpeed, shadowGravity * Time.fixedDeltaTime);
                    }
                    else
                    {
                        _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, _stats.FallAcceleration * Time.fixedDeltaTime);
                    }
                }
            }
            else
            {
                // Skip gravity deceleration if we just rounded a ceiling corner while moving upward
                if (!_roundedCeilingCornerThisFrame)
                {
                    var inAirGravity = _stats.FallAcceleration;
                    if (jumpedWhileForcePushJumping)
                        inAirGravity *= jumpedWhileForcePushJumpingModifier;
                    if (_endedJumpEarly && _frameVelocity.y > 0)
                        inAirGravity *= _stats.JumpEndEarlyGravityModifier;

                    _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
                }
            }
        }
    }

    #endregion

    private void ApplyMovement() {
        if(Player.obj.rigidBody.bodyType != RigidbodyType2D.Static) {
            Player.obj.rigidBody.velocity = _frameVelocity;
        }
    } 

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * _stats.GrounderDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * _stats.RoofDistance);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif
}
