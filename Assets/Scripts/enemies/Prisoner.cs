using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prisoner : MonoBehaviour
{
    public GameObject prisonerSoul;
    public Transform prisonerSoulTarget;
    private Rigidbody2D _rigidBody;
    private BoxCollider2D _collider;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    public LayerMask groundLayer;

    public float defaultSpeed = 3;
    private static float _defaultGravity = 1;

    //General properties
    public int movementDirection = 0; //-1->left, 1->right
    public float gravityAcceleration = 0.5f;
    public float speedAcceleration = 1f;
    private float enemyWidth;
    public float speed = 0f;

    //Collision detection
    public bool isGrounded = true;
    public bool isGroundFloorAhead = true;
    public float groundAheadCheck = 0.51f;
    public float isGroundedCheckOffset = 0.55f; //TODO: Get dynamic value based on enemy height
    public float frontCheck = 0.51f;
    private RaycastHit2D _otherHit; //Enemy or boulder

    //When hit or recovering from hit
    public bool hasBeenHit = false;
    public float hasBeenHitDuration = 0.5f;
    public float hasBeenHitTimeCount = 0f;
    public float recoveryDuration = 0.5f;
    public float recoveryTimeCount = 0f;
    public Vector2 horizontalMoveSpeedDuringHit;
    public bool isRecovering = false;
    public float recoveryMovementStopMultiplier = 0.4f;

    public float damagePower; //When hit by projectile stores and uses the power fo the hit
    public float forceMultiplier = 7f;  //How "hard" a projectile will hit the enemy

    public float timeToTurnAround = 0.5f;
    public float turnAroundTimer = 1.3f;
    public bool isTurning = false;

    public bool isStatic = false;
    public bool isSpawningSoul = false;
    public bool isImmuneToForcePush = false;
    public bool isSpawningFast = false;
    public float spawnAnimationSpeed = 3;
    private bool _isSpawning = true;
    private bool _isFalling = false;

    public float playerCastDistance = 0;
    public bool isAttacking = false;
    public float attackSpeedMultiplier = 1.5f;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if(isImmuneToForcePush)
            _spriteRenderer.color = new Color(0.6f, 0, 0, 1);
        if(isSpawningFast)
            _animator.speed = spawnAnimationSpeed;
        _collider = GetComponent<BoxCollider2D>();
        if(movementDirection == 0) {
            if (getRandomMovement() == 1) 
                FlipHorizontal();
        }
        if(movementDirection == 1)
            FlipHorizontal();
        enemyWidth = _collider.bounds.extents.x;
    }

    private int getRandomMovement()
    {
        // Generate a random number that's either 0 or 1
        int randomNumber = Random.Range(0, 2);

        // Map 0 to -1 and 1 to 1
        return (randomNumber == 0) ? -1 : 1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Projectile"))
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();

            bool hitFromTheLeft = projectile.rigidBody.position.x < _rigidBody.position.x;
            applyGotHitState(projectile.power, hitFromTheLeft);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Enemy"))
        {
            Prisoner prisoner = collision.gameObject.GetComponent<Prisoner>();
            if (prisoner.hasBeenHit && !hasBeenHit)
            {
                bool hitFromTheLeft = prisoner._rigidBody.position.x < _rigidBody.position.x;
                applyGotHitState(prisoner.damagePower, hitFromTheLeft);
            }
        }
        if (collision.transform.CompareTag("Player")) {
            Reaper.obj.KillPlayerShadow();
            isStatic = true;
            _rigidBody.velocity = new Vector2(0,0);
        }

    }

    private void applyGotHitState(float hitPower, bool hitFromTheLeft)
    {
        if(!isImmuneToForcePush) {
            _animator.SetTrigger("hit");
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
    }

    void Update()
    {
        if(_killed) {
            _animator.SetBool("isKilled", _killed);
            return;
        }
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("prisoner_spawn"))
            return;
        else
            _animator.speed = 1;

        //Check if grounded
        Vector3 groundLineCastPosition = _collider.transform.position;
        //Debug.DrawLine(
        //    groundLineCastPosition,
        //    new Vector3(groundLineCastPosition.x, groundLineCastPosition.y - isGroundedCheckOffset, groundLineCastPosition.z),
        //    Color.red);
        bool groundHit = Physics2D.Linecast(
            groundLineCastPosition,
            new Vector3(groundLineCastPosition.x, groundLineCastPosition.y - isGroundedCheckOffset, groundLineCastPosition.z),
            groundLayer);

        // if(groundHit && !isGrounded) {
        //     Debug.Log("landed");
        //     isRecovering = true;
        //     recoveryTimeCount = recoveryDuration;
        // }
        if(!isGrounded && groundHit) {
            _isFalling = false;
        } else if(isGrounded && !groundHit && !hasBeenHit && !isRecovering && !_isSpawning) {
            _isFalling = true;
            _animator.SetTrigger("fall");
        }

        isGrounded = groundHit;

        if (isGrounded && !isTurning && !hasBeenHit)
        {
            //Check ahead if no ground ahead
            Vector2 groundLineAheadCastPosition = _collider.transform.position - _collider.transform.right * enemyWidth * groundAheadCheck;
            isGroundFloorAhead = Physics2D.Linecast(groundLineAheadCastPosition, groundLineAheadCastPosition + Vector2.down, groundLayer);

            //Wall check
            bool isWallAhead = Physics2D.Raycast(_collider.transform.position, new Vector3(-_collider.transform.right.x, 0, 0), frontCheck, groundLayer);

            //Collision with another enemy, but only if not already hit
            _otherHit = Physics2D.Raycast(_collider.transform.position, new Vector3(-_collider.transform.right.x, 0, 0), frontCheck);
            bool isEnemyAhead = false;
            if (_otherHit.transform != null)
               if (_otherHit.transform.CompareTag("Enemy") && !hasBeenHit)
               {
                   isEnemyAhead = true;
               }

            if (isWallAhead || !isGroundFloorAhead || isEnemyAhead)
            {
                isTurning = true;
                turnAroundTimer = 0;
            }
            
        }

        if (hasBeenHit)
        {
            isTurning = false;
            turnAroundTimer = timeToTurnAround;
            hasBeenHitTimeCount -= Time.deltaTime;
            GracefulGravityChange();

            if (hasBeenHitTimeCount < 0)
            {
                hasBeenHit = false;
                isRecovering = true;
                recoveryTimeCount = recoveryDuration;
                _rigidBody.gravityScale = _defaultGravity;
            }
        }
        if (isRecovering)
        {
            float currentVelocity =
                _rigidBody.velocity.x == horizontalMoveSpeedDuringHit.x ?
                horizontalMoveSpeedDuringHit.x : _rigidBody.velocity.x;
            GracefulMovementStop(currentVelocity);
            if (_rigidBody.velocity.x == 0)
            {
                recoveryTimeCount -= Time.deltaTime;
                if (recoveryTimeCount < 0)
                    isRecovering = false;
            }
        }

        if (!hasBeenHit && !isRecovering && isGrounded && !isStatic && !isTurning)
        {
            GracefulSpeedChange();
        }

        if(!isStatic && !hasBeenHit && !isRecovering && isGrounded) {
            Debug.DrawRay(transform.position, (IsFacingRight() ? Vector3.right : Vector3.left) * playerCastDistance, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, IsFacingRight() ? Vector3.right : Vector3.left, playerCastDistance);

            if(hit.transform != null) {
                if(hit.transform.CompareTag("Player")) {
                    //Attack();
                    isAttacking = true;
                }
                else
                    isAttacking = false;
            }
        }

        //Check if landed on edge. Try to recover by moving to one side -> either fall, or reach stable ground
        if(!isGrounded && _rigidBody.velocity == Vector2.zero) {
            Debug.Log("nudging");
            _rigidBody.velocity = new Vector2(7, 0);
        }

        //Update animator
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool("isHit", hasBeenHit);
        _animator.SetBool("isRecovering", isRecovering);
        _animator.SetBool("isMoving", Mathf.Abs(_rigidBody.velocity.x) > 0.01);
        _animator.SetBool("isSpawning", _isSpawning);
        _animator.SetBool("isFalling", _isFalling);
        //_animator.SetBool("isMoving", isMoving);
    }

    // private void Attack()
    // {
    //     _rigidBody.AddForce(new Vector2(IsFacingRight() ? attackSpeed : -attackSpeed, 0));
    // }

    void FixedUpdate()
    {
        if(_killed) {
            _animator.SetBool("isKilled", _killed);
            return;
        }
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("prisoner_spawn"))
            return;
        else
            _animator.speed = 1;

        if (turnAroundTimer <= timeToTurnAround && isTurning)
        {
            _rigidBody.velocity = new Vector2(0, 0);
            turnAroundTimer += Time.deltaTime;
        } else if(turnAroundTimer >= timeToTurnAround && isTurning)
        {
            isTurning = false;
            FlipHorizontal();
        }
        
        if (!hasBeenHit && !isRecovering && isGrounded)
        {
            if (!isTurning && !isStatic)
            {
                Vector2 currentVelocity = _rigidBody.velocity;
                currentVelocity.x = -_collider.transform.right.x * speed * (isAttacking ? attackSpeedMultiplier : 1);
                _rigidBody.velocity = currentVelocity;
            }
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

    private bool _killed = false;

    [ContextMenu("InitiateKill")]
    public void InitiateKill() {
        _killed = true;
        _rigidBody.bodyType = RigidbodyType2D.Static;
        _collider.enabled = false;
        _animator.SetTrigger("death");
    }

    public bool isSpawningPrisoners = false;
    public GameObject[] prisonersToSpawn;
    public void Kill() {
        if(isSpawningPrisoners)
            SpawnPrisoners();
        if(isSpawningSoul) {
            GameObject prisonerSoul = Instantiate(this.prisonerSoul, transform.position, transform.rotation);
            prisonerSoul.GetComponent<PrisonerSoul>().Target = prisonerSoulTarget;
            gameObject.SetActive(false);
        } else {
            Destroy(gameObject);
        }
    }

    private void SpawnPrisoners() {
        foreach(GameObject prisoner in prisonersToSpawn) {
            prisoner.SetActive(true);
        }
    }

    public void SpawningComplete() {
        _isSpawning = false;
    }
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.cyan;
    //    //Gizmos.DrawLine(collider.transform.position, collider.transform.position + Vector3.down * floorCheckY);
    //    Gizmos.DrawLine(collider.transform.position, collider.transform.position + new Vector3(-collider.transform.right.x, 0, 0) * frontCheck);
    //}
}
