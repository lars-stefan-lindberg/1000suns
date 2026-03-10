using FunkyCode;
using UnityEngine;

public class StraightCeilingLightBeam : MonoBehaviour
{
    [SerializeField] private LightSprite2D _bigLight;
    [SerializeField] private LightSprite2D _smallLight1;
    [SerializeField] private LightSprite2D _smallLight2;
    [SerializeField] private LightSprite2D _smallLight3;
    [SerializeField] private float _bighLightMaxAlpha = 0.5f;
    [SerializeField] private float _bighLightMinAlpha = 0.3f;
    [SerializeField] private float _bighLightMaxThickness = 2.5f;
    [SerializeField] private float _bighLightMinThickness = 2f;
    [SerializeField] private float _playerMinDistance = 10f;
    
    [Header("Small Lights Animation")]
    [SerializeField] private bool _smallLight1Active = true;
    [SerializeField] private bool _smallLight2Active = true;
    [SerializeField] private bool _smallLight3Active = true;
    [SerializeField] private float _smallLightMinAlpha = 0.1f;
    [SerializeField] private float _smallLightMaxAlpha = 0.5f;
    [SerializeField] private float _smallLightAlphaIncreaseSpeed = 0.5f;
    [SerializeField] private float _smallLightAlphaDecreaseSpeed = 0.5f;
    [SerializeField] private float _smallLightMinHoldTime = 0.5f;
    [SerializeField] private float _smallLightMaxHoldTime = 2f;
    [SerializeField] private float _smallLight2StartOffset = 0.5f;
    [SerializeField] private float _smallLight3StartOffset = 1f;
    
    private enum SmallLightState { OnMinimum, Increasing, OnMaximum, Decreasing }
    
    private SmallLightState _smallLight1State = SmallLightState.OnMinimum;
    private SmallLightState _smallLight2State = SmallLightState.OnMinimum;
    private SmallLightState _smallLight3State = SmallLightState.OnMinimum;
    private float _smallLight1HoldTimer = 0f;
    private float _smallLight2HoldTimer = 0f;
    private float _smallLight3HoldTimer = 0f;
    private bool _smallLight2Initialized = false;
    private bool _smallLight3Initialized = false;

    void Start() {
        if (_smallLight1Active && _smallLight1 != null) {
            InitializeSmallLight(_smallLight1, ref _smallLight1State, ref _smallLight1HoldTimer);
        }
    }

    void Update() {
        UpdateLightBasedOnPlayerDistance();
        
        if (_smallLight1Active && _smallLight1 != null) {
            UpdateSmallLight(_smallLight1, ref _smallLight1State, ref _smallLight1HoldTimer);
        }
        
        if (_smallLight2Active && _smallLight2 != null) {
            if (!_smallLight2Initialized && Time.time >= _smallLight2StartOffset) {
                InitializeSmallLight(_smallLight2, ref _smallLight2State, ref _smallLight2HoldTimer);
                _smallLight2Initialized = true;
            }
            
            if (_smallLight2Initialized) {
                UpdateSmallLight(_smallLight2, ref _smallLight2State, ref _smallLight2HoldTimer);
            }
        }
        
        if (_smallLight3Active && _smallLight3 != null) {
            if (!_smallLight3Initialized && Time.time >= _smallLight3StartOffset) {
                InitializeSmallLight(_smallLight3, ref _smallLight3State, ref _smallLight3HoldTimer);
                _smallLight3Initialized = true;
            }
            
            if (_smallLight3Initialized) {
                UpdateSmallLight(_smallLight3, ref _smallLight3State, ref _smallLight3HoldTimer);
            }
        }
    }

    private void UpdateLightBasedOnPlayerDistance() {
        var playerTransform = PlayerManager.obj.GetPlayerTransform();
        if (playerTransform == null) return;

        float distance = Mathf.Abs(playerTransform.position.x - transform.position.x);
        
        if (distance < _playerMinDistance) {
            float normalizedDistance = distance / _playerMinDistance;
            float growthFactor = 1f - normalizedDistance;
            
            float targetAlpha = Mathf.Lerp(_bighLightMinAlpha, _bighLightMaxAlpha, growthFactor);
            float targetThickness = Mathf.Lerp(_bighLightMinThickness, _bighLightMaxThickness, growthFactor);
            
            Color color = _bigLight.color;
            color.a = targetAlpha;
            _bigLight.color = color;
            
            _bigLight.lightSpriteTransform.scale = new Vector2(targetThickness, _bigLight.lightSpriteTransform.scale.y);
        } else {
            Color color = _bigLight.color;
            color.a = _bighLightMinAlpha;
            _bigLight.color = color;
            
            _bigLight.lightSpriteTransform.scale = new Vector2(_bighLightMinThickness, _bigLight.lightSpriteTransform.scale.y);
        }
    }

    private void InitializeSmallLight(LightSprite2D light, ref SmallLightState state, ref float holdTimer) {
        Color color = light.color;
        color.a = _smallLightMinAlpha;
        light.color = color;
        
        state = SmallLightState.OnMinimum;
        holdTimer = Random.Range(_smallLightMinHoldTime, _smallLightMaxHoldTime);
    }

    private void UpdateSmallLight(LightSprite2D light, ref SmallLightState state, ref float holdTimer) {
        Color color = light.color;
        
        switch (state) {
            case SmallLightState.OnMinimum:
                holdTimer -= Time.deltaTime;
                if (holdTimer <= 0f) {
                    state = SmallLightState.Increasing;
                }
                break;
                
            case SmallLightState.Increasing:
                color.a += _smallLightAlphaIncreaseSpeed * Time.deltaTime;
                if (color.a >= _smallLightMaxAlpha) {
                    color.a = _smallLightMaxAlpha;
                    state = SmallLightState.OnMaximum;
                    holdTimer = Random.Range(_smallLightMinHoldTime, _smallLightMaxHoldTime);
                }
                light.color = color;
                break;
                
            case SmallLightState.OnMaximum:
                holdTimer -= Time.deltaTime;
                if (holdTimer <= 0f) {
                    state = SmallLightState.Decreasing;
                }
                break;
                
            case SmallLightState.Decreasing:
                color.a -= _smallLightAlphaDecreaseSpeed * Time.deltaTime;
                if (color.a <= _smallLightMinAlpha) {
                    color.a = _smallLightMinAlpha;
                    state = SmallLightState.OnMinimum;
                    holdTimer = Random.Range(_smallLightMinHoldTime, _smallLightMaxHoldTime);
                }
                light.color = color;
                break;
        }
    }
}
