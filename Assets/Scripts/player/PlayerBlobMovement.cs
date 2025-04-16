using System;
using System.Collections;
using FunkyCode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBlobMovement : MonoBehaviour
{
    public static PlayerBlobMovement obj;

    [SerializeField] private GameObject _player;
    public GameObject anchor;
    public SpriteRenderer spriteRenderer;
    [SerializeField] private ScriptableStats _stats;
    [SerializeField] private LayerMask _groundLayerMasks;
    private LayerMask _ceilingLayerMasks;

    private BoxCollider2D _collider;
    private PlayerInput _playerInput;
    private Animator _animator;
    private Vector2 _frameVelocity;
    public bool isGrounded;
    private bool _landed = false;
    private float _frameLeftGrounded = float.MinValue;
    private float _time;
    private bool _cachedQueryStartInColliders;
    private bool _stopCollisions = false;
    private bool _stopMovement = false;
    private Vector2 _movementInput;
    private bool _freezePlayer = false;

    //Not great but it's hard to get this dynamically since the player object is always disabled when player blob is active
    private readonly float _playerColliderHeight = 1.642559f;  

    void Awake() {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
        _ceilingLayerMasks = LayerMask.GetMask("Ground");
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void OnDestroy() {
        obj = null;
    }

    void OnEnable() {
        //Reset transform from any previous squeeze
        anchor.transform.localScale = Vector3.one;
    }

    public void OnMovement(InputAction.CallbackContext value)
    {
        _movementInput = value.ReadValue<Vector2>();
        if(_movementInput.y > 0 && value.performed) {
            if(!PlayerPowersManager.obj.CanTurnFromBlobToHuman) {
                return;
            }
            if(!IsEnoughSpaceForPlayer()) {
                return;
            }
            PlayerBlob.obj.rigidBody.velocity = new Vector2(0,0);
            _frameVelocity = new Vector2(0,0);
            gameObject.SetActive(false);
            _player.transform.position = transform.position;
            if(isGrounded) {
                _player.GetComponent<PlayerMovement>().SetStartingOnGround();
                _player.GetComponent<PlayerMovement>().isGrounded = true;
            }
            _player.GetComponent<PlayerMovement>().spriteRenderer.flipX = IsFacingLeft();
            _player.SetActive(true);
            _player.GetComponent<PlayerMovement>().UnFreeze();
            _player.GetComponent<Player>().PlayToPlayerAnimation();
        }
    }

    public bool IsEnoughSpaceForPlayer()
    {
        // Get the collider bounds of the blob
        Bounds blobBounds = _collider.bounds;
        
        // Calculate the bottom left and bottom right corners of the collider
        Vector2 bottomLeft = new Vector2(blobBounds.min.x, blobBounds.min.y);
        Vector2 bottomRight = new Vector2(blobBounds.max.x, blobBounds.min.y);
        
        // Get the player's collider height to determine how far to cast the rays
        float rayDistance = _playerColliderHeight;
        
        // Direction for the raycasts (upward)
        Vector2 rayDirection = Vector2.up;
        
        // Debug visualization
        // Debug.DrawRay(bottomLeft, rayDirection * rayDistance, Color.red, 2f);
        // Debug.DrawRay(bottomRight, rayDirection * rayDistance, Color.red, 2f);
        
        // Perform the raycasts
        RaycastHit2D leftHit = Physics2D.Raycast(bottomLeft, rayDirection, rayDistance, _groundLayerMasks);
        RaycastHit2D rightHit = Physics2D.Raycast(bottomRight, rayDirection, rayDistance, _groundLayerMasks);
        
        // If either raycast hits something, there's not enough space
        bool hasEnoughSpace = !leftHit && !rightHit;
        
        return hasEnoughSpace;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(!PlayerPowersManager.obj.BlobCanJump) {
            return;
        }
        if (context.performed)
        {
            if (isGrounded || CanUseCoyote)
                _jumpToConsume = true;
            _jumpHeldInput = true;
            _timeJumpWasPressed = _time;
        }
        else if (context.canceled)
        {
            if(!_airJumpPerformed)
                _jumpHeldInput = false;
        }
    }

    public void ExecuteChargedJump() {
        if(isGrounded || CanUseCoyote) {
            _jumpToConsume = true;
            _jumpHeldInput = true;
            _timeJumpWasPressed = _time;
        } else {
            _airJumpToConsume = true;
            _jumpHeldInput = true;
            _timeJumpWasPressed = _time;
        }
    }
        
    public void CancelJumping() {
        _jumpHeldInput = false;
        _jumpToConsume = false;
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
        GatherInput();
        UpdateAnimator();
        FlipPlayer(_movementInput.x);
    }

    public void FlipPlayer(float _xValue)
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

    public bool startingOnGround = true;
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

        bool ceilingHit = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.up, _stats.RoofDistance, _ceilingLayerMasks);

        // Hit a Ceiling
        if (ceilingHit)
        {
            _frameVelocity.y *= _stats.CeilingBounceBackSpeed;
        }

        // Landed on the Ground
        if (!isGrounded && groundHit && PlayerBlob.obj.rigidBody.velocity.y <= 0.05f)
        {
            isGrounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            _landed = true;
            _airJumpToConsume = false;
            _airJumpPerformed = false;

            //To avoid "double grounded". Sometimes when player barely reaches up on edge it gets grounded, but still has upwards velocity, and lands again.
            _frameVelocity.y = 0; 
        }
        // Left the Ground
        else if (isGrounded && !groundHit)
        {
            isGrounded = false;
            _frameLeftGrounded = _time;
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    private bool _jumpToConsume;
    private bool _airJumpToConsume = false;
    private bool _airJumpPerformed = false;
    private float _timeJumpWasPressed;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private bool _bufferedJumpUsable;
    private bool _jumpHeldInput; 
    private bool CanUseJump => (isGrounded || CanUseCoyote) && _jumpToConsume;
    private bool CanUseCoyote => _coyoteUsable && !isGrounded && _time < _frameLeftGrounded + _stats.CoyoteTime;
    private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseAirJump => _airJumpToConsume && !isGrounded && _time > _frameLeftGrounded + _stats.CoyoteTime;

    private void HandleJump()
    {
        if (!_endedJumpEarly && !isGrounded && !_jumpHeldInput && PlayerBlob.obj.rigidBody.velocity.y > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !CanUseAirJump && !HasBufferedJump) return;

        if (CanUseJump) ExecuteRegularJump();
        if (CanUseAirJump) ExecuteAirJump();
    }

    private void ExecuteAirJump()
    {
        ExecuteJump(_stats.JumpPower);
        _airJumpToConsume = false;
        _airJumpPerformed = true;
    }

    private void ExecuteRegularJump()
    {
        ExecuteJump(_stats.JumpPower);
        DustParticleMgr.obj.CreateDust();
        SoundFXManager.obj.PlayJump(gameObject.transform);
        StartCoroutine(JumpSqueeze(_jumpSqueezeX, _jumpSqueezeY, _jumpSqueezeTime));
        _jumpToConsume = false;
    }

    private float _jumpSqueezeX = 0.8f;
    private float _jumpSqueezeY = 1.2f;
    private float _jumpSqueezeTime = 0.08f;
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

    private void ExecuteJump(float jumpPower)
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _frameVelocity.y = jumpPower;
    }

    private void HandleDirection()
    {
        if(_freezePlayer) {
            _frameVelocity.x = 0;
            return;
        }
        
        if (_movementInput.x == 0)
        {
            var deceleration = isGrounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _movementInput.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleGravity()
    {
        if (isGrounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = _stats.GroundingForce;
        }
        else
        {
            var inAirGravity = _stats.FallAcceleration;
            if (_endedJumpEarly && _frameVelocity.y > 0)
                inAirGravity *= _stats.JumpEndEarlyGravityModifier;

            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    private bool _isTransitioningBetweenLevels = false;

    public bool IsTransitioningBetweenLevels() {
        return _isTransitioningBetweenLevels;
    }

    public void TransitionToNextRoom(PlayerManager.PlayerDirection direction) {
        StartCoroutine(TransitionToNextRoomCoroutine(direction));
    }

    private float _transitionDistanceX = 1;
    private float _transitionDistanceUp = 2.5f;
    private float _transitionDistanceDown = 1.5f;
    private IEnumerator TransitionToNextRoomCoroutine(PlayerManager.PlayerDirection direction) {
        float target = 0;
        if(direction == PlayerManager.PlayerDirection.LEFT || direction == PlayerManager.PlayerDirection.RIGHT) {
            if(direction == PlayerManager.PlayerDirection.RIGHT)
                target = transform.position.x + _transitionDistanceX;
            if(direction == PlayerManager.PlayerDirection.LEFT)
                target = transform.position.x - _transitionDistanceX;
            while(transform.position.x != target) {
                transform.position = new Vector2(Mathf.MoveTowards(transform.position.x, target, Time.deltaTime * 5f), transform.position.y);
                yield return null;
            }
        } else if(direction == PlayerManager.PlayerDirection.UP || direction == PlayerManager.PlayerDirection.DOWN) {
            if(direction == PlayerManager.PlayerDirection.UP)
                target = transform.position.y + _transitionDistanceUp;
            if(direction == PlayerManager.PlayerDirection.DOWN)
                target = transform.position.y - _transitionDistanceDown;
            while(transform.position.y != target) {
                transform.position = new Vector2(transform.position.x, Mathf.MoveTowards(transform.position.y, target, Time.deltaTime * 5f));
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f);
        EnablePlayerAfterLevelTransition();
        yield return null;
    }

    public void SetTransitioningBetweenLevels() {
        //Special case since we want to handle "shoot" action separately. You should still be able to charge, but not release in between levels
        _playerInput.currentActionMap.FindAction("Movement").Disable();
        _playerInput.currentActionMap.FindAction("Jump").Disable();
        _freezePlayer = true;
        _movementInput = new Vector2(0,0);
        
        _isTransitioningBetweenLevels = true;
        
        _stopMovement = true;
        _stopCollisions = true;
        PlayerBlob.obj.rigidBody.gravityScale = 0;
        _animator.speed = 0;
    }

    public void EnablePlayerAfterLevelTransition() {
        UnFreeze();
        PlayerBlob.obj.rigidBody.gravityScale = 1;
        _animator.speed = 1;
        _stopMovement = false;
        _stopCollisions = false;

        _isTransitioningBetweenLevels = false;
    }

    

    
    public void Freeze(float freezeDuration) {
        DisablePlayerMovement();
        _freezePlayer = true;
        _movementInput = new Vector2(0,0);
        StartCoroutine(FreezeDuration(freezeDuration));
    }

    private IEnumerator FreezeDuration(float freezeDuration) {
        yield return new WaitForSeconds(freezeDuration);
        _freezePlayer = false;
        EnablePlayerMovement();
    }

    public bool IsFrozen() {
        return _freezePlayer;
    }
    
    public void Freeze() {
        DisablePlayerMovement();
        _freezePlayer = true;
        _movementInput = new Vector2(0,0);
    }
    
    public void UnFreeze() {
        _freezePlayer = false;
        EnablePlayerMovement();
    }

    public void EnablePlayerMovement() {
        _playerInput.currentActionMap.FindAction("Movement").Enable();
        _playerInput.currentActionMap.FindAction("Jump").Enable();
        _playerInput.currentActionMap.FindAction("Shoot").Enable();
    }

    public void DisablePlayerMovement() {
        _playerInput.currentActionMap.FindAction("Movement").Disable();
        _playerInput.currentActionMap.FindAction("Jump").Disable();
        _playerInput.currentActionMap.FindAction("Shoot").Disable();
    }

    private void ApplyMovement() {
        if(PlayerBlob.obj.rigidBody.bodyType != RigidbodyType2D.Static) {
            PlayerBlob.obj.rigidBody.velocity = _frameVelocity;
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

    public bool isFalling = false;
    public bool isMoving = false;
    private float _landedSqueezeX = 1.25f;
    private float _landedSqueezeY = 0.65f;
    private float _landedSqueezeTime = 0.08f;
    private void UpdateAnimator()
    {
        _animator.SetBool("isGrounded", isGrounded);
        isMoving = _movementInput.x != 0;
        _animator.SetBool("isMoving", isMoving);
        isFalling = _frameVelocity.y < -_stats.MinimumFallAnimationSpeed;
        //_animator.SetBool("isFalling", isFalling);
        if (_landed)
        {
            DustParticleMgr.obj.CreateDust();
            SoundFXManager.obj.PlayLand(PlayerBlob.obj.surface, gameObject.transform);
            StartCoroutine(JumpSqueeze(_landedSqueezeX, _landedSqueezeY, _landedSqueezeTime));
            _landed = false;
        }
    }

    public void LandingSqueeze() {
        StartCoroutine(JumpSqueeze(_landedSqueezeX, _landedSqueezeY, _landedSqueezeTime));
    }
}
