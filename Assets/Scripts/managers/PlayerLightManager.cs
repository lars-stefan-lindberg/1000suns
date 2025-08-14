using System.Collections;
using FunkyCode;
using UnityEngine;

public class PlayerLightManager : MonoBehaviour
{
    public static PlayerLightManager obj;
    [SerializeField] private GameObject _light;
    [SerializeField] private float _transitionSpeed = 0.7f;
    private float _defaultLightSize;
    private Light2D _light2D;
    private Coroutine _pushCoroutine;

    void Awake() {
        obj = this;
        _light2D = _light.GetComponent<Light2D>();
        _defaultLightSize = _light2D.size;
    }

    void OnDestroy()
    {
        obj = null;
    }

    void Update()
    {
        Transform activeTransform = PlayerManager.obj.GetPlayerTransform();
        if (activeTransform == null) return;

        _light.transform.position = activeTransform.position;
    }

    public void FadeOut() {
        StartCoroutine(FadeOutLight());
    }

    public void PlayerPush() {
        if (_pushCoroutine != null) {
            StopCoroutine(_pushCoroutine);
        }
        _pushCoroutine = StartCoroutine(IncreaseLightSize());
    }

    public void RestorePlayerPush() {
        if (_pushCoroutine != null) {
            StopCoroutine(_pushCoroutine);
            _pushCoroutine = null;
        }
        StartCoroutine(RestoreLightSize());
    }

    private IEnumerator FadeOutLight() {
        float timeElapsed = 0;
        Light2D light = _light2D; // Using FunkyCode.Light2D
        Color startColor = light.color;
        while (timeElapsed < 2f) {
            float t = timeElapsed / 2f;
            t = t * t * (3f - 2f * t);
            light.color = Color.Lerp(startColor, Color.clear, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        light.color = Color.clear;
        yield return null;
    }

    private IEnumerator IncreaseLightSize() {
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

    private IEnumerator RestoreLightSize() {
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
