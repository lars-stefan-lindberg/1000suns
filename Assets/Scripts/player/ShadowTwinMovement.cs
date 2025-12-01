using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShadowTwinMovement : MonoBehaviour
{
    // --- Jump Kick Start fields ---
    private bool _isJumpKickActive = false;
    private float _jumpKickTimer = 0f;
    public float _jumpKickDuration = 0.1f; // seconds
    public float _jumpKickHorizontal = 4f; // tune as needed
    private float _jumpKickDirection = 1f;
    // --------------------------------
    public static ShadowTwinMovement obj;

    public bool isDevMode = true;
    [SerializeField] private ScriptableStats _stats;
    [SerializeField] private GameObject _playerTwin;
    [SerializeField] private GameObject _playerBlob;
    [SerializeField] private GhostTrailManager _ghostTrail;
    
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

    public bool isOnPlatform = false;
    public Rigidbody2D platformRigidBody;
    public JumpThroughPlatform jumpThroughPlatform;

    private void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _platformLayerMasks = LayerMask.GetMask("JumpThroughs");
        _ceilingLayerMasks = LayerMask.GetMask("Ground");
        _playerInput = GetComponent<PlayerInput>();
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
        if (_isJumpKickActive)
        {
            if(_jumpKickDirection != _movementInput.x){
                _isJumpKickActive = false;
            } else {
                _jumpKickTimer -= Time.deltaTime;
                if (_jumpKickTimer <= 0f)
                    _isJumpKickActive = false;
            }
        }
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

    public bool isFacingLeft()
    {
        return spriteRenderer.flipX;
    }

    public float _poweredDashMultiplier = 1.2f;
    public float partialDashPower = 35;
    public void ExecuteDash(ShadowTwinPull.PullPowerType chargePower)
    {
        _isDashing = true;
        float speed = 0;
        if(chargePower == ShadowTwinPull.PullPowerType.Powered) {
            speed = initialDashSpeed * _poweredDashMultiplier;
            ShadowTwinPlayer.obj.SetHasPowerUp(false);
        } else if(chargePower == ShadowTwinPull.PullPowerType.Partial) {
            speed = partialDashPower;
        } else if(chargePower == ShadowTwinPull.PullPowerType.Full) {
            speed = initialDashSpeed;
        }
        _frameVelocity.x = isFacingLeft() ? -speed : speed;
        //_ghostTrail.ShowGhosts();
    }

    public void EndDash() {
        _isDashing = false;
    }

    public void TriggerForcePullAnimation() {
        _animator.SetTrigger("forcePush");
    }

    public void TriggerEndForcePullAnimation() {
        _animator.SetTrigger("endPull");
    }

    public bool isFalling = false;
    public bool isMoving = false;
    public bool IsPulling = false;

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
        if (_landed)
        {
            DustParticleMgr.obj.CreateDust();
            SoundFXManager.obj.PlayLand(ShadowTwinPlayer.obj.surface, gameObject.transform);
            StartCoroutine(JumpSqueeze(_landedSqueezeX, _landedSqueezeY, _landedSqueezeTime));
            _landed = false;
        }
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

    public void UnFreeze() {
        _freezePlayer = false;
        EnablePlayerMovement();
    }

    private bool _isTransitioningBetweenLevels = false;
    public void SetTransitioningBetweenLevels() {
        //Special case since we want to handle "shoot" action separately. You should still be able to charge, but not release in between levels
        _playerInput.currentActionMap.FindAction("Movement").Disable();
        _playerInput.currentActionMap.FindAction("Jump").Disable();
        _freezePlayer = true;
        _movementInput = new Vector2(0,0);
        
        _isTransitioningBetweenLevels = true;
        
        _stopMovement = true;
        _stopCollisions = true;
        ShadowTwinPlayer.obj.rigidBody.gravityScale = 0;
        _animator.speed = 0;
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
    private float _transitionDistanceUp = 2.5f;
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
            if(direction == PlayerManager.PlayerDirection.UP)
                target = transform.position.y + _transitionDistanceUp;
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
        _playerInput.currentActionMap.FindAction("Movement").Disable();
        _playerInput.currentActionMap.FindAction("Jump").Disable();
        _playerInput.currentActionMap.FindAction("Shoot").Disable();
    }

    public void EnablePlayerMovement() {
        _playerInput.currentActionMap.FindAction("Movement").Enable();
        _playerInput.currentActionMap.FindAction("Jump").Enable();
        _playerInput.currentActionMap.FindAction("Shoot").Enable();
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

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(jumpThroughPlatform != null &&
                _movementInput.y < 0) {
                jumpThroughPlatform.PassThrough();
                return;
            }

            if (isGrounded || CanUseCoyote) {
                _jumpToConsume = true;
            }
            else
            {
                if(StaminaMgr.obj.HasEnoughStamina(new StaminaMgr.AirJump()))
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

    public bool isTransforming = false;
    public void OnSwitch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            HandleSwitchCharacter();
        }
    }

    private void HandleSwitchCharacter() {
        if(isTransforming)
            return;

        if(PlayerPowersManager.obj.CanSwitchBetweenTwinsMerged && !PlayerManager.obj.IsSeparated) {
            //Switch to shadow twin
            SoundFXManager.obj.PlayPlayerShapeshiftToBlob(transform);
            isTransforming = true;
            IsPulling = false;
            ShadowTwinPull.obj.CancelPulling();
            //Player.obj.PlaySwitchToTwinAnimation();
            if(PlayerManager.obj.IsEliInBlobForm()) {
                ToBlob();
            } else {
                ToTwin();
            }
        } else if(PlayerPowersManager.obj.CanSeparate) {
            if(PlayerManager.obj.IsEliInBlobForm()) {
                PlayerSwitcher.obj.SwitchToBlob();
            } else {
                PlayerSwitcher.obj.SwitchToEli();
            }
        }
    }

    [SerializeField] private float _mergeSplitHoldDuration = 0.5f;
    private bool _mergeSplitHeld = false;
    private float _mergeSplitHoldTimer = 0f;

    public void OnMergeSplit(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _mergeSplitHeld = true;
            _mergeSplitHoldTimer = 0f;
        }
        else if (context.canceled)
        {
            //If the merge button is not held we should just switch character instead.
            //If the merge button was held, the execution is done from Update method
            bool mergeButtonNotHeld = _mergeSplitHeld && _mergeSplitHoldTimer < _mergeSplitHoldDuration;
            _mergeSplitHeld = false;
            _mergeSplitHoldTimer = 0f;

            if (mergeButtonNotHeld)
            {
                HandleSwitchCharacter();
            }
        }
    }

    private void PerformMergeSplit()
    {
        if(PlayerPowersManager.obj.CanSeparate) {
            if(PlayerManager.obj.IsSeparated) {
                //Merge
                if(PlayerManager.obj.IsEliInBlobForm()) {
                    _playerBlob.SetActive(false);                        
                } else {
                    _playerTwin.SetActive(false);
                }
                PlayerManager.obj.IsSeparated = false;
            } else {
                //Split
                if(PlayerManager.obj.IsEliInBlobForm()) {
                    _playerBlob.transform.position = transform.position - new Vector3(0, 0.5f, 0);
                    _playerBlob.SetActive(true);
                    PlayerSwitcher.obj.SwitchToBlob();
                } else {
                    _playerTwin.SetActive(true);
                    _playerTwin.transform.position = transform.position;
                    PlayerSwitcher.obj.SwitchToEli();
                }
                PlayerManager.obj.IsSeparated = true;
            }
        }
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
        _playerTwin.GetComponent<PlayerMovement>().spriteRenderer.flipX = isFacingLeft();
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
        isTransforming = false;
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
        _playerBlob.GetComponent<PlayerBlobMovement>().spriteRenderer.flipX = isFacingLeft();
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
    private float _landedSqueezeX = 1.25f;
    private float _landedSqueezeY = 0.65f;
    private float _landedSqueezeTime = 0.08f;
    private bool _landed = false;
    [SerializeField] private LayerMask _groundLayerMasks;
    private LayerMask _platformLayerMasks;
    private LayerMask _ceilingLayerMasks;

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

        bool groundHit = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.down, _stats.GrounderDistance, _groundLayerMasks);
        
        //Corner case when spawning
        if(startingOnGround) {
            groundHit = true;
            if(!_startingOnGroundFalseCoroutineStarted) {
                _startingOnGroundFalseCoroutineStarted = true;
                StartCoroutine(SetStartingOnGroundToFalse());
            }
        }
 
        bool platformHit = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.down, _stats.GrounderDistance, _platformLayerMasks);
        bool ceilingHit = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.up, _stats.RoofDistance, _ceilingLayerMasks);

        if(platformHit) {
            groundHit = true;
            isOnPlatform = true;
        }

        // Hit a Ceiling
        if (ceilingHit && !groundHit)
        {
            HandleCeilingCollisions();
        }

        // Landed on the Ground
        if (!isGrounded && groundHit && ShadowTwinPlayer.obj.rigidBody.velocity.y <= 0.05f)
        {
            isGrounded = true;
            _coyoteUsable = true;
            _endedJumpEarly = false;
            _numberOfAirJumps = 0;
            _airJumpToConsume = false;
            _landed = true;
            isFalling = false;

            //To avoid "double grounded". Sometimes when player barely reaches up on edge it gets grounded, but still has upwards velocity, and lands again.
            _frameVelocity.y = 0; 
        }
        // Left the Ground
        else if (isGrounded && !groundHit)
        {
            isGrounded = false;
            isOnPlatform = false;
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

        if(ceilingHitRight && ceilingHitLeft) {
            _frameVelocity.y *= _stats.CeilingBounceBackSpeed;
        } else if(ceilingHitRight) {
            bool isAirToTheLeft = !Physics2D.Raycast(topRight - new Vector2(0.25f, 0), Vector2.up, _stats.RoofDistance, _ceilingLayerMasks);
            if(isAirToTheLeft) {
                transform.position = new Vector2(transform.position.x - 0.125f, transform.position.y);
            } else {
                _frameVelocity.y *= _stats.CeilingBounceBackSpeed;
            }
        } else if(ceilingHitLeft) {
            bool isAirToTheRight = !Physics2D.Raycast(topLeft + new Vector2(0.25f, 0), Vector2.up, _stats.RoofDistance, _ceilingLayerMasks);
            if(isAirToTheRight) {
                transform.position = new Vector2(transform.position.x + 0.125f, transform.position.y);
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
    private bool _airJumpToConsume = false;
    private int _numberOfAirJumps = 0;
    private const int MAX_NUMBER_OF_AIR_JUMPS = 1;

    private bool CanUseJump => (isGrounded || CanUseCoyote) && _jumpToConsume;
    private bool HasBufferedJump => _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !isGrounded && _time < _frameLeftGrounded + _stats.CoyoteTime;
    private bool CanUseAirJump =>
        isDevMode &&
        !isGrounded &&
        _time > _frameLeftGrounded + _stats.CoyoteTime &&
        _numberOfAirJumps < MAX_NUMBER_OF_AIR_JUMPS &&
        _airJumpToConsume;

    private void HandleJump()
    {
        if (!_endedJumpEarly && !isGrounded && !_jumpHeldInput && ShadowTwinPlayer.obj.rigidBody.velocity.y > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !CanUseAirJump && !HasBufferedJump) return;

        if(HasBufferedJump && isGrounded) {
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
        if(!_isDashing && Mathf.Abs(_frameVelocity.x) >= _stats.MaxSpeed) {
            _isJumpKickActive = true;
            _jumpKickTimer = _jumpKickDuration;
            _jumpKickDirection = isFacingLeft() ? -1f : 1f;
        }

        if(_isDashing) {
            //Reset the high speed of the dash
            if(_frameVelocity.x > _stats.MaxSpeed) {
                _frameVelocity.x = _stats.MaxSpeed;  
            } else if(_frameVelocity.x < -_stats.MaxSpeed) {
                _frameVelocity.x = -_stats.MaxSpeed;
            }
            _isDashing = false;
        }
        
        DustParticleMgr.obj.CreateDust();

        SoundFXManager.obj.PlayJump(gameObject.transform);

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
        StaminaMgr.obj.ExecutePower(new StaminaMgr.AirJump());
    }

    private void ExecuteJump(float jumpPower)
    {
        isOnPlatform = false;
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

        if(IsPulling && isGrounded) {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, _stats.GroundDeceleration * Time.fixedDeltaTime);
            return;
        }
             
        if(_isDashing) {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _movementInput.x * _stats.MaxSpeed, dashDecelerationTime * Time.fixedDeltaTime);
        } else {
            if (_movementInput.x == 0)
            {
                var deceleration = isGrounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = isOnPlatform && platformRigidBody != null ?
                    platformRigidBody.velocity.x :
                    Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, (_movementInput.x * _stats.MaxSpeed) + (isOnPlatform && platformRigidBody != null ? platformRigidBody.velocity.x : 0), _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        // Apply jump kick boost to horizontal frame velocity if active
        if (_isJumpKickActive)
        {
            _frameVelocity.x += _jumpKickHorizontal * _jumpKickDirection;
        }
    }

    #endregion

    #region Gravity

    private void HandleGravity()
    {
        if(isOnPlatform && platformRigidBody != null) {
            _frameVelocity.y = platformRigidBody.velocity.y;
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
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0)
                    inAirGravity *= _stats.JumpEndEarlyGravityModifier;

                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
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
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
    }
#endif
}