using System.Collections;
using FunkyCode;
using UnityEngine;

public class PlayerLightManager : MonoBehaviour
{
    [SerializeField] private GameObject _light;
    [SerializeField] private float _transitionSpeed = 0.7f;
    private float _defaultLightSize;
    private Light2D _light2D;
    private Coroutine _pushCoroutine;
    private Color _originalLightColor;

    void Awake() {
        _light2D = _light.GetComponent<Light2D>();
        _defaultLightSize = _light2D.size;
        _originalLightColor = _light2D.color;
    }

    public void FadeOut() {
        StartCoroutine(FadeOutLight());
    }

    public void FadeIn() {
        StartCoroutine(FadeInLight());
    }

    public void IncreaseLightSize() {
        if (_pushCoroutine != null) {
            StopCoroutine(_pushCoroutine);
        }
        _pushCoroutine = StartCoroutine(IncreaseLightSizeCoroutine());
    }

    public void RestoreLightSize() {
        if (_pushCoroutine != null) {
            StopCoroutine(_pushCoroutine);
            _pushCoroutine = null;
        }
        StartCoroutine(RestoreLightSizeCoroutine());
    }

    private IEnumerator FadeOutLight() {
        float timeElapsed = 0f;
        float duration = 1f;
        Light2D light = _light2D; // Using FunkyCode.Light2D
        Color startColor = light.color;
        Color endColor = Color.clear;
        while (timeElapsed < duration) {
            float t = Mathf.Clamp01(timeElapsed / duration);
            t = t * t * (3f - 2f * t);
            light.color = Color.Lerp(startColor, endColor, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        light.color = endColor;
        yield return null;
    }
    
    private IEnumerator FadeInLight() {
        float timeElapsed = 0f;
        float duration = 1f;
        Light2D light = _light2D; // Using FunkyCode.Light2D
        Color startColor = Color.clear;
        Color endColor = _originalLightColor;
        while (timeElapsed < duration) {
            float t = Mathf.Clamp01(timeElapsed / duration);
            t = t * t * (3f - 2f * t);
            light.color = Color.Lerp(startColor, endColor, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        light.color = endColor;
        yield return null;
    }

    private IEnumerator IncreaseLightSizeCoroutine() {
        float targetSize = _defaultLightSize * 2f;
        while (_light2D.size < targetSize) {
            _light2D.size = Mathf.Lerp(_light2D.size, targetSize, Time.deltaTime * _transitionSpeed);
            // Clamp to avoid overshooting due to Lerp asymptotic behavior.
            if (Mathf.Abs(_light2D.size - targetSize) < 0.01f) {
                _light2D.size = targetSize;
                break;
            }
            yield return null;
        }
    }

    private IEnumerator RestoreLightSizeCoroutine() {
        float duration = 0.2f;
        float elapsed = 0f;
        float startSize = _light2D.size;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Smooth interpolation (ease out)
            _light2D.size = Mathf.Lerp(startSize, _defaultLightSize, t * t);
            yield return null;
        }
        _light2D.size = _defaultLightSize;
    }
}
