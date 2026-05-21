using System.Collections;
using FunkyCode;
using UnityEngine;

public class LightFlash : MonoBehaviour
{
    [SerializeField] private LightSprite2D _light;
    [SerializeField] private float _maxScaleX = 1f;
    [SerializeField] private float _maxScaleY = 1f;
    [SerializeField] private float _scaleUpDuration = 0.1f;
    [SerializeField] private float _scaleDownDuration = 0.1f;

    private Coroutine _flashCoroutine;

    [ContextMenu("Flash")]
    public void Flash() {
        if (_flashCoroutine != null) {
            StopCoroutine(_flashCoroutine);
        }
        _flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine() {
        _light.enabled = true;
        _light.lightSpriteTransform.scale = Vector2.zero;

        float elapsed = 0f;
        while (elapsed < _scaleUpDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / _scaleUpDuration;
            _light.lightSpriteTransform.scale = new Vector2(
                Mathf.Lerp(0f, _maxScaleX, t),
                Mathf.Lerp(0f, _maxScaleY, t)
            );
            yield return null;
        }

        _light.lightSpriteTransform.scale = new Vector2(_maxScaleX, _maxScaleY);

        // elapsed = 0f;
        // while (elapsed < _scaleDownDuration) {
        //     elapsed += Time.deltaTime;
        //     float t = elapsed / _scaleDownDuration;
        //     _light.lightSpriteTransform.scale = new Vector2(
        //         Mathf.Lerp(_maxScaleX, 0f, t),
        //         Mathf.Lerp(_maxScaleY, 0f, t)
        //     );
        //     yield return null;
        // }

        _light.enabled = false;
        _flashCoroutine = null;
    }
}
