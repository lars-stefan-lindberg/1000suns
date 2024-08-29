using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloatyPlatform : MonoBehaviour
{
    public BoxCollider2D _collider;
    public Rigidbody2D _rigidBody;

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

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _blockingCastLayerMask = LayerMask.GetMask(new[] { "Ground", "Default", "JumpThroughs" });
        startingVerticalPosition = transform.position.y;
        _idleVerticalTargetPosition = startingVerticalPosition - idleVerticalDistance;
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
            RegisterPlayerOnPlatform();
        }
        if(collision.transform.CompareTag("Enemy")) {
            Prisoner prisoner = collision.gameObject.GetComponent<Prisoner>();
            MovePlatform(!prisoner.IsFacingRight(), _prisonerPushPower);
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

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            isPlayerOnPlatform = false;
            PlayerMovement.obj.platformRigidBody = null;
            PlayerPush.obj.platform = null;
        }
    }

    public bool somethingToTheRight = false;
    public bool somethingToTheLeft = false;
    private void Update()
    {
        if(_startFallCountDown)
            fallTimer += Time.deltaTime;
        if(fallTimer >= timeFallingBeforeDestroy)
            this.gameObject.SetActive(false);
        if(isFallingPlatform && fallTimer >= timeBeforeFall) {
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

        if (!isPlayerOnPlatform && _rigidBody.velocity.x == 0)
            MoveIdlePlatform();
    }

    private void MoveIdlePlatform()
    {
        if (transform.position.y >= startingVerticalPosition)
            _idleTargetVerticalPosition = _idleVerticalTargetPosition;
        if (transform.position.y <= _idleVerticalTargetPosition)
            _idleTargetVerticalPosition = startingVerticalPosition;
        transform.position = new Vector2(transform.position.x, Mathf.MoveTowards(transform.position.y, _idleTargetVerticalPosition, idleMoveSpeed * Time.deltaTime));
    }

    public void MovePlatform(bool isFacingLeft, float force)
    {
        movePlatform = true;
        float power = force * basePushPower;
        _rigidBody.velocity = new Vector2(isFacingLeft ? power : -power, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_collider.bounds.center, new Vector2(_collider.size.x * (1 + blockingCastDistance), _collider.size.y));
    }
}
