using UnityEngine;

public class PlayerFlash : MonoBehaviour
{
    [SerializeField] private float _flashIntensity = 0.25f;
    [SerializeField] private float _defaultFlashSpeed = 0.27f;
    [SerializeField] private float _chargeFlashIncreaseSpeed = 0.6f;
    [SerializeField] private float _chargeFlashDecreaseSpeed = 0.1f;
    private float _flashIncreaseSpeed;
    private float _flashDecreaseSpeed;

    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private bool _startFlashing = false;
    private bool _increaseFlash = false;
    private bool _decreaseFlash = false;
    private float _elapsedTime = 0f; //Elapsed time between a blend and non-blend
    private float _totalElapsedTime = 0f;
    private float _flashDuration = 0f;
    private float _currentFlashIntensity = 0f;
    private bool _isFlashing = false;
    private bool _isFullyCharged = false;
    private bool _turnOnFullyChargedVfx = false;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
        _flashIncreaseSpeed = _defaultFlashSpeed;
        _flashDecreaseSpeed = _defaultFlashSpeed;
    }

    public void FlashOnce() {
        _startFlashing = true;
        _flashDuration = 0.1f;
        _flashIncreaseSpeed = _defaultFlashSpeed;
        _flashDecreaseSpeed = _defaultFlashSpeed;
    }

    public void FlashFor(float duration, float flashSpeed) {
        _startFlashing = true;
        _flashDuration = duration;
        _flashIncreaseSpeed = flashSpeed;
        _flashDecreaseSpeed = flashSpeed;
    }

    public void ChargeFlash() {
        _startFlashing = true;
        _flashDuration = 0.1f;
        _flashIncreaseSpeed = _chargeFlashIncreaseSpeed;
        _flashDecreaseSpeed = _chargeFlashDecreaseSpeed;
    }

    public void AbortFlash() {
        if(_isFlashing) {
            _increaseFlash = false;
            _decreaseFlash = true;
            _flashDecreaseSpeed = 0.1f;
        }
    }

    public void StartFullyChargedVfx() {
        _isFullyCharged = true;
        _turnOnFullyChargedVfx = true;
    }
    public void EndFullyChargedVfx() {
        _isFullyCharged = false;
        _turnOnFullyChargedVfx = false;
        _material.SetFloat("_EnchantedFade", 0f);
    }

    void Update() {
        if(_startFlashing) {
            _totalElapsedTime = 0;
            _startFlashing = false;
            _increaseFlash = true;
        }

        if(!_isFlashing && _totalElapsedTime < _flashDuration)
            _isFlashing = true;

        if(_totalElapsedTime <= _flashDuration)
            _totalElapsedTime += Time.deltaTime;

        if(_isFlashing) {
            if (_increaseFlash) {
                if(_currentFlashIntensity == 1f) {
                    _elapsedTime = 0f;
                }
                
                _elapsedTime += Time.deltaTime;

                _currentFlashIntensity = Mathf.Lerp(1f, _flashIntensity, _elapsedTime / _flashIncreaseSpeed);

                //_material.SetFloat("_FlashAmount", _currentFlashIntensity);
                _material.SetFloat("_Contrast", _currentFlashIntensity);

                if(_currentFlashIntensity == _flashIntensity) {
                    _increaseFlash = false;
                    _decreaseFlash = true;
                    _elapsedTime = 0;
                }
            } 
            else if(_decreaseFlash) {
                if(_currentFlashIntensity < 1f) {
                    _elapsedTime += Time.deltaTime;
                    _currentFlashIntensity = Mathf.Lerp(_flashIntensity, 1f, _elapsedTime / _flashDecreaseSpeed);
                    _material.SetFloat("_Contrast", _currentFlashIntensity);
                } else {
                    _decreaseFlash = false;
                    _increaseFlash = true;
                    _isFlashing = false;
                }
            }
        }

        if(_isFullyCharged) {
           if(_turnOnFullyChargedVfx) {
            _material.SetFloat("_EnchantedFade", 1f);
            _turnOnFullyChargedVfx = false;
           } 
        }
    }
}
