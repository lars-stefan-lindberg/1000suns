using System.Collections;
using FunkyCode;
using UnityEngine;

public class UmmaraBodyManager : MonoBehaviour
{
    [SerializeField] private Animator _scaleAnimator;
    [SerializeField] private LightSprite2DOverlayPulser2 _lightPulsator;
    [SerializeField] private LightSprite2D _light;
    [SerializeField] private float _lightFadeOutDuration = 1f;


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
        float startAlpha = _light.meshMode.alpha;
        float elapsed = 0f;
        
        while (elapsed < _lightFadeOutDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / _lightFadeOutDuration;
            _light.meshMode.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        
        _light.meshMode.alpha = 0f;
    }
}
