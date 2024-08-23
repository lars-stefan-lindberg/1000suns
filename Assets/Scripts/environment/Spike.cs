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
    private bool _hit = false;
    public float castDistance = 0;
    public float gravity = 0;
    private Vector2 _startingPosition;
    private float _respawnTimer = 0f;
    private float spawnTime = 3f;
    private Color _fadeStartColor;
    [Range(0.1f, 10f), SerializeField] private float _fadeSpeed = 5f;

    private void Awake() {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _collider.enabled = false;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _animator.enabled = false;
        _startingPosition = transform.position;
        _fadeStartColor = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 0);
    }

    private void Update() {
        Debug.DrawRay(transform.position, Vector3.down * castDistance, Color.red);
        if (!_isFalling && !_hit) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, castDistance);

            if(hit.transform != null) {
                if(hit.transform.CompareTag("Player")) {
                    _collider.enabled = true;
                    _isFalling = true;
                    _rigidBody.gravityScale = gravity;
                }
            }
        }
        if(_isRespawning) {
            _respawnTimer += Time.deltaTime;
            if(_respawnTimer >= spawnTime) {
                StartCoroutine(Spawn());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(!_isFalling)
            return;
        _hit = true;
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
        _hit = false;
    }
}
