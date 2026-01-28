using UnityEngine;

public class SpriteFlash : MonoBehaviour
{
[SerializeField] private float _flashIntensity = 0.25f;
    [SerializeField] private float _increaseFlashSpeed = 0.3f;
    [SerializeField] private float _decreaseFlashSpeed = 0.6f;

    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private bool _startFlashing = false;
    private bool _increaseFlash = false;
    private bool _decreaseFlash = false;
    private float _elapsedTime = 0f; //Elapsed time between a blend and non-blend
    private float _currentFlashIntensity = 0f;
    private bool _isFlashing = false;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
    }

    public void Flash() {
        _startFlashing = true;
    }

    public void AbortFlash() {
        if(_isFlashing) {
            _startFlashing = false;
            _isFlashing = false;
            _increaseFlash = false;
            _decreaseFlash = false; ;
            _material.SetFloat("_Contrast", 1f);
        }
    }

    void Update() {
        if(_startFlashing) {
            _startFlashing = false;
            _increaseFlash = true;
            _isFlashing = true;
        }

        if(_isFlashing) {
            if (_increaseFlash) {
                if(_currentFlashIntensity == 1f) {
                    _elapsedTime = 0f;
                }
                
                _elapsedTime += Time.deltaTime;

                _currentFlashIntensity = Mathf.Lerp(1f, _flashIntensity, _elapsedTime / _increaseFlashSpeed);

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
                    _currentFlashIntensity = Mathf.Lerp(_flashIntensity, 1f, _elapsedTime / _decreaseFlashSpeed);
                    _material.SetFloat("_Contrast", _currentFlashIntensity);
                } else {
                    _decreaseFlash = false;
                    _increaseFlash = true;
                    _isFlashing = false;
                }
            }
        }
    }
}
