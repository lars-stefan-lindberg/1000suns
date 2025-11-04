using System.Collections;
using UnityEngine;

public class Moths : MonoBehaviour
{
    private Animator _animator;
    [SerializeField] private SpriteRenderer _moth1Renderer;
    [SerializeField] private SpriteRenderer _moth2Renderer;

    public bool stopFollowingPlayer = false;
    private Color _fadeStartColor;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.enabled = false;
        _moth1Renderer.enabled = false;
        _moth2Renderer.enabled = false;
        _fadeStartColor = new Color(_moth1Renderer.color.r, _moth1Renderer.color.g, _moth1Renderer.color.b, 1f);
    }

    public void Activate() {
        stopFollowingPlayer = false;
        _fadeStartColor.a = 1f;
        _moth1Renderer.color = _fadeStartColor;
        _moth2Renderer.color = _fadeStartColor;
        _moth1Renderer.enabled = true;
        _moth2Renderer.enabled = true;
        _animator.enabled = true;
    }

    public void Deactivate() {
        if(_fadeOutCoroutine != null) {
            StopCoroutine(_fadeOutCoroutine);
        }
        _animator.enabled = false;
        _moth1Renderer.enabled = false;
        _moth2Renderer.enabled = false;
        stopFollowingPlayer = false;
    }

    private Coroutine _fadeOutCoroutine;
    private IEnumerator SpriteFadeOutAndCancel(float targetAlpha, float duration) {
        float elapsed = 0f;
        // Ensure starting alpha is one
        _fadeStartColor.a = 1f;
        while(elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _fadeStartColor.a = Mathf.Lerp(1f, targetAlpha, t);
            _moth1Renderer.color = _fadeStartColor;
            _moth2Renderer.color = _fadeStartColor;
            yield return null;
        }
        // Ensure final alpha is exactly the target
        _fadeStartColor.a = targetAlpha;
        _moth1Renderer.color = _fadeStartColor;
        _moth2Renderer.color = _fadeStartColor;

        _moth1Renderer.enabled = false;
        _moth2Renderer.enabled = false;
        _animator.enabled = false;

        MothsManager.obj.Remove(gameObject);
    }

    private Coroutine _moveToTargetCoroutine;
    public void SetTarget(Vector2 target) {
        stopFollowingPlayer = true;
        if(_moveToTargetCoroutine != null) {
            StopCoroutine(_moveToTargetCoroutine);
        }
        _moveToTargetCoroutine = StartCoroutine(MoveToTarget(target));
    }

    [SerializeField] private float _moveSpeed = 0.1f;

    private IEnumerator MoveToTarget(Vector2 target) {
        float elapsed = 0f;
        while(elapsed < 1.5f) {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(transform.position, target, _moveSpeed);
            yield return null;
        }
        yield return new WaitForSeconds(3f);
        if(_fadeOutCoroutine != null) {
            StopCoroutine(_fadeOutCoroutine);
        }
        _fadeOutCoroutine = StartCoroutine(SpriteFadeOutAndCancel(0f, 0.5f));
    }

    void Update()
    {
        if(PlayerManager.obj != null && !stopFollowingPlayer) {
            Transform playerTransform = PlayerManager.obj.GetPlayerTransform();
            if(playerTransform != null) {
                transform.position = new Vector2(playerTransform.position.x, playerTransform.position.y);
            }
        }
    }
}
