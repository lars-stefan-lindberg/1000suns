using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prisoner : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private BoxCollider2D collider;
    private Animator _animator;
    public LayerMask groundLayer;

    private static float _defaultSpeed = 3;
    private static float _defaultGravity = 1;

    //General properties
    public float gravityAcceleration = 0.5f;
    public float speedAcceleration = 1f;
    private float enemyWidth;
    public float speed = 0f;

    //Collision detection
    public bool isGrounded = false;
    public bool isGroundFloorAhead = true;
    public float groundAheadCheck = 0.51f;
    public float isGroundedCheckOffset = 0.55f; //TODO: Get dynamic value based on enemy height
    public float frontCheck = 0.51f;
    private RaycastHit2D _otherHit; //Enemy or boulder

    //When hit or recovering from hit
    public bool hasBeenHit = false;
    public float hasBeenHitDuration = 0.5f;
    public float hasBeenHitTimeCount = 0f;
    public Vector2 horizontalMoveSpeedDuringHit;
    public bool isRecovering = false;
    public float recoveryMovementStopMultiplier = 0.4f;

    public float damagePower; //When hit by projectile stores and uses the power fo the hit
    public float forceMultiplier = 40f;  //How "hard" a projectile will hit the enemy

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        collider = GetComponent<BoxCollider2D>();
        if (getRandomMovement() == 1) FlipHorizontal();
        enemyWidth = collider.bounds.extents.x;
    }

    private int getRandomMovement()
    {
        // Generate a random number that's either 0 or 1
        int randomNumber = Random.Range(0, 2);

        // Map 0 to -1 and 1 to 1
        return (randomNumber == 0) ? -1 : 1;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Projectile"))
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();

            bool hitFromTheLeft = projectile.rigidBody.position.x < _rigidBody.position.x;
            applyGotHitState(projectile.power, hitFromTheLeft);
        }
        else if (collision.transform.CompareTag("Enemy"))
        {
            Prisoner prisoner = collision.gameObject.GetComponent<Prisoner>();
            if (prisoner.hasBeenHit && !hasBeenHit)
            {
                bool hitFromTheLeft = prisoner._rigidBody.position.x < _rigidBody.position.x;
                applyGotHitState(prisoner.damagePower, hitFromTheLeft);
            }
        }
    }

    private void applyGotHitState(float hitPower, bool hitFromTheLeft)
    {
        damagePower = hitPower;
        hasBeenHit = true;
        hasBeenHitTimeCount = hasBeenHitDuration;
        _rigidBody.gravityScale = 0;
        _rigidBody.velocity = new Vector2(0, 0);

        if (hitFromTheLeft)
            _rigidBody.AddForce(new Vector2(damagePower * forceMultiplier, 0));
        else
            _rigidBody.AddForce(new Vector2(damagePower * -forceMultiplier, 0));

        horizontalMoveSpeedDuringHit = _rigidBody.velocity;
    }

    void Update()
    {
        //Check if grounded
        Vector3 groundLineCastPosition = collider.transform.position;
        //Debug.DrawLine(
        //    groundLineCastPosition,
        //    new Vector3(groundLineCastPosition.x, groundLineCastPosition.y - isGroundedCheckOffset, groundLineCastPosition.z),
        //    Color.red);
        isGrounded = Physics2D.Linecast(
            groundLineCastPosition,
            new Vector3(groundLineCastPosition.x, groundLineCastPosition.y - isGroundedCheckOffset, groundLineCastPosition.z),
            groundLayer);

        //Only perform other collision checks if grounded
        if (isGrounded)
        {
            //Check ahead if no ground ahead
            Vector2 groundLineAheadCastPosition = collider.transform.position - collider.transform.right * enemyWidth * groundAheadCheck;
            isGroundFloorAhead = Physics2D.Linecast(groundLineAheadCastPosition, groundLineAheadCastPosition + Vector2.down, groundLayer);
            Debug.DrawLine(
                groundLineAheadCastPosition,
                new Vector3(groundLineAheadCastPosition.x, groundLineAheadCastPosition.y + Vector2.down.y, collider.transform.position.z),
            Color.magenta);
            if (!isGroundFloorAhead)
            {
                FlipHorizontal();
            }

            //Wall check
            if (Physics2D.Raycast(collider.transform.position, new Vector3(-collider.transform.right.x, 0, 0), frontCheck, groundLayer))
                FlipHorizontal();

            //Collision with another enemy, but only if not already hit
            //_otherHit = Physics2D.Raycast(collider.transform.position, new Vector3(-collider.transform.right.x, 0, 0), frontCheck);
            //if (_otherHit.transform != null)
            //    if (_otherHit.transform.CompareTag("Enemy") && !hasBeenHit)
            //    {
            //        FlipHorizontal();
            //    }
        }

        if (hasBeenHit)
        {
            hasBeenHitTimeCount -= Time.deltaTime;
            GracefulGravityChange();

            if (hasBeenHitTimeCount < 0)
            {
                hasBeenHit = false;
                isRecovering = true;
                _rigidBody.gravityScale = _defaultGravity;
            }
        }
        if (isRecovering)
        {
            float currentVelocity =
                _rigidBody.velocity.x == horizontalMoveSpeedDuringHit.x ?
                horizontalMoveSpeedDuringHit.x : _rigidBody.velocity.x;
            GracefulMovementStop(currentVelocity);
            if (_rigidBody.velocity.x == 0) isRecovering = false;
        }

        if (!hasBeenHit && !isRecovering && isGrounded)
        {
            GracefulSpeedChange();
        }

        //Update animator
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool("isMoving", _rigidBody.velocity.x != 0);
    }

    void FixedUpdate()
    {
        if (!hasBeenHit && !isRecovering && isGrounded)
        {
            Vector2 currentVelocity = _rigidBody.velocity;
            currentVelocity.x = -collider.transform.right.x * speed;
            _rigidBody.velocity = currentVelocity;
        }

        if (collider.transform.position.y < GameMgr.DEAD_ZONE)
        {
            Debug.Log("Enemy died.");
            Destroy(gameObject);
        }
    }

    private void GracefulMovementStop(float currentVelocity)
    {
        _rigidBody.velocity = new Vector2(Mathf.MoveTowards(currentVelocity, 0, recoveryMovementStopMultiplier), _rigidBody.velocity.y);
    }
    private void GracefulGravityChange()
    {
        _rigidBody.gravityScale = Mathf.MoveTowards(_rigidBody.gravityScale, _defaultGravity, gravityAcceleration * Time.fixedDeltaTime);
    }
    private void GracefulSpeedChange()
    {
        speed = Mathf.MoveTowards(speed, _defaultSpeed, speedAcceleration * Time.fixedDeltaTime);
    }

    private void FlipHorizontal()
    {
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y += 180;
        transform.eulerAngles = currentRotation;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.cyan;
    //    //Gizmos.DrawLine(collider.transform.position, collider.transform.position + Vector3.down * floorCheckY);
    //    Gizmos.DrawLine(collider.transform.position, collider.transform.position + new Vector3(-collider.transform.right.x, 0, 0) * frontCheck);
    //}
}
