using UnityEngine;
using System.Collections;

public class TreeBranchPlatform : MonoBehaviour
{
    [SerializeField] Transform _spriteTransform;
    [SerializeField] ParticleSystem _leaves;

    [Header("Bounce Settings")]
    [SerializeField] private float _bounceDownDuration = 0.15f;
    [SerializeField] private float _bounceUpDuration = 0.25f;
    [SerializeField] private float _bounceDistance = 0.1f;
    [SerializeField] private int _numberOfLeavesToEmit = 20;

    private ChildCollider _collider;
    private BoxCollider2D _platformCollider;
    private Vector3 _originalSpriteTransformPosition;
    private Coroutine _bounceCoroutine;
    private bool _hasBouncedThisCollision = false;
    public float _collisionMargin = 0.2f;

    void Awake() {
        _collider = GetComponentInChildren<ChildCollider>();
        _platformCollider = GetComponentInChildren<BoxCollider2D>();
        _originalSpriteTransformPosition = _spriteTransform.position;
    }

    void OnEnable()
    {
        _collider.OnCollisionEnterEvent += HandleCollisionEnter;
        _collider.OnCollisionStayEvent += HandleCollisionStay;
        _collider.OnCollisionExitEvent += HandleCollisionExit;
    }

    void OnDisable()
    {
        _collider.OnCollisionEnterEvent -= HandleCollisionEnter;
        _collider.OnCollisionStayEvent -= HandleCollisionStay;
        _collider.OnCollisionExitEvent -= HandleCollisionExit;
    }

    private void HandleCollisionEnter(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) {
            return;
        }

        if (IsCollisionFromTop(collision) && !_hasBouncedThisCollision) {
            _hasBouncedThisCollision = true;
            OnLandedOn();
        }
    }

    private void HandleCollisionStay(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) {
            return;
        }

        if (IsCollisionFromTop(collision) && !_hasBouncedThisCollision) {
            _hasBouncedThisCollision = true;
            OnLandedOn();
        }
    }

    private void HandleCollisionExit(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            _hasBouncedThisCollision = false;
        }
    }

    private bool IsCollisionFromTop(Collision2D collision) {
        Bounds playerCollisionBounds = collision.collider.bounds;
        Bounds platformBounds = _platformCollider.bounds;
        Vector2 playerBottom = new(playerCollisionBounds.center.x, playerCollisionBounds.center.y - playerCollisionBounds.extents.y);
        Vector2 platformTop = new(platformBounds.center.x, platformBounds.center.y + platformBounds.extents.y);

        return playerBottom.y + _collisionMargin > platformTop.y;
    }

    private void OnLandedOn() {
        if (_bounceCoroutine != null) {
            StopCoroutine(_bounceCoroutine);
        }
        _bounceCoroutine = StartCoroutine(BounceCoroutine());
        _leaves.Emit(_numberOfLeavesToEmit);
    }

    private IEnumerator BounceCoroutine() {
        Vector3 bouncePos = _originalSpriteTransformPosition + Vector3.down * _bounceDistance;

        float elapsed = 0f;
        while (elapsed < _bounceDownDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / _bounceDownDuration;
            _spriteTransform.position = Vector3.Lerp(_originalSpriteTransformPosition, bouncePos, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < _bounceUpDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / _bounceUpDuration;
            _spriteTransform.position = Vector3.Lerp(bouncePos, _originalSpriteTransformPosition, t);
            yield return null;
        }

        _spriteTransform.position = _originalSpriteTransformPosition;
        _bounceCoroutine = null;
    }
}
