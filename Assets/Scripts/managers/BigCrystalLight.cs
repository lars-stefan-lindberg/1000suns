using UnityEngine;
using FunkyCode;

public class BigCrystalLight : MonoBehaviour
{
    [SerializeField] private LightSprite2D _bigLight;
    [SerializeField] private float _bighLightMaxAlpha = 0.5f;
    [SerializeField] private float _bighLightMinAlpha = 0.3f;
    [SerializeField] private float _bighLightMaxSize = 2.5f;
    [SerializeField] private float _bighLightMinSize = 2f;
    [SerializeField] private float _playerMinDistance = 10f;

    void Update() {
        UpdateLightBasedOnPlayerDistance();
    }

    private void UpdateLightBasedOnPlayerDistance() {
        var playerTransform = PlayerManager.obj.GetPlayerTransform();
        if (playerTransform == null) return;

        float distance = Mathf.Abs(playerTransform.position.x - transform.position.x);
        
        if (distance < _playerMinDistance) {
            float normalizedDistance = distance / _playerMinDistance;
            float growthFactor = 1f - normalizedDistance;
            
            float targetAlpha = Mathf.Lerp(_bighLightMinAlpha, _bighLightMaxAlpha, growthFactor);
            float targetSize = Mathf.Lerp(_bighLightMinSize, _bighLightMaxSize, growthFactor);
            
            Color color = _bigLight.color;
            color.a = targetAlpha;
            _bigLight.color = color;
            
            _bigLight.lightSpriteTransform.scale = new Vector2(targetSize, targetSize);
        } else {
            Color color = _bigLight.color;
            color.a = _bighLightMinAlpha;
            _bigLight.color = color;
            
            _bigLight.lightSpriteTransform.scale = new Vector2(_bighLightMinSize, _bighLightMinSize);
        }
    }
}
