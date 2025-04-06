using Unity.VisualScripting;
using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private BoxCollider2D _collider;
    private BoxCollider2D _childCollider;

    public LayerMask groundLayer;
    [SerializeField] private float _wallCheckCastDistance = 1.05f;
    private bool _isGrounded = true;
    private AudioSource _slideSoundAudioSource;
    private bool _isMovingHorizontally = false;
    private float _frontCheck = 1.35f;
    private float _wallImpactCoolDownTime = 0.5f;
    private float _wallImpactCoolDownTimer = 1f;

    private float _bootTime = 0.5f; //Used to avoid landing sound on spawn
    private float _bootTimer = 0f;
    private bool _booted = false;

    private float _grounderDistance = 0.1f;

    private Prisoner _prisonerInContact;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _childCollider = GetComponentInChildren<BoxCollider2D>();
    }

    public float basePushPower = 7f;
    private bool isPlayerBeneath = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if(_isGrounded)
                _rigidBody.bodyType = RigidbodyType2D.Static;

            if(HitUnderneath(collision)) {
                if(PlayerManager.obj.IsPlayerGrounded()) {
                    _rigidBody.bodyType = RigidbodyType2D.Static;
                    Reaper.obj.KillPlayerGeneric();
                }
                else
                    isPlayerBeneath = true;
            }
        }
        else if (collision.transform.CompareTag("Projectile"))
        {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            bool hitFromTheLeft = PlayerManager.obj.GetPlayerTransform().position.x < _rigidBody.position.x;

            //Check for prisoner is stuck to a wall. We assume that the prisoner is on the correct side of the block since the projectile is hitting
            if(_prisonerInContact != null) {
                if(_prisonerInContact.isStuck) {
                    return;
                }
            }

            float power = basePushPower * projectile.power;
            _rigidBody.velocity = new Vector2(hitFromTheLeft ? power : -power, 0);
            bool somethingToTheRight = Physics2D.Raycast(_collider.bounds.center, Vector2.right, _wallCheckCastDistance, groundLayer);
            if(hitFromTheLeft && somethingToTheRight)
                return;
            bool somethingToTheLeft = Physics2D.Raycast(_collider.bounds.center, Vector2.left, _wallCheckCastDistance, groundLayer);
            if(!hitFromTheLeft && somethingToTheLeft)
                return;

            float clipLength = projectile.power / PlayerPush.obj.maxForce;
            if(_slideSoundAudioSource == null || !_slideSoundAudioSource.isPlaying) {
                _slideSoundAudioSource = SoundFXManager.obj.PlayBlockSliding(transform, clipLength);
            } else {
                _slideSoundAudioSource.Stop();
                _slideSoundAudioSource = null;
                _slideSoundAudioSource = SoundFXManager.obj.PlayBlockSliding(transform, clipLength);
            }
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player") || collision.transform.CompareTag("Enemy"))
        {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
            isPlayerBeneath = false;
        }
        if(collision.transform.CompareTag("Enemy"))
            _prisonerInContact = null;
    }

    public float deceleration = 1f;
    private float _blockSizeOffSet = 1.002f; //To dial in the landing sound

    private void Update()
    {
        if(!_booted) {
            _bootTimer += Time.deltaTime;
            if(_bootTimer >= _bootTime)
                _booted = true;
        }

        if (_rigidBody.velocity.x != 0)
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
            if(_slideSoundAudioSource != null && _slideSoundAudioSource.isPlaying) {
                _slideSoundAudioSource.mute = true;
            }
        } else {
            if(_slideSoundAudioSource != null && _slideSoundAudioSource.mute) {
                _slideSoundAudioSource.mute = false;
            }
        }

        _isMovingHorizontally = Mathf.Abs(_rigidBody.velocity.x) > 0.01f;

        if(_isMovingHorizontally) {
            bool isMovingRight = _rigidBody.velocity.x > 0;
            bool isWallAhead = Physics2D.Raycast(new Vector3(_collider.transform.position.x, _collider.transform.position.y - 0.2f, 
                _collider.transform.position.z), 
                isMovingRight ? Vector2.right : Vector2.left, _frontCheck, groundLayer);
            if(isWallAhead) {
                if(_wallImpactCoolDownTimer >= _wallImpactCoolDownTime) {
                    SoundFXManager.obj.PlayBlockWallImpact(transform);
                    _wallImpactCoolDownTimer = 0;
                } else {
                    _wallImpactCoolDownTimer += Time.deltaTime;
                }
            }
        } else {
            _wallImpactCoolDownTimer = 1f;
        }

        if(!_isGrounded && groundHit && _booted)
            SoundFXManager.obj.PlayBlockLand(transform);
        if(_isGrounded && !groundHit && _isMovingHorizontally)
            SoundFXManager.obj.PlayBlockSlideOffEdge(transform);
        
        _isGrounded = groundHit;

        if(!_isMovingHorizontally && _slideSoundAudioSource != null && _slideSoundAudioSource.isPlaying)
            _slideSoundAudioSource.mute = true;
    }

    private void FixedUpdate() {
        if(isPlayerBeneath) {
            if(PlayerManager.obj.IsPlayerGrounded())
                Reaper.obj.KillPlayerGeneric();
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
}
