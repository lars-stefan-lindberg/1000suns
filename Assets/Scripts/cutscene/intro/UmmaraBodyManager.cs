using System.Collections;
using FunkyCode;
using UnityEngine;

public class UmmaraBodyManager : MonoBehaviour
{
    [SerializeField] private Animator _scaleAnimator;
    [SerializeField] private LightSprite2DOverlayPulser2 _lightPulsator;
    [SerializeField] private LightSprite2D _light;
    [SerializeField] private float _lightFadeOutDuration = 1f;
    [SerializeField] private SpriteRenderer _renderer;


    public void Deactivate() {
        StartCoroutine(DeactivateCoroutine());
    }

    private IEnumerator DeactivateCoroutine() {
        AnimatorStateInfo stateInfo = _scaleAnimator.GetCurrentAnimatorStateInfo(0);
        float remainingTime = (1f - stateInfo.normalizedTime) * stateInfo.length;
        yield return new WaitForSeconds(remainingTime);
        
        _scaleAnimator.enabled = false;
        _lightPulsator.enabled = false;
        
        yield return StartCoroutine(FadeOutLight());
        
        _light.enabled = false;
    }

    private IEnumerator FadeOutLight() {
        return FadeOutLight(_lightFadeOutDuration);
    }

    public IEnumerator FadeOutLight(float duration) {
        float startAlpha = _light.meshMode.alpha;
        float elapsed = 0f;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _light.meshMode.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        
        _light.meshMode.alpha = 0f;
    }

    public IEnumerator FadeInLightTo(float targetAlpha, float duration) {
        float startAlpha = _light.meshMode.alpha;
        float elapsed = 0f;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _light.meshMode.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        
        _light.meshMode.alpha = targetAlpha;
    }

    public IEnumerator FadeInSpriteTo(float targetAlpha, float duration) {
        float startAlpha = _renderer.color.a;
        float elapsed = 0f;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, Mathf.Lerp(startAlpha, targetAlpha, t));
            yield return null;
        }
        
        _renderer.color = new Color(_renderer.color.r, _renderer.color.g, _renderer.color.b, targetAlpha);
    }

    public IEnumerator FadeOutSprite(float duration) {
        return FadeInSpriteTo(0f, duration);
    }
}
