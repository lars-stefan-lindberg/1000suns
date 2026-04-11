using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
using FunkyCode;
using UnityEngine;

public class ThunderLight : MonoBehaviour
{
    [SerializeField] private LightSprite2D _light;
    public EventReference _lightningSfx;

    [Header("Flash Settings")]
    [SerializeField] private float _firstFlashFadeSpeed = 2f;
    [SerializeField] private float _intermediateAlpha = 0.6f;
    [SerializeField] private float _delayBetweenFlashes = 0.1f;
    [SerializeField] private float _finalFadeOutSpeed = 1f;

    private Transform _playerTransform;
    private Coroutine _flashCoroutine;
    private EventInstance _thunderSfxInstance;

    void Awake() {
        _playerTransform = Player.obj.transform;
    }

    void FixedUpdate()
    {
        transform.position = new Vector2(_playerTransform.position.x, transform.position.y);
    }

    [ContextMenu("Flash")]
    public void Flash() {
        _thunderSfxInstance = SoundFXManager.obj.CreateAttachedInstance(_lightningSfx, gameObject, null);
        _thunderSfxInstance.start();
        _thunderSfxInstance.release();

        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 0.5f);
        
        if (_flashCoroutine != null) {
            StopCoroutine(_flashCoroutine);
        }
        
        _flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    public void Stop()
    {
        AudioUtils.SafeStop(ref _thunderSfxInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        CameraShakeManager.obj.ShakeCamera(0, 0, 0);
        if(_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);
        _light.enabled = false;
        _light.color = new Color(_light.color.r, _light.color.g, _light.color.b, 0f);
    }

    private IEnumerator FlashCoroutine() {
        _light.enabled = true;
        
        // First flash: Quick fade from 0 to 1
        Color lightColor = _light.color;
        lightColor.a = 0f;
        _light.color = lightColor;
        
        float elapsed = 0f;
        while (elapsed < 0.05f) {
            elapsed += Time.deltaTime;
            lightColor.a = Mathf.Lerp(0f, 1f, elapsed / 0.05f);
            _light.color = lightColor;
            yield return null;
        }
        
        lightColor.a = 1f;
        _light.color = lightColor;
        
        // Fade to intermediate alpha
        elapsed = 0f;
        float fadeDuration = 1f / _firstFlashFadeSpeed;
        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            lightColor.a = Mathf.Lerp(1f, _intermediateAlpha, elapsed / fadeDuration);
            _light.color = lightColor;
            yield return null;
        }
        
        lightColor.a = _intermediateAlpha;
        _light.color = lightColor;
        
        // Brief delay between flashes
        yield return new WaitForSeconds(_delayBetweenFlashes);
        
        // Second flash: Quick fade to 1
        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 2f);
        elapsed = 0f;
        while (elapsed < 0.05f) {
            elapsed += Time.deltaTime;
            lightColor.a = Mathf.Lerp(_intermediateAlpha, 1f, elapsed / 0.05f);
            _light.color = lightColor;
            yield return null;
        }
        
        lightColor.a = 1f;
        _light.color = lightColor;
        
        // Final fade out to 0
        elapsed = 0f;
        fadeDuration = 1f / _finalFadeOutSpeed;
        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            lightColor.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            _light.color = lightColor;
            yield return null;
        }
        
        lightColor.a = 0f;
        _light.color = lightColor;
        _light.enabled = false;
        
        _flashCoroutine = null;
    }
}
