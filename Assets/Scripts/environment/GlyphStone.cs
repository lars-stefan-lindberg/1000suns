using FunkyCode;
using UnityEngine;

public class GlyphStone : MonoBehaviour
{
    private LightSprite2D _lightSprite;
    private SpritePulsator _spritePulsator;

    [SerializeField] private float _activatedScaleMultiplier = 3f;
    [SerializeField] private float _scaleUpDuration = 0.25f;
    [SerializeField] private float _scaleDownDuration = 0.25f;

    private Vector3 _originalLightSpriteScale;
    private bool _hasCachedOriginalScale;
    private Coroutine _lightSpriteScaleCoroutine;

    void Awake() {
        _lightSprite = GetComponentInChildren<LightSprite2D>();
        _spritePulsator = GetComponentInChildren<SpritePulsator>();

        CacheOriginalLightSpriteScaleIfNeeded();
    }

    public void Activate() {
        _spritePulsator.Activate();

        CacheOriginalLightSpriteScaleIfNeeded();
        StartLightSpriteScaleTransition(GetActivatedScale(), _scaleUpDuration);
    }

    public void Deactivate() {
        _spritePulsator.Deactivate();

        CacheOriginalLightSpriteScaleIfNeeded();
        StartLightSpriteScaleTransition(_originalLightSpriteScale, _scaleDownDuration);
    }

    private void CacheOriginalLightSpriteScaleIfNeeded() {
        if (_hasCachedOriginalScale) {
            return;
        }

        if (_lightSprite == null) {
            return;
        }

        _originalLightSpriteScale = _lightSprite.transform.localScale;
        _hasCachedOriginalScale = true;
    }

    private Vector3 GetActivatedScale() {
        return new Vector3(
            _originalLightSpriteScale.x * _activatedScaleMultiplier,
            _originalLightSpriteScale.y * _activatedScaleMultiplier,
            _originalLightSpriteScale.z
        );
    }

    private void StartLightSpriteScaleTransition(Vector3 targetScale, float duration) {
        if (_lightSprite == null) {
            return;
        }

        if (_lightSpriteScaleCoroutine != null) {
            StopCoroutine(_lightSpriteScaleCoroutine);
            _lightSpriteScaleCoroutine = null;
        }

        _lightSpriteScaleCoroutine = StartCoroutine(LightSpriteScaleCoroutine(targetScale, duration));
    }

    private System.Collections.IEnumerator LightSpriteScaleCoroutine(Vector3 targetScale, float duration) {
        Transform t = _lightSprite.transform;

        Vector3 startScale = t.localScale;
        float elapsed = 0f;

        if (duration <= 0f) {
            t.localScale = targetScale;
            _lightSpriteScaleCoroutine = null;
            yield break;
        }

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float lerpT = Mathf.Clamp01(elapsed / duration);
            t.localScale = Vector3.Lerp(startScale, targetScale, lerpT);
            yield return null;
        }

        t.localScale = targetScale;
        _lightSpriteScaleCoroutine = null;
    }
}
