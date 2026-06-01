using System.Collections;
using FunkyCode;
using UnityEngine;

public class DeeLightManager : MonoBehaviour
{
    [SerializeField] private GameObject _light;
    [SerializeField] private float _transitionSpeed = 0.7f;
    [SerializeField] private float _maxPullLightSize = 4f;
    [SerializeField] private float _lerpMargin = 0.1f;
    [Header("Pulsating Settings")]
    [SerializeField] private float _pulsateMinSize = 1.5f;
    [SerializeField] private float _pulsateSpeed = 2f;
    private float _defaultLightSize;
    private Light2D _light2D;
    private Coroutine _pushCoroutine;
    private Coroutine _pulsateCoroutine;
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
        if (_pulsateCoroutine != null) {
            StopCoroutine(_pulsateCoroutine);
            _pulsateCoroutine = null;
        }
        _pushCoroutine = StartCoroutine(IncreaseLightSizeCoroutine());
    }

    public void RestoreLightSize() {
        if (_pushCoroutine != null) {
            StopCoroutine(_pushCoroutine);
            _pushCoroutine = null;
        }
        if (_pulsateCoroutine != null) {
            StopCoroutine(_pulsateCoroutine);
            _pulsateCoroutine = null;
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
        while (_light2D.size < _maxPullLightSize) {
            _light2D.size = Mathf.Lerp(_light2D.size, _maxPullLightSize, Time.deltaTime * _transitionSpeed);
            // Clamp to avoid overshooting due to Lerp asymptotic behavior.
            if (Mathf.Abs(_light2D.size - _maxPullLightSize) < _lerpMargin) {
                _light2D.size = _maxPullLightSize;
                break;
            }
            yield return null;
        }
        // Start pulsating once max size is reached
        _pulsateCoroutine = StartCoroutine(PulsateLightCoroutine());
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

    private IEnumerator PulsateLightCoroutine() {
        while (true) {
            // Pulsate between max size and min pulse size
            float time = 0f;
            float halfCycleDuration = 1f / _pulsateSpeed;
            
            // Shrink to min size
            while (time < halfCycleDuration) {
                time += Time.deltaTime;
                float t = time / halfCycleDuration;
                // Smooth sine wave interpolation
                float smoothT = (Mathf.Sin((t - 0.5f) * Mathf.PI) + 1f) / 2f;
                _light2D.size = Mathf.Lerp(_maxPullLightSize, _pulsateMinSize, smoothT);
                yield return null;
            }
            
            time = 0f;
            // Grow back to max size
            while (time < halfCycleDuration) {
                time += Time.deltaTime;
                float t = time / halfCycleDuration;
                // Smooth sine wave interpolation
                float smoothT = (Mathf.Sin((t - 0.5f) * Mathf.PI) + 1f) / 2f;
                _light2D.size = Mathf.Lerp(_pulsateMinSize, _maxPullLightSize, smoothT);
                yield return null;
            }
        }
    }
}
