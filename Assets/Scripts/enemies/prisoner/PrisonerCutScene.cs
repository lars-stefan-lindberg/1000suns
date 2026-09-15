using System.Collections;
using DG.Tweening;
using FunkyCode;
using UnityEngine;

public class PrisonerCutScene : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private BoxCollider2D _collider;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private LightSprite2DFadeManager _lightSprite2DFadeManager;
    public LayerMask groundLayer;

    public float defaultSpeed = 3;

    public int movementDirection = 0; //-1->left, 1->right
    public float speedAcceleration = 1f;
    public float speed = 0f;

    public bool isGrounded = true;
    public float isGroundedCheckOffset = 0.55f;
    public bool isStatic = true;
    public bool IsSpawning = true;
    private PrisonerCutsceneAnimationEvents _animationEvents;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _lightSprite2DFadeManager = GetComponent<LightSprite2DFadeManager>();
        _collider = GetComponent<BoxCollider2D>();
        _animationEvents = GetComponentInChildren<PrisonerCutsceneAnimationEvents>();
        
        if(movementDirection == 1)
            FlipHorizontal();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player")) {
            SetStatic();
            
            float prisonerX = transform.position.x;
            float playerX = ShadowTwinPlayer.obj.rigidBody.position.x;
            float hitDirection = (prisonerX < playerX) ? -1f : 1f;
            
            ShadowTwinMovement.obj.GetHit(hitDirection);
        }
    }

    private void SetStatic() {
        isStatic = true;
        _rigidBody.velocity = new Vector2(0,0);
    }

    public void StartMoving() {
        _animationEvents.PlaySlide();
        isStatic = false;
    }

    public void Despawn() {
        _rigidBody.bodyType = RigidbodyType2D.Static;
        _collider.enabled = false;
        _animator.SetTrigger("despawn");
        _lightSprite2DFadeManager.StartFadeOut();
        StartCoroutine(DelayedDespawn());
    }

    private IEnumerator DelayedDespawn() {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    void Update()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("prisoner_spawn"))
            return;
        else
            _animator.speed = 1;
        
        //Check if grounded using boxcast
        Vector2 boxSize = new Vector2(_collider.bounds.size.x, 0.1f);
        Vector2 boxCastOrigin = _collider.bounds.center;
        RaycastHit2D groundHit = Physics2D.BoxCast(
            boxCastOrigin,
            boxSize,
            0f,
            Vector2.down,
            isGroundedCheckOffset,
            groundLayer);

        isGrounded = groundHit.collider != null;

        if (isGrounded && !isStatic)
        {
            GracefulSpeedChange();
        }

        //Update animator
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool("isMoving", Mathf.Abs(_rigidBody.velocity.x) > 0.01);
        _animator.SetBool("isSpawning", IsSpawning);
    }

    void FixedUpdate()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("prisoner_spawn"))
            return;
        else
            _animator.speed = 1;

        if (isGrounded)
        {
            if(!isStatic) {
                Vector2 currentVelocity = _rigidBody.velocity;
                float speedMultiplier = 1f;
                currentVelocity.x = -_collider.transform.right.x * speed * speedMultiplier;
                _rigidBody.velocity = currentVelocity;
            }
        }
    }

    private void GracefulSpeedChange()
    {        
        speed = Mathf.MoveTowards(speed, defaultSpeed, speedAcceleration * Time.fixedDeltaTime);
    }

    private void FlipHorizontal()
    {
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y += 180;
        transform.eulerAngles = currentRotation;
    }

    public bool IsFacingRight() {
        return transform.eulerAngles.y > 179.5f && transform.eulerAngles.y < 180.5f;
        //return _rigidBody.velocity.x < 0;
    }
}
