using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class PlayForestWaterfallSfx : MonoBehaviour
{
    [SerializeField] private EventReference _sfx;
    [SerializeField] private float _fadeInDuration = 1f;
    [SerializeField] private float _fadeOutDuration = 1f;

    private EventInstance _eventInstance;
    private Coroutine _fadeCoroutine;
    private float _currentFadeValue = 0f;

    void Start()
    {
        StartAmbience();
    }

    void OnDestroy() {
        if(_fadeCoroutine != null) {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
        
        if(_eventInstance.isValid()) {
            _eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _eventInstance.release();
        }
    }

    public void StartAmbience() {
        if(!_eventInstance.isValid()) {
            _eventInstance = SoundFXManager.obj.CreateAttachedInstance(_sfx, gameObject);
            _eventInstance.start();
            _currentFadeValue = 0f;
            _eventInstance.setParameterByName("fade", _currentFadeValue);
        }
        
        if(_fadeCoroutine != null) {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeToValue(1f, _fadeInDuration));
    }

    public void StopAmbience() {
        if(!_eventInstance.isValid()) {
            return;
        }
        
        if(_fadeCoroutine != null) {
            StopCoroutine(_fadeCoroutine);
        }
        _fadeCoroutine = StartCoroutine(FadeToValue(0f, _fadeOutDuration));
    }
    
    private System.Collections.IEnumerator FadeToValue(float targetValue, float duration) {
        float startValue = _currentFadeValue;
        float elapsed = 0f;
        
        while(elapsed < duration) {
            elapsed += Time.deltaTime;
            _currentFadeValue = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            _eventInstance.setParameterByName("fade", _currentFadeValue);
            yield return null;
        }
        
        _currentFadeValue = targetValue;
        _eventInstance.setParameterByName("fade", _currentFadeValue);
        _fadeCoroutine = null;
        
        if(_currentFadeValue == 0f) {
            _eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _eventInstance.release();
        }
    }
}
