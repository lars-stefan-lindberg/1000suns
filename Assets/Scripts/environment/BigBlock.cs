using UnityEngine;

public class BigBlock : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private BoxCollider2D _collider;
    private Pullable _pullable;

    public LayerMask groundLayer;
    private ParticleSystem _shakeAnimation;
    [SerializeField] int numberOfShakeParticles = 50;
    private bool _isGrounded = true;
    private AudioSource _slideSoundAudioSource;
    private bool _isMovingHorizontally = false;

    private float _bootTime = 0.5f; //Used to avoid landing sound on spawn
    private float _bootTimer = 0f;
    private bool _booted = false;

    private float _grounderDistance = 1.62f;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponentInChildren<BoxCollider2D>();
        _pullable = GetComponentInChildren<Pullable>();
        _shakeAnimation = GetComponentInChildren<ParticleSystem>();
    }

    public void SetGravity(float gravityScale) {
        _rigidBody.gravityScale = gravityScale;
    }

    public float basePushPower = 7f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            if(_pullable.IsPulled)
            {
                Projectile projectile = collision.gameObject.GetComponent<Projectile>();
                bool hitFromTheLeft = PlayerManager.obj.GetPlayerTransform().position.x < _rigidBody.position.x;

                float power = basePushPower * projectile.power;
                _rigidBody.velocity = new Vector2(hitFromTheLeft ? power : -power, 0);

                float clipLength = projectile.power / PlayerPush.obj.maxForce;
                if(_slideSoundAudioSource == null || !_slideSoundAudioSource.isPlaying) {
                    _slideSoundAudioSource = SoundFXManager.obj.PlayBlockSliding(transform, clipLength);
                } else {
                    _slideSoundAudioSource.Stop();
                    _slideSoundAudioSource = null;
                    _slideSoundAudioSource = SoundFXManager.obj.PlayBlockSliding(transform, clipLength);
                }
            } else {
                PlayMovableHint();
            }
        }
    }

    public float deceleration = 1f;
    private bool _isBeingPulled = false;

    private void PlayMovableHint() {
        SoundFXManager.obj.PlayBreakableWallCrackling(transform);
        _shakeAnimation.Emit(numberOfShakeParticles);
    }

    private void Update()
    {
        if(!_booted) {
            _bootTimer += Time.deltaTime;
            if(_bootTimer >= _bootTime)
                _booted = true;
        }

        if(_pullable.IsPulled && !_isBeingPulled) {
            _isBeingPulled = true;
            PlayMovableHint();
        } else if (!_pullable.IsPulled && _isBeingPulled) {
            _isBeingPulled = false;
        }

        if (_rigidBody.velocity.x != 0)
        {
            _rigidBody.velocity = new Vector2(Mathf.MoveTowards(_rigidBody.velocity.x, 0, deceleration * Time.deltaTime), _rigidBody.velocity.y);
        } 

        bool groundHit = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, 
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

        if(!_isGrounded && groundHit && _booted)
            SoundFXManager.obj.PlayBlockLand(transform);
        if(_isGrounded && !groundHit && _isMovingHorizontally)
            SoundFXManager.obj.PlayBlockSlideOffEdge(transform);
        
        _isGrounded = groundHit;

        if(!_isMovingHorizontally && _slideSoundAudioSource != null && _slideSoundAudioSource.isPlaying)
            _slideSoundAudioSource.mute = true;
    }
}
