using System.Collections;
using FunkyCode;
using UnityEngine;

public class Prisoner : MonoBehaviour
{
    public GameObject prisonerSoul;
    public Transform prisonerSoulTarget;
    private Rigidbody2D _rigidBody;
    private BoxCollider2D _collider;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private LightSprite2DFadeManager _lightSprite2DFadeManager;
    private LightSprite2D _lightSprite2D;
    private Pullable _pullable;
    public LayerMask groundLayer; //Used to check if grounded
    public LayerMask collisionLayer; //Raycast for collisions like other prisoners, walls, blocks
    public LayerMask playerLayer;

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
    public float groundAheadCheck = 1.35f;
    public float groundBehindCheck = 1.96f;
    public float isGroundedCheckOffset = 0.55f; //TODO: Get dynamic value based on enemy height
    public float frontCheck = 0.51f;
    public float frontCheckHitBlock = 1f;
    public float behindCheck = 1.4f;
    private RaycastHit2D _otherHit; //Enemy or boulder

    //When hit or recovering from hit
    public bool hasBeenHit = false;
    public float hasBeenHitDuration = 0.5f;
    public float hasBeenHitTimeCount = 0f;
    public float recoveryDuration = 0.5f;
    public float recoveryTimeCount = 0f;
    public bool isRecovering = false;
    public float recoveryMovementStopMultiplier = 0.4f;
    private bool _isBeingPulled = false;

    public float damagePower; //When hit by projectile stores and uses the power of the hit
    public float forceMultiplier = 7f;  //How "hard" a projectile will hit the enemy
    public float forceMultiplierBack = 6f;  //How "hard" a projectile will hit the enemy if hit in the back

    public float timeToTurnAround = 0.5f;
    public float turnAroundTimer = 1.3f;
    public bool isTurning = false;

    public bool isStatic = false;
    public bool isSpawningSoul = false;
    public bool isImmuneToForcePush = false;
    public bool isSpawningFast = false;
    public float spawnAnimationSpeed = 3;
    public bool IsSpawning = true;
    private bool _isFalling = false;

    public float playerCastDistance = 0;
    public bool isAttacking = false;
    public float attackSpeedMultiplier = 1.5f;
    public bool isStuck = false;
    public float bounceOffOtherPrisonerMultiplier = 0.5f;

    public bool isRampaging = false;
    public float rampageDuration = 2f;
    private float rampageTimeCount = 0f;

    private AudioSource _gotHitAudioSource;
    private bool _isFadingOutHitSound = false;
    public bool muteDeathSoundFX = false;
    private Rigidbody2D _blockInContact;
    private Material _material;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _material = _spriteRenderer.material;
        _lightSprite2D = GetComponent<LightSprite2D>();
        _lightSprite2DFadeManager = GetComponent<LightSprite2DFadeManager>();
        _pullable = GetComponent<Pullable>();
        if(isImmuneToForcePush) {
            _lightSprite2DFadeManager.SetFadeInAlpha(0.7f);
            _lightSprite2D.color = Color.HSVToRGB(0f, 0.9f, 1f);
            _lightSprite2D.color.a = 0.7f;
            _material.SetFloat("_ColorReplaceFade", 1);
        }
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
            ApplyGotHitState(projectile.power, projectile.horizontalDirection);
        }
        if (collision.transform.CompareTag("Block"))
        {
            _blockInContact = collision.GetComponent<Rigidbody2D>();
        }
    }

     private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Block"))
        {
            _blockInContact = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Enemy"))
        {
            Prisoner prisoner = collision.gameObject.GetComponent<Prisoner>();
            if (prisoner.hasBeenHit && !hasBeenHit)
            {
                int hitDirection = prisoner._rigidBody.position.x < _rigidBody.position.x ? 1 : -1;
                ApplyGotHitState(prisoner.damagePower * bounceOffOtherPrisonerMultiplier, hitDirection);
            }
        }
        if (collision.transform.CompareTag("Player")) {
            StartCoroutine(SetStaticForABriefMoment());
            Reaper.obj.KillPlayerShadow(PlayerManager.obj.GetPlayerTypeFromCollision(collision));
        }
    }

    private IEnumerator SetStaticForABriefMoment() {
        isStatic = true;
        _rigidBody.velocity = new Vector2(0,0);
        yield return new WaitForSeconds(1f);
        isStatic = false;
    }

    private void ApplyGotHitState(float hitPower, int projectileDirection)
    {
        if(!isImmuneToForcePush) {
            // _gotHitAudioSource = SoundFXManager.obj.PlayPrisonerHit(transform);
            _isFadingOutHitSound = false;
            
            _animator.SetTrigger("hit");
            damagePower = hitPower;
            hasBeenHit = true;
            isStuck = false;
            hasBeenHitTimeCount = hasBeenHitDuration;   
            _rigidBody.gravityScale = 0;
            _rigidBody.velocity = new Vector2(0, 0);

            bool isFacingRight = IsFacingRight();

            bool isHitFront = (projectileDirection == 1 && !isFacingRight) || (projectileDirection == -1 && isFacingRight);

            if(isRecovering || isHitFront)
                _rigidBody.AddForce(new Vector2(damagePower * forceMultiplier * projectileDirection, 0));
            else
                _rigidBody.AddForce(new Vector2(damagePower * forceMultiplierBack * projectileDirection, 0));
            isRecovering = false;

            //Make sure prisoner is moving in the same direction as it got hit, to create sense of "awareness"
            if(projectileDirection == 1 && isFacingRight)
                FlipHorizontal();
            else if(projectileDirection == -1 && !isFacingRight)
                FlipHorizontal();
        } else {
            //TODO, react to being hit
        }
    }


    private float _isStuckCooldownTimer = 0;
    private readonly float _isStuckCooldownTime = 1.5f;
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

        //If stuck, give a second or two to recover
        if(isStuck && _isStuckCooldownTimer < _isStuckCooldownTime) {
            _isStuckCooldownTimer += Time.deltaTime;
            return;
        } else {
            _isStuckCooldownTimer = 0;
        }

        if(_pullable.IsPulled && !_isBeingPulled) {
            _isBeingPulled = true;
            _rigidBody.gravityScale = 0;
            _rigidBody.velocity = new Vector2(0, 0);
            _animator.SetTrigger("hit");
        } else if(!_pullable.IsPulled && _isBeingPulled) {
            _isBeingPulled = false;
            isRecovering = true;
            recoveryTimeCount = recoveryDuration;
            _rigidBody.gravityScale = _defaultGravity;
            //Turn towards player
            PlayerManager.PlayerType activePlayerType = PlayerManager.obj.GetActivePlayerType();
            Transform playerTransform = PlayerManager.obj.GetPlayerTransform(activePlayerType);
            bool isFacingRight = IsFacingRight();
            bool isPlayerToTheRight = playerTransform.position.x > transform.position.x;
            if(isFacingRight != isPlayerToTheRight) {
                FlipHorizontal();
            }
        }

        //To check if we should stop a block when cornered to a wall
        if(_blockInContact != null) {
            float blockVelocity = _blockInContact.velocity.x;
            if(blockVelocity > 0) {
                bool isWallToTheRight = Physics2D.Raycast(_collider.transform.position, Vector2.right, frontCheckHitBlock, collisionLayer);
                if(isWallToTheRight) {
                    _blockInContact.velocity = new Vector2(0,0);
                }
            } else if(blockVelocity < 0) {
                //Debug.DrawLine(_collider.transform.position, new Vector2(_collider.transform.position.x - frontCheckHitBlock, _collider.transform.position.y), Color.yellow);
                bool isWallToTheLeft = Physics2D.Raycast(_collider.transform.position, Vector2.left, frontCheckHitBlock, collisionLayer);
                if(isWallToTheLeft) {
                    _blockInContact.velocity = new Vector2(0,0);
                }
            }
        }
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

        if(!isGrounded && groundHit) {
            _isFalling = false;
            isRecovering = true;
            recoveryTimeCount = recoveryDuration;
        } else if(isGrounded && !groundHit && !hasBeenHit && !_isBeingPulled && !isRecovering && !IsSpawning) {
            _isFalling = true;
            _animator.SetTrigger("fall");
        }

        isGrounded = groundHit.collider != null;

        //Check if space to move is too small. If so go into idle state
        if(isGrounded) {
            bool isWallAhead = Physics2D.Raycast(_collider.transform.position, new Vector3(-_collider.transform.right.x, 0, 0), frontCheck, collisionLayer);
            bool isWallBehind = Physics2D.Raycast(_collider.transform.position, new Vector3(_collider.transform.right.x, 0, 0), behindCheck, collisionLayer);
            
            Vector2 groundLineAheadCastPosition = _collider.transform.position - _collider.transform.right * enemyWidth * groundAheadCheck;
            isGroundFloorAhead = Physics2D.Linecast(groundLineAheadCastPosition, groundLineAheadCastPosition + Vector2.down, groundLayer);
            
            Vector2 groundLineBehindCastPosition = _collider.transform.position + _collider.transform.right * enemyWidth * groundBehindCheck;
            bool isGroundFloorBehind = Physics2D.Linecast(groundLineBehindCastPosition, groundLineBehindCastPosition + Vector2.down, groundLayer);

            //Check for prisoner in front and behind
            
            if((isWallAhead && isWallBehind) || (!isGroundFloorAhead && !isGroundFloorBehind) && !hasBeenHit && !_isBeingPulled) {
                isStuck = true;
            } else {
                isStuck = false;
            }
        } else
            isStuck = false;

        if (isGrounded && !isTurning && !hasBeenHit && !isStuck && !_isBeingPulled)
        {
            //Check ahead if no ground ahead
            Vector2 groundLineAheadCastPosition = _collider.transform.position - _collider.transform.right * enemyWidth * groundAheadCheck;
            isGroundFloorAhead = Physics2D.Linecast(groundLineAheadCastPosition, groundLineAheadCastPosition + Vector2.down, groundLayer);

            //Wall check
            bool isWallAhead = Physics2D.Raycast(_collider.transform.position, new Vector3(-_collider.transform.right.x, 0, 0), frontCheck, collisionLayer);

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

        if(_isBeingPulled) {
            isTurning = false;
            isStuck = false;
            turnAroundTimer = timeToTurnAround;
        }
        if (hasBeenHit)
        {
            isTurning = false;
            isStuck = false;
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
            GracefulMovementStop(_rigidBody.velocity.x);
            if (_rigidBody.velocity.x == 0)
            {
                recoveryTimeCount -= Time.deltaTime;
                if (recoveryTimeCount < 0)
                {
                    isRecovering = false;
                    isRampaging = true;
                    rampageTimeCount = rampageDuration;
                }
            }
        }

        if (isRampaging)
        {
            rampageTimeCount -= Time.deltaTime;
            if (rampageTimeCount < 0)
                isRampaging = false;
        }

        if (!_isBeingPulled && !hasBeenHit && !isRecovering && isGrounded && !isStatic && !isTurning && !isStuck)
        {
            GracefulSpeedChange();
        }

        if(!isStatic && !_isBeingPulled && !hasBeenHit && !isRecovering && isGrounded && !isStuck) {
            //Debug.DrawRay(transform.position, (IsFacingRight() ? Vector3.right : Vector3.left) * playerCastDistance, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, IsFacingRight() ? Vector3.right : Vector3.left, playerCastDistance, playerLayer);

            if(hit.transform != null) {
                if(hit.transform.CompareTag("Player")) {
                    //Attack();
                    isAttacking = true;
                }
                else
                    isAttacking = false;
            } else
                isAttacking = false;
        }

        if(isStuck)
            _rigidBody.velocity = Vector2.zero;

        // if(!_isFadingOutHitSound && _gotHitAudioSource != null && !hasBeenHit && isGrounded) {
        //     _isFadingOutHitSound = true;
        //     SoundFXManager.obj.FadeOutAndStopSound(_gotHitAudioSource, 0.2f);
        // }

        //Update animator
        _animator.SetBool("isGrounded", isGrounded);
        _animator.SetBool("isHit", hasBeenHit || _isBeingPulled);
        _animator.SetBool("isRecovering", isRecovering);
        _animator.SetBool("isMoving", Mathf.Abs(_rigidBody.velocity.x) > 0.01);
        _animator.SetBool("isSpawning", IsSpawning);
        _animator.SetBool("isFalling", _isFalling);
        _animator.SetBool("isStuck", isStuck);
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
        
        if (!_isBeingPulled && !hasBeenHit && !isRecovering && isGrounded && !isStuck)
        {
            if (!isTurning && !isStatic)
            {
                Vector2 currentVelocity = _rigidBody.velocity;
                float speedMultiplier = 1f;
                if (isAttacking)
                    speedMultiplier = attackSpeedMultiplier;
                else if (isRampaging)
                    speedMultiplier = attackSpeedMultiplier;
                currentVelocity.x = -_collider.transform.right.x * speed * speedMultiplier;
                _rigidBody.velocity = currentVelocity;
            }
        }
    }

    private void GracefulMovementStop(float currentVelocity)
    {
        _rigidBody.velocity = new Vector2(Mathf.MoveTowards(currentVelocity, 0, recoveryMovementStopMultiplier * Time.deltaTime), _rigidBody.velocity.y);
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
        // if(_gotHitAudioSource != null)
        //     SoundFXManager.obj.FadeOutAndStopSound(_gotHitAudioSource, 0.2f);
        _killed = true;
        _rigidBody.bodyType = RigidbodyType2D.Static;
        _collider.enabled = false;
        _animator.SetTrigger("death");
        _lightSprite2DFadeManager.StartFadeOut();
    }

    public bool isSpawningPrisoners = false;
    public GameObject[] prisonersToSpawn;
    //Kill is run by the end of the death animation via event trigger
    public void Kill() {
        if(isSpawningPrisoners)
            SpawnPrisoners();
        if(isSpawningSoul) {
            GameObject prisonerSoul = Instantiate(this.prisonerSoul, transform.position, transform.rotation);
            prisonerSoul.GetComponent<PrisonerSoul>().Target = prisonerSoulTarget.position;
            StartCoroutine(DelayedKill());
        } else {
            StartCoroutine(DelayedKill());
        }
    }

    //Used to give death sound time to complete before destroying the prisoner game object
    private IEnumerator DelayedKill() {
        Destroy(_spriteRenderer);
        // if(_gotHitAudioSource != null)
        //     SoundFXManager.obj.FadeOutAndStopSound(_gotHitAudioSource, 0.2f);
        _rigidBody.bodyType = RigidbodyType2D.Static;
        _collider.enabled = false;
        _lightSprite2DFadeManager.StartFadeOut();
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private void SpawnPrisoners() {
        foreach(GameObject prisoner in prisonersToSpawn) {
            prisoner.SetActive(true);
        }
    }

    // private void OnDrawGizmos()
    // {
    //     if(_collider != null) {
    //         Gizmos.color = Color.cyan;
    //         Vector2 boxSize = new Vector2(_collider.bounds.size.x, 0.1f);
    //         Vector3 boxCastOrigin = _collider.bounds.center;
    //         Vector3 boxCastEnd = boxCastOrigin + Vector3.down * isGroundedCheckOffset;
            
    //         Gizmos.DrawWireCube(boxCastEnd, boxSize);
    //         Gizmos.DrawLine(boxCastOrigin, boxCastEnd);
    //     }
    // }
}
