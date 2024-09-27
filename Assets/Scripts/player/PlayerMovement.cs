using System;
using System.Collections;
using UnityEditor.VisionOS;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour, IPlayerController
{
    public static PlayerMovement obj;

    [SerializeField] private ScriptableStats _stats;
    private SpriteRenderer _spriteRenderer;
    public GameObject anchor;
    private BoxCollider2D _collider;
    private Animator _animator;
    private PlayerInput _playerInput;
    private Vector2 _frameVelocity;
    private bool _cachedQueryStartInColliders;

    //[Header("Dependecies")]
    public GameObject buildPowerJumpAnimation;

    private float _time;
    private bool _jumpHeldInput; 
    private Vector2 _movementInput;

    public bool _isFallDashing = false;
    public float dashDecelerationTime = 160f;
    public float initialDashSpeed = 40f;

    public bool isOnPlatform = false;
    public Rigidbody2D platformRigidBody;
    public JumpThroughPlatform jumpThroughPlatform;

    #region Interface
    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;
    #endregion

    private void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponentInChildren<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _powerJumpForce = _stats.JumpPower * 2f;
        _groundLayerMasks = LayerMask.GetMask("Ground");
        _platformLayerMasks = LayerMask.GetMask("JumpThroughs");
        _ceilingLayerMasks = LayerMask.GetMask("Ground");
        _playerInput = GetComponentInChildren<PlayerInput>();
    }

    private void OnDestroy()
    {
        obj = null;
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        BuildUpPowerJump();
        HandleJump();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
    }

    private void Update()
    {
        _time += Time.deltaTime;
        GatherInput();
        UpdateAnimator();
        FlipPlayer(_movementInput.x);
    }

    private void FlipPlayer(float _xValue)
    {
        if (_xValue < 0)
            _spriteRenderer.flipX = true;
        else if (_xValue > 0)
            _spriteRenderer.flipX = false;
    }

    public void FlipPlayer()
    {
        _spriteRenderer.flipX = !_spriteRenderer.flipX;
    }

    public bool isFacingLeft()
    {
        return _spriteRenderer.flipX;
    }

    public void ExecuteFallDash()
    {
        _isFallDashing = true;
        _frameVelocity.x = isFacingLeft() ? initialDashSpeed : -initialDashSpeed;
    }

    public void TriggerForcePushAnimation() {
        if(_movementInput.x != 0 && isGrounded)
            _animator.SetTrigger("forcePushWhileRunning");
        else
            _animator.SetTrigger("forcePush");
    }

    public void ExecuteForcePushJump() {
        isForcePushJumping = true;
        forcePushJumpOnGroundTimer = 0;
        _frameVelocity.x = isFacingLeft() ? -initialForcePushJumpSpeed : initialForcePushJumpSpeed;
        PlayerPush.obj.ResetBuiltUpPower();
        Player.obj.hasPowerUp = false;
        _animator.SetTrigger("forcePushJump");
    }

    public void ExecuteForcePushJumpWithProjectile() {
        isForcePushJumping = true;
        forcePushJumpOnGroundTimer = 0;
        _frameVelocity.x = isFacingLeft() ? initialForcePushJumpSpeed : -initialForcePushJumpSpeed;
    }

    public bool isFalling = false;
    public bool isMoving = false;
    private void UpdateAnimator()
    {
        _animator.SetBool("isGrounded", isGrounded);
        isMoving = _movementInput.x != 0;
        _animator.SetBool("isMoving", isMoving);
        isFalling = _frameVelocity.y < -_stats.MinimumFallAnimationSpeed;
        _animator.SetBool("isFalling", isFalling);
        if (_landed)
        {
            DustParticleMgr.obj.CreateDust();
            AudioManager.obj.PlayLand(Player.obj.surface, gameObject.transform);
            StartCoroutine(JumpSqueeze(_landedSqueezeX, _landedSqueezeY, _landedSqueezeTime));
            _landed = false;
        }
    }

    private bool _freezePlayer = false;
    public void Freeze(float freezeDuration) {
        DisablePlayerMovement();
        _freezePlayer = true;
        _movementInput = new Vector2(0,0);
        StartCoroutine(FreezeDuration(freezeDuration));
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

    public void OnMovement(InputAction.CallbackContext value)
    {
        _movementInput = value.ReadValue<Vector2>();

        if (_movementInput.y < 0 && isGrounded && !_buildingUpPowerJump) //Pressing down
        {
            if(StaminaMgr.obj.HasEnoughStamina(new StaminaMgr.PowerJump()))
            {
                _buildingUpPowerJump = true;
                _buildUpPowerJumpTime = 0;
                buildPowerJumpAnimation.GetComponent<BuildPowerJumpAnimationMgr>().Play();
            }
        }
        else if(_movementInput.y >= 0)
            CancelPowerJumpCharge();
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
            if (!PowerJumpMaxCharged)
            {
                if(_movementInput.x != 0 && PlayerPush.obj.IsFullyCharged() && Player.obj.hasPowerUp) {
                    _jumpToConsume = true;
                    ExecuteForcePushJump();
                }
                if (isGrounded)
                    _jumpToConsume = true;
                else
                {
                    if(StaminaMgr.obj.HasEnoughStamina(new StaminaMgr.AirJump()))
                        _airJumpToConsume = true;
                }
                _jumpHeldInput = true;
                _timeJumpWasPressed = _time;
            } else
            {
                ExecutePowerJump();
                CancelPowerJumpCharge();
            }
        }
        else if (context.canceled)
        {
            _jumpHeldInput = false;
        }
    }

    private void GatherInput()
    {
        if (_stats.SnapInput)
        {
            _movementInput.x = Mathf.Abs(_movementInput.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_movementInput.x);
            _movementInput.y = Mathf.Abs(_movementInput.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_movementInput.y);
        }
    }

    private void BuildUpPowerJump()
    {
        if (_buildingUpPowerJump && _buildUpPowerJumpTime < POWER_JUMP_MAX_CHARGED_TIME)
        {
            _buildUpPowerJumpTime += Time.deltaTime;
        }
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
    private LayerMask _groundLayerMasks;
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

        // Ground and Ceiling
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
        if (ceilingHit)
        {
            _frameVelocity.y *= _stats.CeilingBounceBackSpeed;
        }

        // Landed on the Ground
        if (!isGrounded && groundHit)
        {
            isGrounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            _numberOfAirJumps = 0;
            _airJumpToConsume = false;
            _powerJumpExecuted = false;
            _landed = true;
            jumpedWhileForcePushJumping = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));            
        }
        // Left the Ground
        else if (isGrounded && !groundHit)
        {
            isGrounded = false;
            isOnPlatform = false;
            _frameLeftGrounded = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    #endregion

    #region Jumping
    public bool isForcePushJumping = false;
    public bool jumpedWhileForcePushJumping = false;
    public float jumpedWhileForcePushJumpingModifier = 0.6f;
    public float initialForcePushJumpSpeed = 30f;
    public float forcePushJumpOnGroundDuration = 0.01f;
    public float forcePushJumpOnGroundTimer = 0f;
    private bool _jumpToConsume;
    private float _timeJumpWasPressed;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private bool _bufferedJumpUsable;
    private bool _airJumpToConsume = false;
    private int _numberOfAirJumps = 0;
    private bool _buildingUpPowerJump = false;
    private float _buildUpPowerJumpTime = 0;
    private float _powerJumpForce;
    private bool _powerJumpExecuted = false;
    private float _powerJumpAirGravityModifer = 0.4f;
    private const float POWER_JUMP_MAX_CHARGED_TIME = 0.5f;
    private const int MAX_NUMBER_OF_AIR_JUMPS = 1;

    private bool PowerJumpMaxCharged => _buildUpPowerJumpTime >= POWER_JUMP_MAX_CHARGED_TIME;
    private bool CanUseJump => (isGrounded || CanUseCoyote) && _jumpToConsume;
    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !isGrounded && _time < _frameLeftGrounded + _stats.CoyoteTime;
    private bool CanUseAirJump =>
        !isGrounded &&
        _time > _frameLeftGrounded + _stats.CoyoteTime &&
        _numberOfAirJumps < MAX_NUMBER_OF_AIR_JUMPS &&
        _airJumpToConsume &&
        !_powerJumpExecuted;

    private void HandleJump()
    {
        if (!_endedJumpEarly && !isGrounded && !_jumpHeldInput && Player.obj.rigidBody.velocity.y > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !CanUseAirJump && !HasBufferedJump) return;

        if (CanUseJump) ExecuteRegularJump();

        if (CanUseAirJump) ExecuteAirJump();
    }

    private void ExecutePowerJump()
    {
        ExecuteJump(_powerJumpForce);
        _powerJumpExecuted = true;
        StaminaMgr.obj.ExecutePower(new StaminaMgr.PowerJump());
    }

    private float _jumpSqueezeX = 0.8f;
    private float _jumpSqueezeY = 1.2f;
    private float _jumpSqueezeTime = 0.08f;

    private void ExecuteRegularJump()
    {
        ExecuteJump(_stats.JumpPower);
        DustParticleMgr.obj.CreateDust();
        AudioManager.obj.PlayJump(gameObject.transform);
        StartCoroutine(JumpSqueeze(_jumpSqueezeX, _jumpSqueezeY, _jumpSqueezeTime));
        _jumpToConsume = false;
        if(isForcePushJumping) {
            jumpedWhileForcePushJumping = true;
        }
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
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = jumpPower;
        Jumped?.Invoke();
        CancelPowerJumpCharge();
    }

    private void CancelPowerJumpCharge()
    {
        _buildUpPowerJumpTime = 0;
        _buildingUpPowerJump = false;
        buildPowerJumpAnimation.GetComponent<BuildPowerJumpAnimationMgr>().Stop();
    }

    #endregion

    #region Horizontal

    private void HandleDirection()
    {
        if (isForcePushJumping) {
            forcePushJumpOnGroundTimer += Time.fixedDeltaTime;
            if(forcePushJumpOnGroundTimer > forcePushJumpOnGroundDuration) 
                isForcePushJumping = false;
        }
        
        if (_isFallDashing)
        {
            //Change horizontal movement while dashing
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, dashDecelerationTime * Time.fixedDeltaTime);
            if(_frameVelocity.x == 0)
            {
                _isFallDashing = false;
            }
        } else
        {
            if(jumpedWhileForcePushJumping) {
                //Time to defy physics and keep horizontal velocity
                
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
            if (_isFallDashing)
            {
                //Just keep horizontal movement
                _frameVelocity.y = 0;
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_powerJumpExecuted)
                    inAirGravity *= _powerJumpAirGravityModifer;
                if (jumpedWhileForcePushJumping)
                    inAirGravity *= jumpedWhileForcePushJumpingModifier;
                if (_endedJumpEarly && _frameVelocity.y > 0)
                    inAirGravity *= _stats.JumpEndEarlyGravityModifier;

                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }
    }

    #endregion

    private void ApplyMovement() {
        if(Player.obj.rigidBody.bodyType != RigidbodyType2D.Static) {
            if (_freezePlayer)
                Player.obj.rigidBody.velocity = new Vector2(0,0);
            else 
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

public interface IPlayerController
{
    public event Action<bool, float> GroundedChanged;

    public event Action Jumped;
}
