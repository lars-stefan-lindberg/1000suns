using System;
using System.Collections;
using UnityEngine;

public class FloatyPlatform : MonoBehaviour
{
    private BoxCollider2D _collider;
    [SerializeField] private BoxCollider2D _childCollider;
    private Rigidbody2D _rigidBody;
    private SpriteRenderer _spriteRenderer;
    private Pullable _pullable;

    public float idleMoveSpeed;
    //private float _idleVerticalTargetPosition;
    public bool isPlayer1OnPlatform = false;
    public bool isPlayer2OnPlatform = false;
    public float startingVerticalPosition;
    public float idleVerticalDistance = 0.25f;

    public float blockingCastDistance = 0.1f;
    public float basePushPower = 5f;
    public float deceleration = 20f;
    private LayerMask _blockingCastLayerMask;
    public bool movePlatform = false;
    public bool isFallingPlatform = false;
    public bool isFallingOnMovePlatform = false;
    public float timeBeforeFall = 1f;
    public float timeFallingBeforeDestroy = 7f;
    public float fallTimer = 0f;
    private bool _startFallCountDown = false;

    //private float _idleTargetVerticalPosition = 0;
    private float _prisonerPushPower = 2f;

    private FallingPlatformFlash _fallingPlatformFlash;

    private bool _respawning = false;
    private bool _isFallingOnMovePlatformFallStarted = false;
    public float spawnTime = 1f;
    public float spawnTimer = 0f;
    private Vector2 _startingPosition;

    private Color _fadeStartColor;
    [Range(0.1f, 10f), SerializeField] private float _fadeSpeed = 5f;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _blockingCastLayerMask = LayerMask.GetMask(new[] { "Ground", "Default", "JumpThroughs", "Enemies" });
        startingVerticalPosition = transform.position.y;
        //_idleVerticalTargetPosition = startingVerticalPosition - idleVerticalDistance;
        _fallingPlatformFlash = GetComponent<FallingPlatformFlash>();
        _startingPosition = transform.position;
        _fadeStartColor = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 0);
        _pullable = GetComponentInChildren<Pullable>();
    }

    public float _playerOffset = 0.1f;
    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.transform.CompareTag("FloatingPlatform"))
        {
            FloatyPlatform floatyPlatform = collider.GetComponentInParent<FloatyPlatform>();
            if(floatyPlatform.isPlayer1OnPlatform && floatyPlatform.IsFalling()) {
                collider.enabled = false;
                floatyPlatform._collider.enabled = false;
                RegisterPlayer1OnPlatform();
            }
            if(floatyPlatform.isPlayer2OnPlatform && floatyPlatform.IsFalling()) {
                collider.enabled = false;
                floatyPlatform._collider.enabled = false;
                RegisterPlayer2OnPlatform();
            }
        }
        if(collider.transform.CompareTag("Player"))
        {
            PlayerIdentity player = collider.GetComponent<PlayerIdentity>();
            if (player != null)
            {
                if (player.id == 1)
                    _isPlayer1CollisionTriggered = true;
                else if (player.id == 2)
                    _isPlayer2CollisionTriggered = true;
            }
        }
        if(collider.transform.CompareTag("Enemy")) {
            Prisoner prisoner = collider.gameObject.GetComponent<Prisoner>();
            if(prisoner.isGrounded) {
                bool hitFromRight = collider.transform.position.x > transform.position.x;
                MovePlatform(!hitFromRight, _prisonerPushPower);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.transform.CompareTag("Player"))
        {
            PlayerIdentity player = collider.GetComponent<PlayerIdentity>();
            if (player != null)
            {
                if (player.id == 1)
                {
                    isPlayer1OnPlatform = false;
                    _isPlayer1CollisionTriggered = false;
                }
                else if (player.id == 2)
                {
                    isPlayer2OnPlatform = false;
                    _isPlayer2CollisionTriggered = false;
                }
            }
        }
    }

    public bool IsFalling() {
        return fallTimer >= timeBeforeFall;
    }

    private void RegisterPlayer1OnPlatform() {
        isPlayer1OnPlatform = true;
        if(isFallingPlatform)
            _startFallCountDown = true;
        else if(isFallingOnMovePlatform)
            _fallingPlatformFlash.StartFlashingConstantSpeed();
    }

    private void RegisterPlayer2OnPlatform() {
        isPlayer2OnPlatform = true;
        if(isFallingPlatform)
            _startFallCountDown = true;
        else if(isFallingOnMovePlatform)
            _fallingPlatformFlash.StartFlashingConstantSpeed();
    }

    public bool somethingToTheRight = false;
    public bool somethingToTheLeft = false;
    private bool _startFlashing = true;

    private bool _isPlayer1CollisionTriggered = false;
    private bool _isPlayer2CollisionTriggered = false;
    private bool _isBeingPulled = false;

    private void Update()
    {
        if(_isPlayer1CollisionTriggered) {
            if(PlayerMovement.obj.isGrounded && IsPlayer1AbovePlatform()) {
                _isPlayer1CollisionTriggered = false;
                RegisterPlayer1OnPlatform();
            }
        } else if(_isPlayer2CollisionTriggered) {
            if(ShadowTwinMovement.obj.isGrounded && IsPlayer2AbovePlatform()) {
                _isPlayer2CollisionTriggered = false;
                RegisterPlayer2OnPlatform();
            }
        }

        bool isPullablePulled = _pullable != null && _pullable.IsPulled;
        bool wasJustPulled = isPullablePulled && !_isBeingPulled;
        bool wasJustReleased = !isPullablePulled && _isBeingPulled;
        if(wasJustPulled) {
            _isBeingPulled = true;
            movePlatform = true;
        } else if(wasJustReleased) {
            _isBeingPulled = false;
        }

        if(!_isBeingPulled) {
            if(_isFallingOnMovePlatformFallStarted) {
                fallTimer += Time.deltaTime;
            }
            if(_startFallCountDown) {
                fallTimer += Time.deltaTime;
                if(_startFlashing) {
                    _startFlashing = false;
                    _fallingPlatformFlash.StartFlashing(timeBeforeFall);
                }
            }
            if(fallTimer >= timeFallingBeforeDestroy) {
                StartRespawning();
            }
            if(isFallingPlatform && fallTimer >= timeBeforeFall) {
                _fallingPlatformFlash.StopFlashing();
                _rigidBody.bodyType = RigidbodyType2D.Dynamic;
                _rigidBody.gravityScale = 1;
                return;
            }
            if(_isFallingOnMovePlatformFallStarted) {
                //Skip the rest of the logic if the platform is falling
                return;
            }
            
            somethingToTheRight = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.right, blockingCastDistance, _blockingCastLayerMask);
            somethingToTheLeft = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.left, blockingCastDistance, _blockingCastLayerMask);

            if (somethingToTheRight && _rigidBody.velocity.x > 0) {
                movePlatform = false;
                SoundFXManager.obj.PlayFloatingPlatformWallHit(transform);
            }
            if (somethingToTheLeft && _rigidBody.velocity.x < 0) {
                movePlatform = false;
                SoundFXManager.obj.PlayFloatingPlatformWallHit(transform);
            }
        }

        if (movePlatform)
        {
            // If the platform is being pulled, let the external pull logic control velocity.
            if (!_isBeingPulled)
            {
                _rigidBody.velocity = new Vector2(
                    Mathf.MoveTowards(_rigidBody.velocity.x, 0, deceleration * Time.deltaTime),
                    _rigidBody.velocity.y);

                if(isFallingOnMovePlatform && !_isFallingOnMovePlatformFallStarted && !_respawning) {
                    _fallingPlatformFlash.StopFlashing();
                    _rigidBody.bodyType = RigidbodyType2D.Dynamic;
                    _rigidBody.gravityScale = 1;
                    _isFallingOnMovePlatformFallStarted = true;
                }
            }
        }
        else
        {
            _rigidBody.velocity = new Vector2(0, 0);
        }

        if(!_isBeingPulled && Mathf.Approximately(_rigidBody.velocity.x, 0f))
        {
            movePlatform = false;
        }

        if(_respawning) {
            spawnTimer += Time.deltaTime;
            if(spawnTimer >= spawnTime) {
                _respawning = false;
                transform.position = _startingPosition;
                StartCoroutine(FadeInSprite());
                _childCollider.enabled = true; //If it was set to false from another platform
                _collider.enabled = true;
            }
        }
        // if (!isPlayerOnPlatform && _rigidBody.velocity.x == 0)
        //     MoveIdlePlatform();
    }

    private bool IsPlayer1AbovePlatform()
    {
        return Player.obj.transform.position.y > transform.position.y;
    }

    private bool IsPlayer2AbovePlatform()
    {
        return ShadowTwinPlayer.obj.transform.position.y > transform.position.y;
    }

    // private void MoveIdlePlatform()
    // {
    //     if (transform.position.y >= startingVerticalPosition)
    //         _idleTargetVerticalPosition = _idleVerticalTargetPosition;
    //     if (transform.position.y <= _idleVerticalTargetPosition)
    //         _idleTargetVerticalPosition = startingVerticalPosition;
    //     transform.position = new Vector2(transform.position.x, Mathf.MoveTowards(transform.position.y, _idleTargetVerticalPosition, idleMoveSpeed * Time.deltaTime));
    // }

    public void MovePlatform(bool isFacingLeft, float force)
    {
        if(isFacingLeft) {
            somethingToTheRight = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.right, blockingCastDistance, _blockingCastLayerMask);
            if(somethingToTheRight)
                return;
        }
        if(!isFacingLeft) {
            somethingToTheLeft = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.left, blockingCastDistance, _blockingCastLayerMask);
            if(somethingToTheLeft)
                return;
        }

        movePlatform = true;
        float power = force * basePushPower;
        //To avoid decreasing the velocity
        if(power >= _rigidBody.velocity.magnitude)
            _rigidBody.velocity = new Vector2(isFacingLeft ? power : -power, 0);
    }

    private void StartRespawning() {
        _respawning = true;
        fallTimer = 0f;
        _startFallCountDown = false;
        spawnTimer = 0f;
        _rigidBody.velocity = new Vector3(0,0,0);
        _rigidBody.gravityScale = 0;
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
        _fadeStartColor.a = 0;
        _spriteRenderer.color = _fadeStartColor;
        isPlayer1OnPlatform = false;
        isPlayer2OnPlatform = false;
        _fallingPlatformFlash.StopFlashing();
        _startFlashing = true;
        _isFallingOnMovePlatformFallStarted = false;
    }

    private IEnumerator FadeInSprite() {
        while(_spriteRenderer.color.a < 1f) {
            _fadeStartColor.a += Time.deltaTime * _fadeSpeed;
            _spriteRenderer.color = _fadeStartColor;
            yield return null;
        }
    }

    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireCube(_collider.bounds.center, new Vector2(_collider.size.x * (1 + blockingCastDistance), _collider.size.y));
    // }
}
