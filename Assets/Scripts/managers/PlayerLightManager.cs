using System.Collections;
using FunkyCode;
using UnityEngine;

public class PlayerLightManager : MonoBehaviour
{
    public static PlayerLightManager obj;
    [SerializeField] private GameObject _light;
    [SerializeField] private float _transitionSpeed = 5f;

    private bool _isTransitioning = false;
    private Transform _lastActiveTransform = null;

    void Awake() {
        obj = this;
    }

    void OnDestroy()
    {
        obj = null;
    }

    void Update()
    {
        Transform activeTransform = PlayerManager.obj.GetPlayerTransform();
        if (activeTransform == null) return;

        // Detect transformation event
        if (_lastActiveTransform != null && activeTransform != _lastActiveTransform)
        {
            _isTransitioning = true;
        }

        if (_isTransitioning)
        {
            Vector3 targetPos = activeTransform.position;
            _light.transform.position = Vector3.Lerp(_light.transform.position, targetPos, Time.deltaTime * _transitionSpeed);
            // End transition if close enough to the current position
            if (Vector3.Distance(_light.transform.position, targetPos) < 0.05f)
            {
                _light.transform.position = targetPos;
                _isTransitioning = false;
            }
        }
        else
        {
            _light.transform.position = activeTransform.position;
        }

        _lastActiveTransform = activeTransform;
    }

    public void FadeOut() {
        StartCoroutine(FadeOutLight());
    }

    private IEnumerator FadeOutLight() {
        float timeElapsed = 0;
        Light2D light = _light.GetComponent<Light2D>();
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
}
