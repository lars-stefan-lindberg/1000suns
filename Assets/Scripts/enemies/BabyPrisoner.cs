using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
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
    public float alertSpeed = 4f;
    public float maxSpeed = 3;
    public float speedAcceleration = 1f;

    public float timeToTurnAround = 0.5f;
    public float turnAroundTimer = 1.3f;
    public bool isTurning = false;

    public bool isAlerted = false;

    public float playerCastDistance = 0;

    public float originHorizontalPos;
    public float maxTravellingDistance = 5f;

    [Header("Dependencies")]
    public LayerMask groundLayer;

    private float _enemyWidth;

    void Start() {
        _collider = GetComponent<BoxCollider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _enemyWidth = _collider.bounds.extents.x;
        originHorizontalPos = transform.position.x;
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

        //Save until potentially making a smart baby prisoner
        // if (isGrounded && !isTurning)
        // {
        //     //Check ahead if no ground ahead
        //     Vector2 groundLineAheadCastPosition = _collider.transform.position - _collider.transform.right * _enemyWidth * groundAheadCheck;
        //     isGroundFloorAhead = Physics2D.Linecast(groundLineAheadCastPosition, groundLineAheadCastPosition + Vector2.down, groundLayer);

        //     //Wall check
        //     bool isWallAhead = Physics2D.Raycast(_collider.transform.position, new Vector3(-_collider.transform.right.x, 0, 0), frontCheck, groundLayer);

        //     if (isWallAhead || !isGroundFloorAhead)
        //     {
        //         isTurning = true;
        //         turnAroundTimer = 0;
        //     }
            
        // }

        if(!isTurning && !isAlerted) {
            float currentHorizontalPos = transform.position.x;
            if(((currentHorizontalPos >= (originHorizontalPos + maxTravellingDistance)) && !IsMovingLeft()) || 
                ((currentHorizontalPos <= (originHorizontalPos - maxTravellingDistance)) && IsMovingLeft())) 
            {
                isTurning = true;
                turnAroundTimer = 0;
            }
        }

        if (isGrounded && !isAlerted)
        {
            GracefulSpeedChange();
        }
        // Debug.DrawRay(transform.position, (IsFacingLeft() ? Vector3.left : Vector3.right) * playerCastDistance, Color.red);
        // RaycastHit2D hit = Physics2D.Raycast(transform.position, (IsFacingLeft() ? Vector3.left : Vector3.right), playerCastDistance);

        // if(!isAlerted) {
        //     if(hit.transform != null) {
        //         if(hit.transform.CompareTag("Player")) {
        //             Alert();
        //         }
        //     }
        // }

        //Update animator
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool("isMoving", Mathf.Abs(_rigidBody.velocity.x) > 0.01);
        _animator.SetBool("isAlerted", isAlerted);
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
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("BabyPrisonerFinishLine")) {
            speed = 0;
        }
    }

    private bool IsMovingLeft() {
        return _rigidBody.velocity.x < 0;
    }

    private bool IsFacingLeft() {
        return transform.eulerAngles.y > -0.5 && transform.eulerAngles.y < 0.5;
    }

    public float alertRunDuration = 1.5f;
    public void Alert() {
        speed = 0;
        isAlerted = true;
        isTurning = false;
        StartCoroutine(AlertRunDelay(alertRunDuration));
    }

    private IEnumerator AlertRunDelay(float alertRunDelay) {
        yield return new WaitForSeconds(alertRunDelay);
        _animator.SetBool("isAlerted", isAlerted);
        if(IsFacingLeft()) 
            FlipHorizontal();
        speed = alertSpeed;
    }

    public void Despawn() {
        _animator.SetTrigger("despawn");
        StartCoroutine(DelayedSetInactive(1f));
    }

    private IEnumerator DelayedSetInactive(float delay) {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
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
