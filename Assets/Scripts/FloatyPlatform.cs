using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatyPlatform : MonoBehaviour
{
    public static FloatyPlatform obj;

    private BoxCollider2D _collider;
    private Rigidbody2D _rigidBody;

    public float landedCastDistance = 0.02f;
    public float platformDistance = 20f;
    public float pushPower = 10f;
    public float xCoordinateToMoveTo = 0;
    public float deceleration = 20f;
    private LayerMask _landedCastLayerMask;
    public bool movePlatform = false;

    private void Awake()
    {
        obj = this;
        _collider = GetComponentInChildren<BoxCollider2D>();
        _rigidBody = GetComponentInChildren<Rigidbody2D>();
        _landedCastLayerMask = LayerMask.GetMask("Default");
    }

    private void Update()
    {
        bool somethingLanded = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.up, landedCastDistance, _landedCastLayerMask);

        if(movePlatform)
        {
            transform.position = new Vector3(Mathf.MoveTowards(transform.position.x, xCoordinateToMoveTo, deceleration * Time.deltaTime), transform.position.y, transform.position.z);
            //transform.position = new Vector3(-15, transform.position.y, transform.position.z);
            //_rigidBody.velocity = new Vector2(Mathf.MoveTowards(_rigidBody.velocity.x, 0, deceleration * Time.deltaTime), _rigidBody.velocity.y);
        }
        if(transform.position.x == xCoordinateToMoveTo)
        {
            movePlatform = false;
        }
    }

    public void MovePlatform()
    {
        movePlatform = true;
        //_rigidBody.velocity = new Vector2(PlayerMovement.obj.isFacingLeft() ? pushPower : -pushPower, 0);
        xCoordinateToMoveTo = transform.position.x + (PlayerMovement.obj.isFacingLeft() ? platformDistance : -platformDistance);
    }

    private void OnDestroy()
    {
        obj = null;
    }

    //Created move function
    //When to initiate move function?
    //- PlayerPush needs to know if Player is on a SPECIFIC platform -> Call move function
    //- How to set that Player is on platform? Cast in player object?
}
