using UnityEngine;

public class PlayerChargeFlash : MonoBehaviour
{
    [SerializeField] private float _startFlashSpeed = 0.27f;
    [SerializeField] private float _endFlashSpeed = 0.15f;
    private float _flashSpeed = 0.27f;
    [SerializeField] private float _flashIntensity = 0.5f;
    [SerializeField] private float _timeToFullyChargeSpeed = 0.2f;
    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private bool _startFlashing = false;
    private bool _endFlashing = false;
    private bool _increaseFlash = false;
    private bool _decreaseFlash = false;
    private float _elapsedTimeBetweenBlendAndNonBlend = 0f;
    private float _currentFlashIntensity = 0f;

    public void StartFlashing() {
        _startFlashing = true;
        _increaseFlash = true;
        _flashSpeed = _startFlashSpeed;
    }

    public void EndFlashing() {
        _endFlashing = true;
    }

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
    }

    void Update()
    {
        if(_endFlashing) {
            _startFlashing = false;
            _currentFlashIntensity = 0;
            _material.SetFloat("_FlashAmount", 0);
            _endFlashing = false;
        }
        if(_startFlashing) {
            _flashSpeed = Mathf.Lerp(_flashSpeed, _endFlashSpeed, Time.deltaTime * _timeToFullyChargeSpeed);
            if (_increaseFlash) {
                if(_currentFlashIntensity == 0f) {
                    _elapsedTimeBetweenBlendAndNonBlend = 0f;
                }
                
                _elapsedTimeBetweenBlendAndNonBlend += Time.deltaTime;

                _currentFlashIntensity = Mathf.Lerp(0f, _flashIntensity, _elapsedTimeBetweenBlendAndNonBlend / _flashSpeed);

                _material.SetFloat("_FlashAmount", _currentFlashIntensity);

                if(_currentFlashIntensity == _flashIntensity) {
                    _increaseFlash = false;
                    _decreaseFlash = true;
                    _elapsedTimeBetweenBlendAndNonBlend = 0;
                }
            } 
            else if(_decreaseFlash) {
                if(_currentFlashIntensity > 0f) {
                    _elapsedTimeBetweenBlendAndNonBlend += Time.deltaTime;
                    _currentFlashIntensity = Mathf.Lerp(_flashIntensity, 0f, _elapsedTimeBetweenBlendAndNonBlend / _flashSpeed);
                    _material.SetFloat("_FlashAmount", _currentFlashIntensity);
                } else {
                    _decreaseFlash = false;
                    _increaseFlash = true;
                }
            }
        }
    }
}
