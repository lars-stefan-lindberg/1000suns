using System.Collections;
using FMOD.Studio;
using Unity.VisualScripting;
using UnityEngine;

public class Block : MonoBehaviour
{
    public LayerMask groundLayer;
    [SerializeField] private float _wallCheckCastDistance = 1.05f;
    [SerializeField] private float _minVelocityForWallHit = 2f;
    [SerializeField] private float _wallDetectionDistance = 0.2f;
    [SerializeField] private float _wallDetectionBoxWidthMultiplier = 0.5f;
    [SerializeField] private bool _showWallDetectionGizmo = true;
    public float basePushPower = 7f;
    public float deceleration = 1f;
    public float _blockSizeOffSet = 1.002f; //To dial in the landing sound
    private bool _isGrounded = true;
    private bool _isTouchingCeiling = false;
    private AudioSource _slideSoundAudioSource;
    private bool _isMovingHorizontally = false;
    private float _frontCheck = 1.35f;
    private float _bootTime = 0.5f; //Used to avoid landing sound on spawn
    private float _bootTimer = 0f;
    private bool _booted = false;
    private float _grounderDistance = 0.1f;
    private Prisoner _prisonerInContact;
    private BlockAudio _blockAudio;
    private EventInstance _blockSlideSfxInstance;
    private bool _isPlayerBeneath = false;
    private PlayerManager.PlayerType _playerType;
    private bool _isBeingPulled = false;
    private Rigidbody2D _rigidBody;
    private BoxCollider2D _collider;
    private BoxCollider2D _childCollider;
    private Pullable _pullable;
    private int _adjacentBlockCount = 0;
    private int _adjacentBlockCountWhenMovementStarted = 0;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _childCollider = GetComponentInChildren<BoxCollider2D>();
        _pullable = GetComponentInChildren<Pullable>();
        _blockAudio = GetComponent<BlockAudio>();
    }

    public void SetGravity(float gravityScale) {
        _rigidBody.gravityScale = gravityScale;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if(_isGrounded && !_pullable.IsPulled)
                _rigidBody.bodyType = RigidbodyType2D.Static;

            if(HitUnderneath(collision) && _rigidBody.velocity.y < -0.05f && !_pullable.IsPulled) {
                PlayerManager.PlayerType playerType = PlayerManager.obj.GetPlayerTypeFromCollider(collision);
                if(PlayerManager.obj.IsPlayerGrounded(playerType)) {
                    _rigidBody.bodyType = RigidbodyType2D.Static;
                    Reaper.obj.KillPlayerGeneric(playerType);
                }
                else {
                    _isPlayerBeneath = true;
                    _playerType = playerType;
                }
            }
        }
        else if (collision.transform.CompareTag("Projectile"))
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            bool hitFromTheLeft = PlayerManager.obj.GetPlayerTransform().position.x < _rigidBody.position.x;

            //Check for prisoner is stuck to a wall. We assume that the prisoner is on the correct side of the block since the projectile is hitting
            if(_prisonerInContact != null) {
                if(_prisonerInContact.isStuck) {
                    return;
                }
            }

            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
            _adjacentBlockCountWhenMovementStarted = _adjacentBlockCount;
            float power = basePushPower * projectile.power;
            _rigidBody.velocity = new Vector2(hitFromTheLeft ? power : -power, 0);
            
            PlaySlideSound();
        } else if(collision.transform.CompareTag("Block")) {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
            _adjacentBlockCount++;
        } else if(collision.transform.CompareTag("Enemy")) {
            _prisonerInContact = collision.GetComponent<Prisoner>();
            if(HitUnderneath(collision)) {
                Prisoner prisoner = collision.GetComponent<Prisoner>();
                if(prisoner.isGrounded) {
                    _rigidBody.bodyType = RigidbodyType2D.Static;
                    Reaper.obj.KillPrisoner(prisoner);
                }
            }
        }
    }

    private void PlaySlideSound() {
        AudioUtils.SafeStop(ref _blockSlideSfxInstance, STOP_MODE.ALLOWFADEOUT);
        _blockAudio.PlaySlide(ref _blockSlideSfxInstance);
    }

    private void StopSlideSound() {
        if(AudioUtils.IsPlaying(_blockSlideSfxInstance))
            AudioUtils.SafeStop(ref _blockSlideSfxInstance);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player") || collision.transform.CompareTag("Enemy"))
        {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
            _isPlayerBeneath = false;
        }
        if(collision.transform.CompareTag("Enemy"))
            _prisonerInContact = null;
        if(collision.transform.CompareTag("Block"))
            _adjacentBlockCount--;
    }

    private void Update()
    {
        if(!_booted) {
            _bootTimer += Time.deltaTime;
            if(_bootTimer >= _bootTime)
                _booted = true;
        }

        if(!_isBeingPulled && _pullable.IsPulled) {
            _isBeingPulled = true;
            _isPlayerBeneath = false;
            _adjacentBlockCountWhenMovementStarted = _adjacentBlockCount;
            PlaySlideSound();
        } else if(_isBeingPulled && !_pullable.IsPulled) {
            _isBeingPulled = false;
        }

        if(_pullable.IsPulled)
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;

        // Don't apply deceleration when being pulled/controlled
        if (_rigidBody.velocity.x != 0 && !_pullable.IsPulled)
        {
            _rigidBody.velocity = new Vector2(Mathf.MoveTowards(_rigidBody.velocity.x, 0, deceleration * Time.deltaTime), _rigidBody.velocity.y);
        } 

        // Debug.DrawLine(
        //    _collider.transform.position,
        //    new Vector3(_collider.transform.position.x, _collider.transform.position.y - _isGroundedCheckOffset, _collider.transform.position.z),
        //    Color.red);

        bool groundHit = Physics2D.BoxCast(_childCollider.bounds.center, _childCollider.size - new Vector2(_blockSizeOffSet, 0), 0, 
            Vector2.down, _grounderDistance, groundLayer);

        if(!groundHit) {
            StopSlideSound();
        }

        bool ceilingHit = Physics2D.BoxCast(_childCollider.bounds.center, _childCollider.size - new Vector2(_blockSizeOffSet, 0), 0, 
            Vector2.up, _grounderDistance, groundLayer);

        _isMovingHorizontally = Mathf.Abs(_rigidBody.velocity.x) > 0.01f;

        if(_isMovingHorizontally) {
            bool isMovingRight = _rigidBody.velocity.x > 0;
            bool isWallAhead = Physics2D.BoxCast(_childCollider.bounds.center, 
                new Vector2(_childCollider.size.x * _wallDetectionBoxWidthMultiplier, _childCollider.size.y), 
                0, 
                isMovingRight ? Vector2.right : Vector2.left, 
                _wallDetectionDistance, 
                groundLayer);
            if(isWallAhead) {
                bool velocityAboveThreshold = Mathf.Abs(_rigidBody.velocity.x) >= _minVelocityForWallHit;
                bool wasNotAdjacentWhenStarted = _adjacentBlockCountWhenMovementStarted == 0;
                bool shouldPlayWallHit = wasNotAdjacentWhenStarted && velocityAboveThreshold;
                if(shouldPlayWallHit) {
                    _blockAudio.PlayWallHit();
                }
            }
        }

        if(!_isGrounded && groundHit && _booted)
            _blockAudio.PlayLand();
        if(_isGrounded && !groundHit && _isMovingHorizontally)
            _blockAudio.PlaySlideOffEdge();
        
        if(!_isTouchingCeiling && ceilingHit && _pullable.IsPulled)
            _blockAudio.PlayLand();
        
        _isGrounded = groundHit;
        _isTouchingCeiling = ceilingHit;

        if(!_isMovingHorizontally) {
            StopSlideSound();
        }

        if(_isGrounded && !_isMovingHorizontally && !_pullable.IsPulled && _adjacentBlockCount == 0) {
            _rigidBody.bodyType = RigidbodyType2D.Static;
        } else {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void FixedUpdate() {
        if(_isPlayerBeneath && !_pullable.IsPulled) {
            if(PlayerManager.obj.IsPlayerGrounded(_playerType))
                Reaper.obj.KillPlayerGeneric(_playerType);
        }
    }

    private bool HitUnderneath(Collider2D collider) {
        //For reference:
        //Vector2 topRight = new Vector2(boxBounds.center.x + boxBounds.extents.x, boxBounds.center.y + boxBounds.extents.y);
        Bounds collisionBounds = collider.bounds;
        Vector2 top = new(collisionBounds.center.x, collisionBounds.center.y + collisionBounds.extents.y);

        Bounds blockBounds = _childCollider.bounds;
        Vector2 bottom = new(blockBounds.center.x, blockBounds.center.y - blockBounds.extents.y);
        float margin = 0.3f;
        return bottom.y > top.y - margin;
    }

    private void OnDrawGizmos() {
        if(!_showWallDetectionGizmo || _childCollider == null)
            return;

        if(_isMovingHorizontally) {
            bool isMovingRight = _rigidBody.velocity.x > 0;
            Vector2 boxSize = new Vector2(_childCollider.size.x * _wallDetectionBoxWidthMultiplier, _childCollider.size.y);
            Vector2 direction = isMovingRight ? Vector2.right : Vector2.left;
            Vector2 center = _childCollider.bounds.center;
            
            // Draw the initial box
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, boxSize);
            
            // Draw the end box
            Vector2 endCenter = center + direction * _wallDetectionDistance;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(endCenter, boxSize);
            
            // Draw lines connecting the boxes
            Gizmos.color = Color.cyan;
            Vector2 halfSize = boxSize * 0.5f;
            Gizmos.DrawLine(center + new Vector2(0, halfSize.y), endCenter + new Vector2(0, halfSize.y));
            Gizmos.DrawLine(center - new Vector2(0, halfSize.y), endCenter - new Vector2(0, halfSize.y));
        }
    }
}
