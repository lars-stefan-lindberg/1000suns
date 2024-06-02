using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloatyPlatform : MonoBehaviour
{
    public BoxCollider2D _collider;
    private Rigidbody2D _rigidBody;

    public float blockingCastDistance = 0.1f;
    public float pushPower = 10f;
    public float deceleration = 20f;
    private LayerMask _blockingCastLayerMask;
    public bool movePlatform = false;

    public bool blockedToTheRight = false;
    public bool blockedToTheLeft = false;

    private static readonly string[] colliderTags = { "Player", "Block", "Enemy", "Ground" };

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _blockingCastLayerMask = LayerMask.GetMask(new[] { "Ground", "Default", "JumpThroughs" });
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.CompareTag("Player"))
        {
            PlayerMovement.obj.isOnPlatform = true;
            PlayerMovement.obj.platformRigidBody = _rigidBody;
            PlayerPush.obj.platform = this;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
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

        if(movePlatform)
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
