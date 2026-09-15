using System.Collections;
using UnityEngine;

public class UmmaraEyesManager : MonoBehaviour
{
    [SerializeField] private float _blinkInterval = 5f;
    [SerializeField] private Animator _leftEyeAnimator;
    [SerializeField] private Animator _rightEyeAnimator;
    [SerializeField] private SpriteRenderer _leftEyeRenderer;
    [SerializeField] private SpriteRenderer _rightEyeRenderer;
    
    private Animator _animator;
    private Coroutine _blinkCoroutine;

    void Awake() {
        _animator = GetComponent<Animator>();
    }

    [ContextMenu("Activate")]
    public void Activate() {
        if (_blinkCoroutine != null) {
            StopCoroutine(_blinkCoroutine);
        }
        Blink();
        _blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    public void Deactivate() {
        if (_blinkCoroutine != null) {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }
        _animator.enabled = false;
        StartCoroutine(DeactivateCoroutine());
    }

    private IEnumerator DeactivateCoroutine() {
        AnimatorStateInfo stateInfo = _leftEyeAnimator.GetCurrentAnimatorStateInfo(0);
        float remainingTime = (1f - stateInfo.normalizedTime) * stateInfo.length;
        yield return new WaitForSeconds(remainingTime);
        
        _leftEyeAnimator.enabled = false;
        _rightEyeAnimator.enabled = false;
    }

    private void Blink() {
        _animator.SetTrigger("blink");
    }

    private IEnumerator BlinkRoutine() {
        while (true) {
            yield return new WaitForSeconds(_blinkInterval);
            Blink();
        }
    }

    public IEnumerator FadeInSpritesTo(float targetAlpha, float duration) {
        StartCoroutine(FadeInSpriteTo(_leftEyeRenderer, targetAlpha, duration));
        StartCoroutine(FadeInSpriteTo(_rightEyeRenderer, targetAlpha, duration));
        yield return null;
    }
    
    private IEnumerator FadeInSpriteTo(SpriteRenderer renderer, float targetAlpha, float duration) {
        float startAlpha = renderer.color.a;
        float elapsed = 0f;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }
        
        renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, targetAlpha);
    }

    public IEnumerator FadeOutSprites(float duration) {
        return FadeInSpritesTo(0f, duration);
    }
}
