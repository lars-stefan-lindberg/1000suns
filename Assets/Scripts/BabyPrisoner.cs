using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BabyPrisoner : MonoBehaviour
{
    private BoxCollider2D _collider;
    private Rigidbody2D _rigidBody;
    private Animator _animator;
    public bool isGrounded = false;
    public float isGroundedCheckOffset = 0.55f; //TODO: Get dynamic value based on enemy height
    public float groundAheadCheck = 0.51f;
    public bool isGroundFloorAhead = true;
    public float frontCheck = 0.51f;

    public float speed = 0f;
    public float maxSpeed = 3;
    public float speedAcceleration = 1f;

    public float timeToTurnAround = 0.5f;
    public float turnAroundTimer = 1.3f;
    public bool isTurning = false;

    [Header("Dependencies")]
    public LayerMask groundLayer;

    private float _enemyWidth;

    void Start() {
        _collider = GetComponent<BoxCollider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _enemyWidth = _collider.bounds.extents.x;
    }

    void Update()
    {
        //Check if grounded
        Vector3 groundLineCastPosition = _collider.transform.position;
        //Debug.DrawLine(
        //    groundLineCastPosition,
        //    new Vector3(groundLineCastPosition.x, groundLineCastPosition.y - isGroundedCheckOffset, groundLineCastPosition.z),
        //    Color.red);
        isGrounded = Physics2D.Linecast(
            groundLineCastPosition,
            new Vector3(groundLineCastPosition.x, groundLineCastPosition.y - isGroundedCheckOffset, groundLineCastPosition.z),
            groundLayer);

        if (isGrounded && !isTurning)
        {
            //Check ahead if no ground ahead
            Vector2 groundLineAheadCastPosition = _collider.transform.position - _collider.transform.right * _enemyWidth * groundAheadCheck;
            isGroundFloorAhead = Physics2D.Linecast(groundLineAheadCastPosition, groundLineAheadCastPosition + Vector2.down, groundLayer);

            //Wall check
            bool isWallAhead = Physics2D.Raycast(_collider.transform.position, new Vector3(-_collider.transform.right.x, 0, 0), frontCheck, groundLayer);

            if (isWallAhead || !isGroundFloorAhead)
            {
                isTurning = true;
                turnAroundTimer = 0;
            }
            
        }

        if (isGrounded)
        {
            GracefulSpeedChange();
        }

        //Update animator
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool("isMoving", Mathf.Abs(_rigidBody.velocity.x) > 0.01);
        //_animator.SetBool("isMoving", isMoving);
    }


    void FixedUpdate()
    {
        if (turnAroundTimer <= timeToTurnAround && isTurning)
        {
            _rigidBody.velocity = new Vector2(0, 0);
            turnAroundTimer += Time.deltaTime;
        } else if(turnAroundTimer >= timeToTurnAround && isTurning)
        {
            isTurning = false;
            FlipHorizontal();
        }
        
        if (isGrounded)
        {
            if (!isTurning)
            {
                Vector2 currentVelocity = _rigidBody.velocity;
                currentVelocity.x = -_collider.transform.right.x * speed;
                _rigidBody.velocity = currentVelocity;
            }
        }
        

        // if (_collider.transform.position.y < GameMgr.DEAD_ZONE)
        // {
        //     Debug.Log("Enemy died.");
        //     Destroy(gameObject);
        // }
    }

    private void FlipHorizontal()
    {
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y += 180;
        transform.eulerAngles = currentRotation;
    }

    private void GracefulSpeedChange()
    {        
        speed = Mathf.MoveTowards(speed, maxSpeed, speedAcceleration * Time.fixedDeltaTime);
    }
}
