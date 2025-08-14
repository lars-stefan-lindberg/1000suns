using System.Collections;
using UnityEngine;

public class ChargeAnimationMgr : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    
    [SerializeField] private float _animationFadeMultiplier = 7f;

    private bool _isStarted = false;
    private float _playerOffset = 0.2f;
    private Color _fadeStartColor;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator.enabled = false;
        _spriteRenderer.enabled = false;
        _fadeStartColor = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1f);
    }

    public void Charge() {
        _fadeStartColor.a = 0f;
        _spriteRenderer.color = _fadeStartColor;
        _spriteRenderer.enabled = true;
        _animator.enabled = true;
        StartCoroutine(SpriteFadeInCharge(0.5f, 0.5f));
        if(!_isStarted) {
            _isStarted = true;
            _animator.Play("charge_start");
        }
    }

    public void FullyCharged() {
        _fadeStartColor.a = 1f;
        _spriteRenderer.color = _fadeStartColor;
        _animator.SetBool("fullyCharged", true);
    }

    public void FullyChargedPoweredUp() {
        _fadeStartColor.a = 1f;
        _spriteRenderer.color = _fadeStartColor;
        _animator.SetBool("fullyCharged", true);
        //Save for future reference, if I want to change the fully charged powered up animation
        //_animator.SetBool("poweredUp", true);
    }

    public void Cancel() {
        _isStarted = false;
        StartCoroutine(SpriteFadeOutAndCancel(_animationFadeMultiplier));
    }

    public void HardCancel() {
        _animator.SetBool("fullyCharged", false);
        //_animator.SetBool("poweredUp", false);
        _animator.enabled = false;
        _fadeStartColor.a = 0;
        _spriteRenderer.color = _fadeStartColor;
        _spriteRenderer.enabled = false;
    }

    private IEnumerator SpriteFadeOutAndCancel(float fadeMultiplier) {
        while(_spriteRenderer.color.a > 0) {
            _fadeStartColor.a -= Time.deltaTime * fadeMultiplier;
            _spriteRenderer.color = _fadeStartColor;
            yield return null;
        }

        _animator.SetBool("fullyCharged", false);
        //_animator.SetBool("poweredUp", false);
        _animator.enabled = false;
        _spriteRenderer.enabled = false;
    }

    // Coroutine to fade in the sprite renderer's alpha to a target value over a given duration
    private IEnumerator SpriteFadeInCharge(float targetAlpha, float duration) {
        float elapsed = 0f;
        // Ensure starting alpha is zero (already set in Charge)
        _fadeStartColor.a = 0f;
        while(elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _fadeStartColor.a = Mathf.Lerp(0f, targetAlpha, t);
            _spriteRenderer.color = _fadeStartColor;
            yield return null;
        }
        // Ensure final alpha is exactly the target
        _fadeStartColor.a = targetAlpha;
        _spriteRenderer.color = _fadeStartColor;
    }

    void Update()
    {
        if(Player.obj != null)
            transform.position = new Vector2(Player.obj.transform.position.x, Player.obj.transform.position.y + _playerOffset);
    }
}
