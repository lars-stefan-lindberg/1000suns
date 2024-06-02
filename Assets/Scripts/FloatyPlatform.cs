using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloatyPlatform : MonoBehaviour
{
    public BoxCollider2D _collider;
    private Rigidbody2D _rigidBody;

    public float idleMoveSpeed;
    private float _idleVerticalTargetPosition;
    public bool isPlayerOnPlatform = false;
    public float startingVerticalPosition;
    public float idleVerticalDistance = 0.25f;

    public float blockingCastDistance = 0.1f;
    public float pushPower = 10f;
    public float deceleration = 20f;
    private LayerMask _blockingCastLayerMask;
    public bool movePlatform = false;

    private float _idleTargetVerticalPosition = 0;

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
        if(collision.transform.CompareTag("Player"))
        {
            isPlayerOnPlatform = true;
            PlayerMovement.obj.isOnPlatform = true;
            PlayerMovement.obj.platformRigidBody = _rigidBody;
            PlayerPush.obj.platform = this;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            isPlayerOnPlatform = false;
            PlayerMovement.obj.isOnPlatform = false;
            PlayerMovement.obj.platformRigidBody = null;
            PlayerPush.obj.platform = null;
        }
    }

    public bool somethingToTheRight = false;
    public bool somethingToTheLeft = false;
    private void Update()
    {
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

    public void MovePlatform()
    {
        movePlatform = true;
        _rigidBody.velocity = new Vector2(PlayerMovement.obj.isFacingLeft() ? pushPower : -pushPower, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_collider.bounds.center, new Vector2(_collider.size.x * (1 + blockingCastDistance), _collider.size.y));
    }
}
