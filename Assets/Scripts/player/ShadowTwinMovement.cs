using System.Collections;
using Cinemachine;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShadowTwinMovement : MonoBehaviour
{
    // --- Jump Kick Start fields ---
    // private bool _isJumpKickActive = false;
    // private float _jumpKickTimer = 0f;
    // public float _jumpKickDuration = 0.1f; // seconds
    // public float _jumpKickHorizontal = 4f; // tune as needed
    // private float _jumpKickDirection = 1f;
    // --------------------------------
    public SurfaceTypeManager.SurfaceType surface = SurfaceTypeManager.SurfaceType.Default;
    public static ShadowTwinMovement obj;

    public bool isDevMode = true;
    [SerializeField] private ScriptableStats _stats;
    [SerializeField] private GameObject _playerTwin;
    [SerializeField] private GameObject _playerBlob;
    [SerializeField] private GhostTrailManager _ghostTrail;
    [SerializeField] private GameObject _soulVfx;
    
    public SpriteRenderer spriteRenderer;
    public GameObject anchor;
    private BoxCollider2D _collider;
    private Animator _animator;
    private PlayerInput _playerInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;

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
    private SharedCharacterAudio _sharedPlayerAudio;
    private DeeAudio _deeAudio;
    
    // --- Hit boost variables ---
    private bool _isHit = false;
    private float _hitBoostTimer = 0f;
    private int _hitBoostPhase = 0; // 0: idle, 1: rising, 2: falling
    private float _currentHitBoost = 0f;
    private float _hitBoostDirection = 1f;
    private float _hitBoostMax = 10f;
    [SerializeField] private float _hitBoostRiseTime = 0.12f;
    [SerializeField] private float _hitBoostFallTime = 0.3f;

    // --- Wall jump variables ---
    [Header("Wall Jump Configuration")]
    [SerializeField] private float _wallJumpVerticalPower = 12f;
    [SerializeField] private float _wallJumpHorizontalPower = 15f;
    [SerializeField] private float _wallJumpDirectionLockDuration = 0.2f;
    [SerializeField] private float _wallJumpBoostDuration = 0.1f;
    
    private bool _isWallJumping = false;
    private float _wallJumpTimer = 0f;
    private float _wallJumpDirection = 0f;
    private float _wallJumpBoostTimer = 0f;

    // --- Floating platform propel variables ---
    [Header("Floating Platform Propel Configuration")]
    [SerializeField] private float _propelThroughPlatformDuration = 0.3f;
    [SerializeField] private float _postPropelFloatyDuration = 0.5f;
    [SerializeField] private float _postPropelGravityModifier = 0.4f; // Lower = more floaty
    [SerializeField] private float _platformColliderDisableDuration = 0.6f; // How long to disable platform collider
    
    private bool _isPropellingThroughPlatform = false;
    private float _propelTimer = 0f;
    private Vector2 _propelVelocity = Vector2.zero;
    private bool _isLatchingToFloatingPlatform = false;
    private bool _isPostPropelFloaty = false;
    private float _postPropelFloatyTimer = 0f;
    private FloatyPlatform _targetFloatingPlatform = null;

    private void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _ceilingLayerMasks = LayerMask.GetMask(new[] { "Ground", "Default", "Block" });
        _latchLayerMask = LayerMask.GetMask(new[] { "Ground", "Block", "JumpThroughs", "Pullable" });
        _playerInput = GetComponent<PlayerInput>();
        _sharedPlayerAudio = GetComponent<SharedCharacterAudio>();
        _deeAudio = GetComponent<DeeAudio>();
    }

    private void OnDestroy()
    {
        obj = null;
    }

    public void EnableSharedCharacterAudio()
    {
        if (_sharedPlayerAudio != null)
        {
            _sharedPlayerAudio.EnableSound();
        }
    }

    public void DisableSharedCharacterAudio()
    {
        if (_sharedPlayerAudio != null)
        {
            _sharedPlayerAudio.DisableSound();
        }
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

        if(_isPausedInAir) {
            _frameVelocity = new Vector2(0,0);
        }

        ApplyMovement();
    }

    private void Update()
    {
        _time += Time.deltaTime;
        UpdateAnimator();
        
        HandleFlipPlayer();
        
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
        
        // Update wall jump timer
        if (_isWallJumping)
        {
            // Cancel wall jump if player lands on ground
            if (IsEffectivelyGrounded())
            {
                _isWallJumping = false;
                _wallJumpBoostTimer = 0f;
            }
            else
            {
                _wallJumpTimer -= Time.deltaTime;
                _wallJumpBoostTimer -= Time.deltaTime;
                
                if (_wallJumpTimer <= 0f)
                {
                    _isWallJumping = false;
                    _wallJumpBoostTimer = 0f;
                }
            }
        }
        
        // Update propel through platform timer
        if (_isPropellingThroughPlatform)
        {
            // Cancel propel if player lands on ground
            if (isGrounded)
            {
                _isPropellingThroughPlatform = false;
                _propelTimer = 0f;
                ShadowTwinPlayer.obj.ResetGravity();
            }
            else
            {
                _propelTimer -= Time.deltaTime;
                
                if (_propelTimer <= 0f)
                {
                    _isPropellingThroughPlatform = false;
                    _propelTimer = 0f;
                    // Transition to floaty state instead of immediately resetting gravity
                    _isPostPropelFloaty = true;
                    _postPropelFloatyTimer = _postPropelFloatyDuration;
                    ShadowTwinPlayer.obj.ResetGravity();
                }
            }
        }
        
        // Update post-propel floaty timer
        if (_isPostPropelFloaty)
        {
            // Cancel floaty state if player lands on ground
            if (isGrounded)
            {
                _isPostPropelFloaty = false;
                _postPropelFloatyTimer = 0f;
            }
            else
            {
                _postPropelFloatyTimer -= Time.deltaTime;
                
                if (_postPropelFloatyTimer <= 0f)
                {
                    _isPostPropelFloaty = false;
                    _postPropelFloatyTimer = 0f;
                }
            }
        }
        
        // Update jump kick timer
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

    private void HandleFlipPlayer() {
        if (ShadowTwinPull.obj != null && ShadowTwinPull.obj.IsControllingObject)
        {
            Rigidbody2D controlledObject = ShadowTwinPull.obj.GetControlledObject();
            if (controlledObject != null)
            {
                // Face the controlled object
                float objectX = controlledObject.position.x;
                float playerX = transform.position.x;
                if (objectX < playerX - 0.1f)
                {
                    spriteRenderer.flipX = true;
                }
                else if (objectX > playerX + 0.1f)
                {
                    spriteRenderer.flipX = false;
                }
            }
            return;
        }
        
        if(_isWallJumping) {
            return;
        }
        
        // When latched to a wall, face away from the wall
        if (_isLatchedToSurface && _latchSurfaceType == LatchSurfaceType.Wall)
        {
            // _latchDirection.x tells us which way we're facing the wall
            // If latchDirection.x is positive (1), we latched to the right, so face left (flipX = true)
            // If latchDirection.x is negative (-1), we latched to the left, so face right (flipX = false)
            if (_latchDirection.x > 0)
            {
                spriteRenderer.flipX = true; // Face left (away from wall on the right)
            }
            else if (_latchDirection.x < 0)
            {
                spriteRenderer.flipX = false; // Face right (away from wall on the left)
            }
            return;
        }

        FlipPlayer(_movementInput.x);
    }

    public float baseProjectilePushPower = 7f;
    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.transform.CompareTag("Projectile")) {  
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            bool hitFromTheLeft = collision.transform.position.x < ShadowTwinPlayer.obj.rigidBody.position.x;

            if(isOnMoveable && moveableRigidbody.CompareTag("FloatingPlatform")) {
                if(PlayerMovement.obj.moveableRigidbody != moveableRigidbody) {
                    FloatyPlatform floatyPlatform = moveableRigidbody.GetComponent<FloatyPlatform>();
                    floatyPlatform.MovePlatform(hitFromTheLeft, projectile.power);
                }
                //else, if both players on same platform, don't apply any force to shadow twin, or the platform. Let PlayerPush handle it
            } else {
                float power = baseProjectilePushPower * projectile.power;
                _frameVelocity.x = hitFromTheLeft ? power : -power;
            }
        }
    }

    private Vector2 GetTopMiddleColliderPosition() {
        return _collider.bounds.center + new Vector3(0, _collider.bounds.extents.y, 0);
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

    //May be used by moveables, like floating platforms, to unregister themselves
    public void UnregisterMoveable() {
        isOnMoveable = false;
        moveableRigidbody = null;
    }

    public float _poweredDashMultiplier = 1.2f;
    public void ExecuteDash(ShadowTwinPull.PullPowerType chargePower)
    {
        _isDashing = true;
        float speed = 0;
        if(chargePower == ShadowTwinPull.PullPowerType.Powered) {
            speed = initialDashSpeed * _poweredDashMultiplier;
            ShadowTwinPlayer.obj.SetHasPowerUp(false);
        } else if(chargePower == ShadowTwinPull.PullPowerType.Full) {
            speed = initialDashSpeed;
        }
        _frameVelocity.x = IsFacingLeft() ? -speed : speed;
    }

    public void EndDash() {
        _isDashing = false;
    }

    public void TriggerForcePullAnimation() {
        _animator.SetTrigger("forcePush");
    }

    public bool isFalling = false;
    public bool isMoving = false;
    public bool IsPulling = false;
    private bool _isLatchPulling = false;

    private void UpdateAnimator()
    {
        _animator.SetBool("isDashing", _isDashing);
        _animator.SetBool("isGrounded", isGrounded);
        // Keep moving during short grace to avoid triggering stop animation on quick direction changes
        // Velocity is not enough to check though, since player can have velocity, but there's no movement input
        //isMoving = Mathf.Abs(Player.obj.rigidBody.velocity.x) > _movingVelocityEpsilon || _movementInput.x != 0;
        isMoving = _movementInput.x != 0;
        _animator.SetBool("isMoving", isMoving);
        isFalling = _frameVelocity.y < -_stats.MinimumFallAnimationSpeed;
        _animator.SetBool("isFalling", isFalling);
        _animator.SetBool("isLatched", _isLatchedToSurface);
        if (_landed)
        {
            DustParticleMgr.obj.CreateDust(PlayerManager.PlayerType.SHADOW_TWIN);
            _sharedPlayerAudio.PlayLand(surface);
            StartCoroutine(JumpSqueeze(_landedSqueezeX, _landedSqueezeY, _landedSqueezeTime));
            _landed = false;
        }
    }

    private bool _freezePlayer = false;
    private bool _stopMovement = false;
    private bool _stopCollisions = false;
    private bool _isPausedInAir = false;
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

    public void UnFreeze() {
        _freezePlayer = false;
        EnablePlayerMovement();
    }

    public void PauseInAir() {
        _isPausedInAir = true;
        _animator.SetTrigger("anchorPull");
        UpdateAnimatorIsLatchPulling(true);
    }

    public void UnpauseInAir() {
        _isPausedInAir = false;
    }

    public bool IsPausedInAir() {
        return _isPausedInAir;
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
        ShadowTwinPlayer.obj.rigidBody.gravityScale = 0;
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

    public void SetPlayerInputDevice(PlayerSlot slot) {
        _playerInput.enabled = true;
        _playerInput.SwitchCurrentControlScheme(slot.device is Keyboard ? "Keyboard" : "Gamepad", slot.device);
    }

    public void EnablePlayerAfterLevelTransition() {
        UnFreeze();
        ShadowTwinPlayer.obj.rigidBody.gravityScale = 1;
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
    public void SetNewPowerRecevied() {
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

    public void OnMovement(InputAction.CallbackContext value)
    {
        _movementInput = value.ReadValue<Vector2>();
        _movementInput.x = GetHorizontalInput(_movementInput.x);
        if (_stats.SnapInput)
        {
            _movementInput.y = Mathf.Abs(_movementInput.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_movementInput.y);
        }
    } 

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
        ShadowTwinPlayer.obj.rigidBody.AddForce(new Vector2(-1000, 0));
    }

    public bool IsHorizontalInput() {
        return _movementInput.x != 0;
    }

    public void SimulateJumpInput(bool jumpHeld, float currentTime) {
        _jumpHeldInput = jumpHeld;
        
        if (jumpHeld && !_previousJumpHeld) {
            if (IsEffectivelyGrounded() || CanUseCoyote) {
                _jumpToConsume = true;
            }
            _timeJumpWasPressed = currentTime;
        }
        
        _previousJumpHeld = jumpHeld;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(jumpThroughPlatform != null &&
                _movementInput.y < 0) {
                jumpThroughPlatform.PassThrough();
                return;
            }

            if (IsEffectivelyGrounded() || CanUseCoyote) {
                _jumpToConsume = true;
            }
            else
            {
                if(_isLatchedToSurface && (_latchSurfaceType == LatchSurfaceType.Wall || _latchSurfaceType == LatchSurfaceType.Ceiling)) {
                    _latchedJumpToConsume = true;
                }
            }
            _jumpHeldInput = true;
            _timeJumpWasPressed = _time;
        }
        else if (context.canceled)
        {
            _jumpHeldInput = false;
        }
    }

    public bool isTransforming = false;
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

    public void OnHit(float direction, float hitBoost)
    {
        _isHit = true;
        _hitBoostPhase = 1;
        _hitBoostTimer = 0f;
        _hitBoostDirection = Mathf.Sign(direction);
        _currentHitBoost = 0f;
        _hitBoostMax = hitBoost;
    }

    private void HandleSwitchCharacter() {
        if(isTransforming)
            return;

        if(PlayerPowersManager.obj.CanSwitchBetweenTwinsMerged && !PlayerManager.obj.IsSeparated) {
            //Switch to shadow twin
            _sharedPlayerAudio.PlayShapeshift();
            isTransforming = true;
            ShadowTwinPull.obj.CancelPulling();
            ShadowTwinPull.obj.DisablePull();
            
            if(PlayerManager.obj.IsEliInBlobForm()) {
                //The execution for actually transforming to twin blob is done from the animation events 
                ShadowTwinPlayer.obj.PlayToTwinBlobAnimation();
            } else {
                //The execution for actually transforming to twin is done from the animation events 
                ShadowTwinPlayer.obj.PlayToTwinAnimation();
            }
        } else if(PlayerPowersManager.obj.CanSeparate) {
            StartCoroutine(SwitchTo(PlayerManager.obj.IsEliInBlobForm()));
        }
    }

    private IEnumerator SwitchTo(bool isEliInBlobForm) {
        //Before disabling all controls we need to make sure that any ongoing pull will not be cancelled
        if(IsPulling) {
            ShadowTwinPull.obj.HoldPull = true;
        }
        //Disable all controls
        PlayerSwitcher.obj.DisableAll();
        GameObject soul = Instantiate(_soulVfx, transform.position, transform.rotation);
        PrisonerSoul prisonerSoul = soul.GetComponent<PrisonerSoul>();
        if(isEliInBlobForm) {
            prisonerSoul.Target = _playerBlob.transform.position;
        } else {
            prisonerSoul.Target = _playerTwin.transform.position;
        }
        while (!prisonerSoul.IsTargetReached) {
            yield return null;
        }
        if(isEliInBlobForm) {
            PlayerBlob.obj.FlashOnce();
            PlayerSwitcher.obj.SwitchToBlob();
        } else {
            Player.obj.FlashOnce();
            PlayerSwitcher.obj.SwitchToEli();
        }
        Destroy(soul);
        yield return null;
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
            if(PlayerManager.obj.IsSeparated && !CloseEnoughToMerge(PlayerManager.obj.IsEliInBlobForm())) {
                HandleSwitchCharacter();
                return;
            }
            _mergeSplitHeld = true;
            _mergeSplitHoldTimer = 0f;
            _sharedPlayerAudio.PlayMergeSplit(ref _mergeSplitSfxInstance);
            ShadowTwinPlayer.obj.StartChargeFlash();
        }
        else if (context.canceled)
        {
            _mergeSplitHeld = false;
            _mergeSplitHoldTimer = 0f;

            //Only abort flash and sfx if eli is active. If not, the split happened and we want to finish the vfx and sfx
            if(PlayerSwitcher.obj.IsDeeActive()) {
                if(AudioUtils.IsPlaying(_mergeSplitSfxInstance)) {
                    AudioUtils.SafeStop(ref _mergeSplitSfxInstance);
                }
                ShadowTwinPlayer.obj.AbortFlash();
            }
        }
    }

    private bool CloseEnoughToMerge(bool isEliInBlobForm) {
        if(isEliInBlobForm) {
            return isGrounded && _playerBlob.GetComponent<PlayerBlobMovement>().isGrounded && Vector3.Distance(_playerBlob.transform.position, transform.position) <= 2f;
        } else {
            return isGrounded && _playerTwin.GetComponent<PlayerMovement>().isGrounded && Vector3.Distance(_playerTwin.transform.position, transform.position) <= 1.5f;
        }
    }

    private void PerformMergeSplit()
    {
        if(PlayerPowersManager.obj.CanSeparate) {
            if(PlayerManager.obj.IsSeparated) {
                StartCoroutine(MergeVfx(PlayerManager.obj.IsEliInBlobForm()));
            } else {
                bool isEliInBlobForm = PlayerManager.obj.IsEliInBlobForm();
                Vector3 splitTarget;
                if(isEliInBlobForm) {
                    if(IsFacingLeft()) {
                        splitTarget = transform.position + new Vector3(-1, -0.5f, 0);
                    } else {
                        splitTarget = transform.position + new Vector3(1, -0.5f, 0);
                    }
                } else {
                    if(IsFacingLeft()) {
                        splitTarget = transform.position + new Vector3(-1, 0, 0);
                    } else {
                        splitTarget = transform.position + new Vector3(1, 0, 0);
                    }
                }
                StartCoroutine(SplitVfx(splitTarget, PlayerManager.obj.IsEliInBlobForm()));
            }
        }
    }

    private IEnumerator MergeVfx(bool isEliInBlobForm) {
        GameObject soul;
        if(isEliInBlobForm) {
            _playerBlob.SetActive(false);
            soul = Instantiate(_soulVfx, _playerBlob.transform.position, _playerBlob.transform.rotation);
        } else {
            _playerTwin.SetActive(false);
            soul = Instantiate(_soulVfx, _playerTwin.transform.position, _playerTwin.transform.rotation);
        }
        
        PrisonerSoul prisonerSoul = soul.GetComponent<PrisonerSoul>();
        prisonerSoul.Target = transform.position;
        while (!prisonerSoul.IsTargetReached) {
            yield return null;
        }
        PlayerManager.obj.IsSeparated = false;
        Destroy(soul);
        yield return null;
    }

    private IEnumerator SplitVfx(Vector3 target, bool isEliInBlobForm) {
        GameObject soul = Instantiate(_soulVfx, transform.position, transform.rotation);
        PrisonerSoul prisonerSoul = soul.GetComponent<PrisonerSoul>();
        prisonerSoul.Target = target;
        while (!prisonerSoul.IsTargetReached) {
            yield return null;
        }
        if(isEliInBlobForm)
            SplitToBlob(target);
        else    
            SplitToTwin(target);
        Destroy(soul);
        yield return null;
    }

    public void ToTwin() {
        ICinemachineCamera activeVirtualCamera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if(activeVirtualCamera != null && activeVirtualCamera.Follow == transform) {
            activeVirtualCamera.Follow = _playerTwin.transform;
        }

        ShadowTwinPlayer.obj.rigidBody.velocity = new Vector2(0, 0);
        _frameVelocity = new Vector2(0, 0);
        gameObject.SetActive(false);
        _playerTwin.transform.position = transform.position;
        _playerTwin.GetComponent<PlayerMovement>().spriteRenderer.flipX = IsFacingLeft();
        _playerTwin.SetActive(true);
        PlayerSwitcher.obj.SwitchToEli();
        if(isGrounded) {
            _playerTwin.GetComponent<PlayerMovement>().SetStartingOnGround();
            _playerTwin.GetComponent<PlayerMovement>().isGrounded = true;
        } else {
            _playerTwin.GetComponent<PlayerMovement>().isGrounded = false;
        }
        if(IsFrozen()) {
            _playerTwin.GetComponent<PlayerMovement>().Freeze();
        } else {
            _playerTwin.GetComponent<PlayerMovement>().UnFreeze();
        }
        PlayerPush.obj.EnableCharge();
        isTransforming = false;
    }

    public void SplitToTwin(Vector3 splitTarget) {
        ICinemachineCamera activeVirtualCamera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if(activeVirtualCamera != null && activeVirtualCamera.Follow == transform) {
            activeVirtualCamera.Follow = _playerTwin.transform;
        }

        ShadowTwinPlayer.obj.rigidBody.velocity = new Vector2(0, 0);
        _frameVelocity = new Vector2(0, 0);
        
        _playerTwin.transform.position = splitTarget;
        _playerTwin.GetComponent<PlayerMovement>().spriteRenderer.flipX = IsFacingLeft();
        if(isGrounded) {
            _playerTwin.GetComponent<PlayerMovement>().SetStartingOnGround();
            _playerTwin.GetComponent<PlayerMovement>().isGrounded = true;
        } else {
            _playerTwin.GetComponent<PlayerMovement>().isGrounded = false;
        }
        _playerTwin.SetActive(true);

        //Need to reset animator. For some reason it starts playing jump animation
        Player.obj.ResetAnimator();

        PlayerSwitcher.obj.SwitchToEli();
        
        PlayerPush.obj.EnableCharge();

        PlayerManager.obj.IsSeparated = true;
    }

    public void ToBlob() {
        ICinemachineCamera activeVirtualCamera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if(activeVirtualCamera != null && activeVirtualCamera.Follow == transform) {
            activeVirtualCamera.Follow = _playerBlob.transform;
        }

        ShadowTwinPlayer.obj.rigidBody.velocity = new Vector2(0, 0);
        _frameVelocity = new Vector2(0, 0);
        gameObject.SetActive(false);
        _playerBlob.transform.position = transform.position - new Vector3(0, 0.5f, 0);
        _playerBlob.GetComponent<PlayerBlobMovement>().spriteRenderer.flipX = IsFacingLeft();
        _playerBlob.SetActive(true);
        PlayerSwitcher.obj.SwitchToBlob();
        if(isGrounded) {
            _playerBlob.GetComponent<PlayerBlobMovement>().SetStartingOnGround();
            _playerBlob.GetComponent<PlayerBlobMovement>().isGrounded = true;
        } else {
            _playerBlob.GetComponent<PlayerBlobMovement>().isGrounded = false;
        }
        if(IsFrozen()) {
            _playerBlob.GetComponent<PlayerBlobMovement>().Freeze();
        } else {
            _playerBlob.GetComponent<PlayerBlobMovement>().UnFreeze();
        }
        isTransforming = false;
    }

    public void SplitToBlob(Vector3 splitTarget) {
        ICinemachineCamera activeVirtualCamera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if(activeVirtualCamera != null && activeVirtualCamera.Follow == transform) {
            activeVirtualCamera.Follow = _playerBlob.transform;
        }

        ShadowTwinPlayer.obj.rigidBody.velocity = new Vector2(0, 0);
        _frameVelocity = new Vector2(0, 0);

        _playerBlob.transform.position = splitTarget;

        _playerBlob.GetComponent<PlayerBlobMovement>().spriteRenderer.flipX = IsFacingLeft();
        if(isGrounded) {
            _playerBlob.GetComponent<PlayerBlobMovement>().SetStartingOnGround();
            _playerBlob.GetComponent<PlayerBlobMovement>().isGrounded = true;
        } else {
            _playerBlob.GetComponent<PlayerBlobMovement>().isGrounded = false;
        }
        _playerBlob.SetActive(true);
        PlayerSwitcher.obj.SwitchToBlob();

        PlayerManager.obj.IsSeparated = true;
    }

    public void CancelJumping() {
        _jumpToConsume = false;
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
    public bool startingOnGround = true;

    // Helper method to get effective grounded state considering shadow lash
    private bool IsEffectivelyGrounded()
    {
        // During shadow lash, always treat player as airborne even if physically grounded
        if (ShadowTwinLash.obj != null && ShadowTwinLash.obj.ShouldTreatPlayerAsAirborne())
        {
            return false;
        }
        return isGrounded;
    }
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
        if (!isGrounded && groundHit && ShadowTwinPlayer.obj.rigidBody.velocity.y <= 0.05f)
        {
            isGrounded = true;
            _coyoteUsable = true;
            _endedJumpEarly = false;
            _latchedJumpToConsume = false;
            _landed = true;
            isFalling = false;

            //To avoid "double grounded". Sometimes when player barely reaches up on edge it gets grounded, but still has upwards velocity, and lands again.
            _frameVelocity.y = 0; 
        }
        // Left the Ground
        else if (isGrounded && !groundHit)
        {
            isGrounded = false;
            isOnMoveable = false;
            moveableRigidbody = null;
            _frameLeftGrounded = _time;
        }

        HandleMicroLedges();

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    public float stepHeight = 0.02f;
    public float stepSmooth = 0.02f;
    public float feetCastOffset = 0.05f; //Sometimes player collider hovers slightly above ground. If casting from feet we need to do it lower down than expected
    public float microLedgeForwardCastDistance = 0.1f;
    private void HandleMicroLedges() {
        if(!isGrounded) return;
        
        if(_movementInput.x > 0) {
            bool wallHit = Physics2D.Raycast(_collider.bounds.center + new Vector3(_collider.size.x / 2, -_collider.size.y / 2 - feetCastOffset), Vector2.right, microLedgeForwardCastDistance, _groundLayerMasks);
            if(wallHit) {
                bool stepHeightWallHit = Physics2D.Raycast(_collider.bounds.center + new Vector3(_collider.size.x / 2, -_collider.size.y / 2 + stepHeight), Vector2.right, microLedgeForwardCastDistance, _groundLayerMasks);
                if(!stepHeightWallHit) {
                    ShadowTwinPlayer.obj.rigidBody.position += Vector2.up * stepSmooth;
                }
            }
        } else if(_movementInput.x < 0) {
            bool wallHit = Physics2D.Raycast(_collider.bounds.center + new Vector3(-_collider.size.x / 2, -_collider.size.y / 2 - feetCastOffset), Vector2.left, microLedgeForwardCastDistance, _groundLayerMasks);
            if(wallHit) {
                bool stepHeightWallHit = Physics2D.Raycast(_collider.bounds.center + new Vector3(-_collider.size.x / 2, -_collider.size.y / 2 + stepHeight), Vector2.left, microLedgeForwardCastDistance, _groundLayerMasks);
                if(!stepHeightWallHit) {
                    ShadowTwinPlayer.obj.rigidBody.position += Vector2.up * stepSmooth;
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
    private bool _jumpToConsume;
    private float _timeJumpWasPressed = -100;  //To avoid having buffered jump from the start
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private bool _latchedJumpToConsume = false;
    private bool _previousJumpHeld = false;

    private bool CanUseJump => (IsEffectivelyGrounded() || CanUseCoyote) && _jumpToConsume;
    private bool HasBufferedJump => _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !IsEffectivelyGrounded() && _time < _frameLeftGrounded + _stats.CoyoteTime;

    private void HandleJump()
    {
        if (!_endedJumpEarly && !IsEffectivelyGrounded() && !_jumpHeldInput && ShadowTwinPlayer.obj.rigidBody.velocity.y > 0) _endedJumpEarly = true;

        if (_isLatchedToSurface && _latchedJumpToConsume)
        {
            HandleLatchJump();
            return;
        }

        if (!_jumpToConsume && !HasBufferedJump) return;

        if(HasBufferedJump && IsEffectivelyGrounded() && !_jumpToConsume) {
            ExecuteRegularJump();
            return;
        }

        if (CanUseJump) ExecuteRegularJump();
    }

    private float _jumpSqueezeX = 0.8f;
    private float _jumpSqueezeY = 1.2f;
    private float _jumpSqueezeTime = 0.12f;

    private void ExecuteRegularJump()
    {
        if(IsPulling) {
            ShadowTwinPull.obj.CancelPulling();
            ShadowTwinPull.obj.OnShootButtonCanceled();
        }
        
        ExecuteJump(_stats.JumpPower);
        
        // Activate jump kick start
        // if(!IsPulling && Mathf.Abs(_frameVelocity.x) >= _stats.MaxSpeed) {
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
        
        DustParticleMgr.obj.CreateDust(PlayerManager.PlayerType.SHADOW_TWIN);

        _sharedPlayerAudio.PlayJump();

        StartCoroutine(JumpSqueeze(_jumpSqueezeX, _jumpSqueezeY, _jumpSqueezeTime));
        _jumpToConsume = false;
    }

    public void JumpSqueeze() {
        StartCoroutine(JumpSqueeze(_jumpSqueezeX, _jumpSqueezeY, _jumpSqueezeTime));
    }
    private void ExecuteJump(float jumpPower)
    {
        isOnMoveable = false;
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _coyoteUsable = false;
        _frameVelocity.y = jumpPower;
    }

    private bool _justBounced = false;
    public void ApplyBounce(float bouncePower)
    {
        isOnMoveable = false;
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _coyoteUsable = false;
        _frameVelocity.y = _stats.JumpPower * bouncePower;
        _justBounced = true;
        isGrounded = false;
        _animator.Play("main_character_with_cape_jump", 0, 0);
    }

    #endregion

    #region Horizontal

    [Header("Surface Latch Configuration")]
    [SerializeField] private float _latchBoxCastDistanceHorizontal = 20f;
    [SerializeField] private float _latchBoxCastDistanceVertical = 20f;
    [SerializeField] private float _latchSpeed = 30f;
    [SerializeField] private LayerMask _latchLayerMask;

    private bool _isLatchedToSurface;
    private Vector2 _latchPosition;
    private Vector2 _latchDirection;
    private bool _latchReachedThisPull;
    private LatchSurfaceType _latchSurfaceType;

    public enum LatchSurfaceType
    {
        None,
        Ground,
        Wall,
        Ceiling
    }

    public void UpdateAnimatorIsLatchPulling(bool value) {
        _animator.SetBool("isLatchPulling", value);
    }

    public void UpdateAnimatorIsPulling(bool value) {
        _animator.SetBool("isPulling", value);
    }

    #region Surface Latch

    public bool IsLatchedToSurface()
    {
        return _isLatchedToSurface;
    }

    public bool IsWallJumping()
    {
        return _isWallJumping;
    }

    public float GetWallJumpDirection()
    {
        return _wallJumpDirection;
    }

    public LatchSurfaceType GetLatchSurfaceType()
    {
        return _latchSurfaceType;
    }

    public bool TryLatchToSurface(Vector2 direction)
    {
        if (_isLatchedToSurface || direction == Vector2.zero)
            return false;

        Vector2 boxSize = new Vector2(_collider.bounds.size.x, _collider.bounds.size.y);
        Vector2 origin = (Vector2)transform.position;
        
        // Use different cast distances for horizontal vs vertical lashing
        float castDistance = Mathf.Abs(direction.x) > 0 ? _latchBoxCastDistanceHorizontal : _latchBoxCastDistanceVertical;
        
        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0f, direction, castDistance, _latchLayerMask);
        
        if (hit.collider != null)
        {
            Vector2 playerPos = (Vector2)transform.position;

            ShadowLashBeamManager.obj.TriggerHitSurfaceAnimation();
            
            // Check if lashing upward to a floating platform ceiling
            bool isFloatingPlatform = hit.collider.gameObject.layer == LayerMask.NameToLayer("JumpThroughs") 
                                      && hit.collider.CompareTag("FloatingPlatform");
            LatchSurfaceType surfaceType = DetermineLatchSurfaceType(direction, hit.normal);
            
            // Set flag if this is a floating platform ceiling we're lashing to
            _isLatchingToFloatingPlatform = isFloatingPlatform && surfaceType == LatchSurfaceType.Ceiling && direction.y > 0;
            
            // Store reference to the floating platform if we're lashing to it
            if (_isLatchingToFloatingPlatform)
            {
                _targetFloatingPlatform = hit.collider.GetComponentInParent<FloatyPlatform>();
            }
            else
            {
                _targetFloatingPlatform = null;
            }
            
            // Align latch position to move purely horizontally or vertically
            if (Mathf.Abs(direction.x) > 0)
            {
                // Horizontal movement - keep player's Y position
                _latchPosition = new Vector2(hit.point.x, playerPos.y);
            }
            else
            {
                // Vertical movement - keep player's X position
                _latchPosition = new Vector2(playerPos.x, hit.point.y);
            }
            
            _latchDirection = direction;
            _latchSurfaceType = surfaceType;
            StartLatchPull();
            return true;
        }
        
        return false;
    }

    private LatchSurfaceType DetermineLatchSurfaceType(Vector2 direction, Vector2 surfaceNormal)
    {
        float angle = Vector2.Angle(Vector2.up, surfaceNormal);
        
        if (angle < 45f)
        {
            return LatchSurfaceType.Ground;
        }
        else if (angle > 135f)
        {
            return LatchSurfaceType.Ceiling;
        }
        else
        {
            return LatchSurfaceType.Wall;
        }
    }

    private void StartLatchPull()
    {
        _isLatchedToSurface = false;
        _latchReachedThisPull = false;
        _isLatchPulling = true;
        ShadowTwinPlayer.obj.DisableGravity();
        _ghostTrail.ShowGhosts();
    }

    public void EndLatchPull()
    {
        _isLatchedToSurface = false;
        _latchPosition = Vector2.zero;
        _latchDirection = Vector2.zero;
        _latchSurfaceType = LatchSurfaceType.None;
        _latchReachedThisPull = false;
        _isLatchingToFloatingPlatform = false;
        _targetFloatingPlatform = null;
        _isLatchPulling = false;
        UpdateAnimatorIsLatchPulling(false);
        ShadowTwinPlayer.obj.ResetGravity();
        
        ShadowTwinLash.obj.SetIsShadowLashing(false);
    }

    private void OnLatchReached()
    {
        CameraShakeManager.obj.ForcePushShake();
        _deeAudio.PlayAnchorReached();
        //ShockWaveManager.obj.CallShockWave(_latchPosition, 0.2f, 0.05f, 0.15f);
        _isLatchedToSurface = true;
    }

    private void StartPropelThroughPlatform()
    {
        _isPropellingThroughPlatform = true;
        _propelTimer = _propelThroughPlatformDuration;
        // Note: _propelVelocity is set before calling this method
        ShadowTwinPlayer.obj.DisableGravity();
        ShadowTwinLash.obj.SetIsShadowLashing(false);
        
        // Temporarily disable the floating platform's collider to prevent grounded detection
        if (_targetFloatingPlatform != null)
        {
            _targetFloatingPlatform.TemporarilyDisableCollider(_platformColliderDisableDuration);
        }
    }

    private void HandlePropelThroughPlatform()
    {
        _frameVelocity = _propelVelocity;
    }

    private void HandleLatchPullVelocity()
    {
        // For floating platforms, use distance check instead of collision check
        // because the player can pass through them
        bool hasReachedSurface;
        if (_isLatchingToFloatingPlatform)
        {
            // Check if we're close to the latch position
            float distanceToTarget = Vector2.Distance(transform.position, _latchPosition);
            hasReachedSurface = distanceToTarget < 0.5f; // Threshold distance
        }
        else
        {
            hasReachedSurface = CheckLatchSurfaceCollision();
        }

        if (hasReachedSurface)
        {
            if (!_latchReachedThisPull)
            {
                _latchReachedThisPull = true;
                
                // Check if this is a floating platform - if so, start propelling instead of latching
                if (_isLatchingToFloatingPlatform)
                {
                    // Capture the current lash velocity and start propelling
                    _propelVelocity = _frameVelocity;
                    StartPropelThroughPlatform();
                    // End the latch pull state
                    _latchPosition = Vector2.zero;
                    _latchDirection = Vector2.zero;
                    _latchReachedThisPull = false;
                    _isLatchPulling = false;
                    UpdateAnimatorIsLatchPulling(false);
                    _isLatchingToFloatingPlatform = false;
                    _targetFloatingPlatform = null; // Clear platform reference after starting propel
                    return;
                }
                
                OnLatchReached();
                
                // Check if lash button was released - if so, drop immediately after latching
                if (ShadowTwinLash.obj.WasLashButtonReleased())
                {
                    // Button was released - end the latch and drop
                    EndLatchPull();
                    return;
                }
            }
            
            _frameVelocity = Vector2.zero;
            return;
        }

        // Use constant speed in the latch direction
        _frameVelocity = _latchDirection * _latchSpeed;
    }

    private bool CheckLatchSurfaceCollision()
    {
        Vector2 boxSize = new Vector2(_collider.bounds.size.x, _collider.bounds.size.y);
        Vector2 origin = _collider.bounds.center;
        
        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0f, _latchDirection, 0.01f, _latchLayerMask);
        
        return hit.collider != null;
    }

    public void HandleLatchJump()
    {
        if (!_isLatchedToSurface)
            return;

        if (_latchSurfaceType == LatchSurfaceType.Ceiling)
        {
            EndLatchPull();
            _jumpToConsume = false;
            _latchedJumpToConsume = false;
        }
        else if (_latchSurfaceType == LatchSurfaceType.Wall)
        {
            ExecuteWallJump();
        }
        else if (_latchSurfaceType == LatchSurfaceType.Ground)
        {
            EndLatchPull();
            ExecuteJump(_stats.JumpPower);
            DustParticleMgr.obj.CreateDust(PlayerManager.PlayerType.SHADOW_TWIN);
            _sharedPlayerAudio.PlayJump();
            StartCoroutine(JumpSqueeze(_jumpSqueezeX, _jumpSqueezeY, _jumpSqueezeTime));
            _jumpToConsume = false;
            _latchedJumpToConsume = false;
        }
    }

    private void ExecuteWallJump()
    {
        // Determine which direction to jump away from the wall
        // The latch direction tells us which way we pulled towards the wall
        float horizontalDirection = -_latchDirection.x; // Jump opposite to the wall direction
        
        EndLatchPull();
        
        // Set wall jump state
        _isWallJumping = true;
        _wallJumpTimer = _wallJumpDirectionLockDuration;
        _wallJumpBoostTimer = _wallJumpBoostDuration;
        _wallJumpDirection = horizontalDirection;
        
        // Apply vertical jump power (reduced compared to regular jump)
        _frameVelocity.y = _wallJumpVerticalPower;
        
        // Apply horizontal jump power away from the wall
        _frameVelocity.x = horizontalDirection * _wallJumpHorizontalPower;
        
        // Reset jump state
        isOnMoveable = false;
        isGrounded = false;
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _coyoteUsable = false;
        
        // Visual and audio feedback
        //DustParticleMgr.obj.CreateDust(PlayerManager.PlayerType.SHADOW_TWIN);  //TODO need to create vertical dust close to the wall
        _sharedPlayerAudio.PlayJump();
        
        _jumpToConsume = false;
        _latchedJumpToConsume = false;
    }

    #endregion

    private void HandleDirection()
    {
                // --- Hit boost logic ---
        if (_isHit)
        {
            if (_hitBoostPhase == 1) // Rising
            {
                _hitBoostTimer += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(_hitBoostTimer / _hitBoostRiseTime);
                _currentHitBoost = Mathf.Lerp(0f, _hitBoostMax, t);
                if (t >= 1f)
                {
                    _hitBoostPhase = 2;
                    _hitBoostTimer = 0f;
                }
            }
            else if (_hitBoostPhase == 2) // Falling
            {
                _hitBoostTimer += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(_hitBoostTimer / _hitBoostFallTime);
                _currentHitBoost = Mathf.Lerp(_hitBoostMax, 0f, t);
                if (t >= 1f)
                {
                    _isHit = false;
                    _hitBoostPhase = 0;
                    _hitBoostTimer = 0f;
                    _currentHitBoost = 0f;
                }
            }
        }

        if(_freezePlayer) {
            _frameVelocity.x = 0;
            return;
        }

        // When controlling an object, stop player movement
        if(ShadowTwinPull.obj != null && ShadowTwinPull.obj.IsControllingObject) {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, _stats.GroundDeceleration * Time.fixedDeltaTime);
            return;
        }

        // During propel through platform, maintain horizontal velocity
        if(_isPropellingThroughPlatform) {
            // Horizontal velocity is already set in HandlePropelThroughPlatform
            // Just return to skip normal horizontal movement logic
            return;
        }

        // During wall jump, apply boost phase then transition to normal air control
        if(_isWallJumping) {
            // Check if player is pressing the same direction as wall jump
            bool pressingSameDirection = (_wallJumpDirection > 0 && _movementInput.x > 0) || 
                                        (_wallJumpDirection < 0 && _movementInput.x < 0);
            bool pressingOpposite = (_wallJumpDirection > 0 && _movementInput.x < 0) || 
                                   (_wallJumpDirection < 0 && _movementInput.x > 0);
            
            // During boost phase, always maintain full horizontal power
            if (_wallJumpBoostTimer > 0f)
            {
                // Boost phase - maintain wall jump power regardless of input
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _wallJumpDirection * _wallJumpHorizontalPower, _stats.Acceleration * Time.fixedDeltaTime);
            }
            // After boost phase, apply normal air control
            else if (pressingSameDirection)
            {
                // Maintain the wall jump horizontal velocity when pressing same direction
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _wallJumpDirection * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
            else if (pressingOpposite)
            {
                // Pressing opposite - use acceleration towards zero (like regular jump with input)
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _movementInput.x * _stats.MaxSpeed, _stats.AirDeceleration * Time.fixedDeltaTime);
            }
            else
            {
                // No input - use air deceleration (like regular jump without input)
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, _stats.AirDeceleration * Time.fixedDeltaTime);
            }
            return;
        }

        //Apply ground deceleration even no matter if player is grounded or not, since ground deceleration "feels" better in the air
        if(_isLatchPulling) {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, _stats.GroundDeceleration * Time.fixedDeltaTime);
            return;
        }
             
        if(_isDashing) {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _movementInput.x * _stats.MaxSpeed, dashDecelerationTime * Time.fixedDeltaTime);
        } else {
            float boost = _currentHitBoost * _hitBoostDirection;
            if (_movementInput.x == 0)
            {
                if(boost > 0) {
                    _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, (_hitBoostDirection * _stats.MaxSpeed) + boost, _stats.Acceleration * Time.fixedDeltaTime);    
                } else {    
                    var deceleration = IsEffectivelyGrounded() ? _stats.GroundDeceleration : _stats.AirDeceleration;
                    _frameVelocity.x = isOnMoveable && moveableRigidbody != null ?
                        moveableRigidbody.velocity.x :
                        Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
                }
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, (_movementInput.x * _stats.MaxSpeed) + (isOnMoveable && moveableRigidbody != null ? moveableRigidbody.velocity.x : 0) + boost, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        // Apply jump kick boost to horizontal frame velocity if active
        // if (_isJumpKickActive)
        // {
        //     _frameVelocity.x += _jumpKickHorizontal * _jumpKickDirection;
        // }
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if(isOnMoveable && moveableRigidbody != null) {
            _frameVelocity.y = moveableRigidbody.velocity.y;
            return;
        }
        if (IsEffectivelyGrounded() && _frameVelocity.y <= 0f)
        {
            // Don't apply grounding force if we just bounced
            if (!_justBounced)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                _justBounced = false;
            }
        }
        else
        {
            if (_isDashing)
            {
                //Just keep horizontal movement
                _frameVelocity.y = 0;
            }
            else if (_isPropellingThroughPlatform)
            {
                // Handle propel through platform - maintain velocity
                HandlePropelThroughPlatform();
                return;
            }
            else if (_isLatchPulling && _latchPosition != Vector2.zero)
            {
                // Handle latch pull velocity
                HandleLatchPullVelocity();
                return;
            }
            else if (_isLatchedToSurface)
            {
                // When latched, no gravity
                _frameVelocity.y = 0;
                return;
            }
            else
            {
                // Skip gravity deceleration if we just rounded a ceiling corner to maintain jump height
                if (!_roundedCeilingCornerThisFrame)
                {
                    var inAirGravity = _stats.FallAcceleration;
                    if (_endedJumpEarly && _frameVelocity.y > 0)
                        inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                    
                    // Apply floaty gravity modifier if in post-propel state
                    if (_isPostPropelFloaty)
                        inAirGravity *= _postPropelGravityModifier;

                    _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
                }
            }
        }
    }

    #endregion

    private void ApplyMovement() {
        if(ShadowTwinPlayer.obj.rigidBody.bodyType != RigidbodyType2D.Static) {
            ShadowTwinPlayer.obj.rigidBody.velocity = _frameVelocity;
        }
    } 

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * _stats.GrounderDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * _stats.RoofDistance);
        
        if(_collider != null) {
            Vector3 rightFootRayStart = _collider.bounds.center + new Vector3(_collider.size.x / 2, -_collider.size.y / 2 - feetCastOffset);
            Vector3 rightStepRayStart = _collider.bounds.center + new Vector3(_collider.size.x / 2, -_collider.size.y / 2 + stepHeight);
            Vector3 leftFootRayStart = _collider.bounds.center + new Vector3(-_collider.size.x / 2, -_collider.size.y / 2 - feetCastOffset);
            Vector3 leftStepRayStart = _collider.bounds.center + new Vector3(-_collider.size.x / 2, -_collider.size.y / 2 + stepHeight);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(rightFootRayStart, rightFootRayStart + Vector3.right * microLedgeForwardCastDistance);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(rightStepRayStart, rightStepRayStart + Vector3.right * microLedgeForwardCastDistance);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftFootRayStart, leftFootRayStart + Vector3.left * microLedgeForwardCastDistance);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(leftStepRayStart, leftStepRayStart + Vector3.left * microLedgeForwardCastDistance);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif
}