using UnityEngine;

public class SpritePulsator : MonoBehaviour
{
    [SerializeField] private float _minContrast = 0f;
    [SerializeField] private float _maxContrast = 1f;
    [SerializeField] private float _increasePulseSpeed = 0.3f;
    [SerializeField] private float _decreasePulseSpeed = 0.6f;
    [SerializeField] private float _pauseAtBoundariesDuration = 0.08f;
    [SerializeField] private float _deactivateReturnSpeed = 0.25f;

    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private bool _isActive = false;

    private float _elapsedTime = 0f;
    private float _pauseTimer = 0f;
    private float _currentContrast = 1f;
    private float _initialContrast = 1f;

    private enum PulsatorState {
        Idle,
        Increasing,
        PauseAtIntensity,
        Decreasing,
        PauseAtOne,
        ReturningToOne
    }

    private PulsatorState _state = PulsatorState.Idle;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
        _initialContrast = _material.GetFloat("_Contrast");
        _currentContrast = _initialContrast;
    }

    public void Activate() {
        _isActive = true;

        if (_state == PulsatorState.Idle || _state == PulsatorState.ReturningToOne) {
            _elapsedTime = 0f;
            _pauseTimer = 0f;
            _state = PulsatorState.Increasing;
        }
    }

    public void Deactivate() {
        _isActive = false;

        if (_state != PulsatorState.Idle) {
            _elapsedTime = 0f;
            _pauseTimer = 0f;
            _state = PulsatorState.ReturningToOne;
        }
    }

    private void Update() {
        const float epsilon = 0.0005f;

        if (_state == PulsatorState.Idle) {
            return;
        }

        if (!_isActive && _state != PulsatorState.ReturningToOne) {
            _elapsedTime = 0f;
            _pauseTimer = 0f;
            _state = PulsatorState.ReturningToOne;
        }

        if (_state == PulsatorState.PauseAtIntensity || _state == PulsatorState.PauseAtOne) {
            _pauseTimer += Time.deltaTime;
            if (_pauseTimer >= _pauseAtBoundariesDuration) {
                _pauseTimer = 0f;
                _elapsedTime = 0f;
                _state = _state == PulsatorState.PauseAtIntensity ? PulsatorState.Decreasing : PulsatorState.Increasing;
            }
            return;
        }

        if (_state == PulsatorState.Increasing) {
            _elapsedTime += Time.deltaTime;
            float t = _increasePulseSpeed <= 0f ? 1f : Mathf.Clamp01(_elapsedTime / _increasePulseSpeed);
            _currentContrast = Mathf.Lerp(_maxContrast, _minContrast, t);
            _material.SetFloat("_Contrast", _currentContrast);

            if (Mathf.Abs(_currentContrast - _minContrast) <= epsilon || t >= 1f) {
                _currentContrast = _minContrast;
                _material.SetFloat("_Contrast", _currentContrast);
                _elapsedTime = 0f;
                _pauseTimer = 0f;
                _state = PulsatorState.PauseAtIntensity;
            }

            return;
        }

        if (_state == PulsatorState.Decreasing) {
            _elapsedTime += Time.deltaTime;
            float t = _decreasePulseSpeed <= 0f ? 1f : Mathf.Clamp01(_elapsedTime / _decreasePulseSpeed);
            _currentContrast = Mathf.Lerp(_minContrast, _maxContrast, t);
            _material.SetFloat("_Contrast", _currentContrast);

            if (Mathf.Abs(_currentContrast - _maxContrast) <= epsilon || t >= 1f) {
                _currentContrast = _maxContrast;
                _material.SetFloat("_Contrast", _currentContrast);
                _elapsedTime = 0f;
                _pauseTimer = 0f;
                _state = PulsatorState.PauseAtOne;
            }

            return;
        }

        if (_state == PulsatorState.ReturningToOne) {
            _elapsedTime += Time.deltaTime;
            float t = _deactivateReturnSpeed <= 0f ? 1f : Mathf.Clamp01(_elapsedTime / _deactivateReturnSpeed);
            _currentContrast = Mathf.Lerp(_currentContrast, _initialContrast, t);
            _material.SetFloat("_Contrast", _currentContrast);

            if (Mathf.Abs(_currentContrast - _initialContrast) <= epsilon || t >= 1f) {
                _currentContrast = _initialContrast;
                _material.SetFloat("_Contrast", _initialContrast);
                _state = PulsatorState.Idle;
            }
        }
    }
}
