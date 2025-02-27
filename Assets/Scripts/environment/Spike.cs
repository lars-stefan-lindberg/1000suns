using System.Collections;
using UnityEngine;

public class Spike : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private BoxCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    public bool isRespawnable = false;
    private bool _isRespawning = false;
    private bool _isFalling = false;
    private bool _startFalling = false;
    private bool _hasDetectedPlayer = false;
    public float castDistance = 0;
    public float gravity = 0;
    private Vector2 _startingPosition;
    private float _respawnTimer = 0f;
    private float spawnTime = 3f;
    private Color _fadeStartColor;
    [Range(0.1f, 10f), SerializeField] private float _fadeSpeed = 5f;
    [SerializeField] private GameObject _dustParticles;

    private float _originXPosition;
    private readonly float _shakeDistance = 0.1f;
    private readonly float _shakeTime = 0.12f;
    private readonly float _shakeFrameWait = 0.08f;
    private readonly float _shakeTimeTotal = 0.4f;

    private void Awake() {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _collider.enabled = false;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _animator.enabled = false;
        _startingPosition = transform.position;
        _fadeStartColor = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 0);
        _originXPosition = _spriteRenderer.transform.position.x;
    }

    private readonly float _raycastOffsetX = 2;
    [SerializeField] private float _raycastOffsetY = 0;

    private void Update() {
        // Debug.DrawRay(new Vector2(transform.position.x - _raycastOffsetX, transform.position.y + _raycastOffsetY), Vector3.down * (castDistance + _raycastOffsetY), Color.red);
        // Debug.DrawRay(new Vector2(transform.position.x + _raycastOffsetX, transform.position.y + _raycastOffsetY), Vector3.down * (castDistance + _raycastOffsetY), Color.red);
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y + _raycastOffsetY), Vector3.down * (castDistance + _raycastOffsetY), Color.red);
        if (!_isFalling && !_hasDetectedPlayer) {
            //RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(transform.position.x - _raycastOffsetX, transform.position.y + _raycastOffsetY), Vector3.down, castDistance + _raycastOffsetY);
            //RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(transform.position.x + _raycastOffsetX, transform.position.y + _raycastOffsetY), Vector3.down, castDistance + _raycastOffsetY);
            RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + _raycastOffsetY), Vector3.down, castDistance + _raycastOffsetY);
            bool hit = false;
            // if(hitLeft.transform != null) {
            //     if(hitLeft.transform.CompareTag("Player")) {
            //         hit = true;
            //     }
            // }
            if(hitRight.transform != null) {
                if(hitRight.transform.CompareTag("Player")) {
                    hit = true;
                }
            }

            if(hit) {
                GameObject dustParticles = Instantiate(_dustParticles, transform);
                dustParticles.transform.parent = null;
                dustParticles.GetComponent<ParticleSystem>().Play();
                _startFalling = true;
                _isFalling = true;
                StartCoroutine(ShakeBeforeFall(dustParticles));
            }
        }
        if(_startFalling) {
            _startFalling = false;
            SoundFXManager.obj.PlayFallingSpikeCrackling(transform);
        }
        if(_isRespawning) {
            _respawnTimer += Time.deltaTime;
            if(_respawnTimer >= spawnTime) {
                StartCoroutine(Spawn());
            }
        }
    }

    private IEnumerator ShakeBeforeFall(GameObject dustParticles) {
        //Shake spike
        float leftX = _originXPosition - _shakeDistance;
        float rightX = _originXPosition + _shakeDistance;
        float[] positions = new float[2] {leftX, rightX};
        float time = 0f;
        int index = 1;
        while(time <= _shakeTimeTotal) {
            time += (Time.deltaTime / _shakeTime) + _shakeFrameWait;
            index += 1;
            _spriteRenderer.transform.position = new Vector2(positions[index % 2], _spriteRenderer.transform.position.y);
            yield return new WaitForSeconds(_shakeFrameWait);
        }
        _spriteRenderer.transform.position = new Vector2(_originXPosition, _spriteRenderer.transform.position.y);

        Destroy(dustParticles, 1);
        _collider.enabled = true;
        _rigidBody.gravityScale = gravity;
        SoundFXManager.obj.PlayFallingSpikeFall(transform);
        yield return null;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(!_isFalling)
            return;
        SoundFXManager.obj.PlayFallingSpikeHit(transform);
        _hasDetectedPlayer = true;
        _isFalling = false;
        _rigidBody.velocity = Vector3.zero;
        _rigidBody.gravityScale = 0;
        _collider.enabled = false;
        if(collision.transform.CompareTag("Player")) {
            Reaper.obj.KillPlayerGeneric();
        } else if(collision.transform.CompareTag("Enemy")) {
            Prisoner prisoner = collision.gameObject.GetComponent<Prisoner>();
            Reaper.obj.KillPrisoner(prisoner);
        } 
        StartCoroutine(MakeInvisibleAfterBreak(_breakAnimationTime));
        if(isRespawnable) {
            _isRespawning = true;
            _respawnTimer = 0f;
        }
    }

    private float _breakAnimationTime = 0.5f;
    private IEnumerator MakeInvisibleAfterBreak(float breakAnimationTime) {
        _animator.enabled = true;
        _animator.SetTrigger("break");
        yield return new WaitForSeconds(breakAnimationTime);
        _fadeStartColor.a = 0;
        _spriteRenderer.color = _fadeStartColor;
        _animator.Rebind();
        _animator.Update(0f);
        _animator.enabled = false; 
    }

    private IEnumerator Spawn() {
        transform.position = _startingPosition;
        while(_spriteRenderer.color.a < 1f) {
            _fadeStartColor.a += Time.deltaTime * _fadeSpeed;
            _spriteRenderer.color = _fadeStartColor;
            yield return null;
        }
        _isRespawning = false;
        _collider.enabled = true;
        _hasDetectedPlayer = false;
    }
}
