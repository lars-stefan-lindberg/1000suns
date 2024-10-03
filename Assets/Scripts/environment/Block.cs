using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private BoxCollider2D _collider;
    private BoxCollider2D _childCollider;

    public LayerMask groundLayer;
    [SerializeField] private float _wallCheckCastDistance = 1.05f;
    private bool _isGrounded = true;
    private float _isGroundedCheckOffset;
    private AudioSource _slideSoundAudioSource;
    private bool _isMovingHorizontally = false;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _childCollider = GetComponentInChildren<BoxCollider2D>();
        Bounds blockBounds = _childCollider.bounds;
        _isGroundedCheckOffset = blockBounds.extents.y + 0.01f;
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
                if(PlayerMovement.obj.isGrounded)
                    Reaper.obj.KillPlayerGeneric();
                else
                    isPlayerBeneath = true;
            }
        }
        else if (collision.transform.CompareTag("Projectile"))
        {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            bool hitFromTheLeft = Player.obj.transform.position.x < _rigidBody.position.x;
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
            }
        } else if(collision.transform.CompareTag("Enemy")) {
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
    }

    public float deceleration = 1f;
    private void Update()
    {
        if (_rigidBody.velocity.x != 0)
        {
            _rigidBody.velocity = new Vector2(Mathf.MoveTowards(_rigidBody.velocity.x, 0, deceleration * Time.deltaTime), _rigidBody.velocity.y);
        } 

        Vector3 groundLineCastPosition = _collider.transform.position;
        // Debug.DrawLine(
        //    groundLineCastPosition,
        //    new Vector3(groundLineCastPosition.x, groundLineCastPosition.y - _isGroundedCheckOffset, groundLineCastPosition.z),
        //    Color.red);
        _isGrounded = Physics2D.Linecast(
            groundLineCastPosition,
            new Vector3(groundLineCastPosition.x, groundLineCastPosition.y - _isGroundedCheckOffset, groundLineCastPosition.z),
            groundLayer);
        
        if(!_isGrounded) {
            if(_slideSoundAudioSource != null && _slideSoundAudioSource.isPlaying) {
                _slideSoundAudioSource.mute = true;
            }
        } else {
            if(_slideSoundAudioSource != null && _slideSoundAudioSource.mute) {
                _slideSoundAudioSource.mute = false;
            }
        }

        _isMovingHorizontally = Mathf.Abs(_rigidBody.velocity.x) > 0.01f;

        if(!_isMovingHorizontally && _slideSoundAudioSource != null && _slideSoundAudioSource.isPlaying)
            _slideSoundAudioSource.mute = true;
    }

    private void FixedUpdate() {
        if(isPlayerBeneath) {
            if(PlayerMovement.obj.isGrounded)
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
