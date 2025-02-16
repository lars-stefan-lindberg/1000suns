using UnityEngine;

public class PlayerFlash : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] private float _flashIntensity = 0.5f;
    [SerializeField] private float _defaultFlashSpeed = 0.27f;
    private float _flashSpeed;

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

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
        _flashSpeed = _defaultFlashSpeed;
    }

    public void FlashOnce() {
        _startFlashing = true;
        _flashDuration = 0.1f;
        _flashSpeed = _defaultFlashSpeed;
    }

    public void FlashFor(float duration, float flashSpeed) {
        _startFlashing = true;
        _flashDuration = duration;
        _flashSpeed = flashSpeed;
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
                if(_currentFlashIntensity == 0f) {
                    _elapsedTime = 0f;
                }
                
                _elapsedTime += Time.deltaTime;

                _currentFlashIntensity = Mathf.Lerp(0f, _flashIntensity, _elapsedTime / _flashSpeed);

                _material.SetFloat("_FlashAmount", _currentFlashIntensity);

                if(_currentFlashIntensity == _flashIntensity) {
                    _increaseFlash = false;
                    _decreaseFlash = true;
                    _elapsedTime = 0;
                }
            } 
            else if(_decreaseFlash) {
                if(_currentFlashIntensity > 0f) {
                    _elapsedTime += Time.deltaTime;
                    _currentFlashIntensity = Mathf.Lerp(_flashIntensity, 0f, _elapsedTime / _flashSpeed);
                    _material.SetFloat("_FlashAmount", _currentFlashIntensity);
                } else {
                    _decreaseFlash = false;
                    _increaseFlash = true;
                    _isFlashing = false;
                }
            }
        }
    }
}
