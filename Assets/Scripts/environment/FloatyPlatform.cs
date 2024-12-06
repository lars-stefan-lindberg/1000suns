using System.Collections;
using UnityEngine;

public class FloatyPlatform : MonoBehaviour
{
    private BoxCollider2D _collider;
    private Rigidbody2D _rigidBody;
    private SpriteRenderer _spriteRenderer;

    public float idleMoveSpeed;
    private float _idleVerticalTargetPosition;
    public bool isPlayerOnPlatform = false;
    public float startingVerticalPosition;
    public float idleVerticalDistance = 0.25f;

    public float blockingCastDistance = 0.1f;
    public float basePushPower = 5f;
    public float deceleration = 20f;
    private LayerMask _blockingCastLayerMask;
    public bool movePlatform = false;
    public bool isFallingPlatform = false;
    public float timeBeforeFall = 1f;
    public float timeFallingBeforeDestroy = 7f;
    public float fallTimer = 0f;
    private bool _startFallCountDown = false;

    private float _idleTargetVerticalPosition = 0;
    private float _prisonerPushPower = 2f;

    private FallingPlatformFlash _fallingPlatformFlash;

    private bool _respawning = false;
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
        _blockingCastLayerMask = LayerMask.GetMask(new[] { "Ground", "Default", "JumpThroughs" });
        startingVerticalPosition = transform.position.y;
        _idleVerticalTargetPosition = startingVerticalPosition - idleVerticalDistance;
        _fallingPlatformFlash = GetComponent<FallingPlatformFlash>();
        _startingPosition = transform.position;
        _fadeStartColor = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.CompareTag("FloatingPlatform"))
        {
            FloatyPlatform floatyPlatform = collision.gameObject.GetComponent<FloatyPlatform>();
            if(floatyPlatform.isPlayerOnPlatform && floatyPlatform.IsFalling()) {
                floatyPlatform._collider.enabled = false;
                RegisterPlayerOnPlatform();
            }
        }
        if(collision.transform.CompareTag("Player"))
        {
            //Check if player is landing on top of platform
            Bounds playerBounds = collision.collider.bounds;
            Vector2 playerBottom = new(playerBounds.center.x, playerBounds.center.y - playerBounds.extents.y);
            Bounds platformBounds = _collider.bounds;
            Vector2 platformTop = new(platformBounds.center.x, platformBounds.center.y + platformBounds.extents.y);
            if(platformTop.y < playerBottom.y)
                RegisterPlayerOnPlatform();
        }
        if(collision.transform.CompareTag("Enemy")) {
            Prisoner prisoner = collision.gameObject.GetComponent<Prisoner>();
            if(prisoner.isGrounded) {
                bool hitFromRight = collision.transform.position.x > transform.position.x;
                MovePlatform(!hitFromRight, _prisonerPushPower);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            isPlayerOnPlatform = false;
            PlayerMovement.obj.platformRigidBody = null;
            PlayerPush.obj.platform = null;
        }
    }

    public bool IsFalling() {
        return fallTimer >= timeBeforeFall;
    }

    private void RegisterPlayerOnPlatform() {
        isPlayerOnPlatform = true;
        PlayerMovement.obj.platformRigidBody = _rigidBody;
        PlayerPush.obj.platform = this;
        if(isFallingPlatform)
            _startFallCountDown = true;
    }

    public bool somethingToTheRight = false;
    public bool somethingToTheLeft = false;
    private bool _startFlashing = true;
    private void Update()
    {
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
        somethingToTheRight = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.right, blockingCastDistance, _blockingCastLayerMask);
        somethingToTheLeft = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.left, blockingCastDistance, _blockingCastLayerMask);

        if (somethingToTheRight && _rigidBody.velocity.x > 0)
            movePlatform = false;
        if (somethingToTheLeft && _rigidBody.velocity.x < 0)
            movePlatform = false;

        if (movePlatform)
        {
            _rigidBody.velocity = new Vector2(Mathf.MoveTowards(_rigidBody.velocity.x, 0, deceleration * Time.deltaTime), _rigidBody.velocity.y);
        } else
        {
            _rigidBody.velocity = new Vector2(0, 0);
        }
        if(_rigidBody.velocity.x == 0)
        {
            movePlatform = false;
        }

        if(_respawning) {
            spawnTimer += Time.deltaTime;
            if(spawnTimer >= spawnTime) {
                _respawning = false;
                transform.position = _startingPosition;
                StartCoroutine(FadeInSprite());
                _collider.enabled = true; //If it was set to false from another platform
            }
        }
        // if (!isPlayerOnPlatform && _rigidBody.velocity.x == 0)
        //     MoveIdlePlatform();
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
        isPlayerOnPlatform = false;
        _fallingPlatformFlash.StopFlashing();
        _startFlashing = true;
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
