using FunkyCode;
using UnityEngine;

public class LightSprite2DOverlayPulser2 : MonoBehaviour
{
    [SerializeField] private float _minAlpha = 0.5f;
    [SerializeField] private float _maxAlpha = 1f;
    [SerializeField] private float _increasePulseSpeed = 0.3f;
    [SerializeField] private float _decreasePulseSpeed = 0.6f;
    [SerializeField] private float _pauseAtBoundariesDuration = 0.08f;
    [SerializeField] private float _deactivateReturnSpeed = 0.25f;

    private LightSprite2D _lightSource;

    private bool _isActive = false;

    private float _elapsedTime = 0f;
    private float _pauseTimer = 0f;
    private float _currentAlpha = 1f;
    private float _initialAlpha = 1f;

    private enum PulsatorState {
        Idle,
        Increasing,
        PauseAtMin,
        Decreasing,
        PauseAtMax,
        ReturningToInitial
    }

    private PulsatorState _state = PulsatorState.Idle;

    private void Awake() {
        _lightSource = GetComponent<LightSprite2D>();
        _initialAlpha = _lightSource.meshMode.alpha;
        _currentAlpha = _initialAlpha;
        Activate();
    }

    public void Activate() {
        _isActive = true;

        if (_state == PulsatorState.Idle || _state == PulsatorState.ReturningToInitial) {
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
            _state = PulsatorState.ReturningToInitial;
        }
    }

    private void Update() {
        const float epsilon = 0.0005f;

        if (_state == PulsatorState.Idle) {
            return;
        }

        if (!_isActive && _state != PulsatorState.ReturningToInitial) {
            _elapsedTime = 0f;
            _pauseTimer = 0f;
            _state = PulsatorState.ReturningToInitial;
        }

        if (_state == PulsatorState.PauseAtMin || _state == PulsatorState.PauseAtMax) {
            _pauseTimer += Time.deltaTime;
            if (_pauseTimer >= _pauseAtBoundariesDuration) {
                _pauseTimer = 0f;
                _elapsedTime = 0f;
                _state = _state == PulsatorState.PauseAtMin ? PulsatorState.Decreasing : PulsatorState.Increasing;
            }
            return;
        }

        if (_state == PulsatorState.Increasing) {
            _elapsedTime += Time.deltaTime;
            float t = _increasePulseSpeed <= 0f ? 1f : Mathf.Clamp01(_elapsedTime / _increasePulseSpeed);
            _currentAlpha = Mathf.Lerp(_maxAlpha, _minAlpha, t);
            _lightSource.meshMode.alpha = _currentAlpha;

            if (Mathf.Abs(_currentAlpha - _minAlpha) <= epsilon || t >= 1f) {
                _currentAlpha = _minAlpha;
                _lightSource.meshMode.alpha = _currentAlpha;
                _elapsedTime = 0f;
                _pauseTimer = 0f;
                _state = PulsatorState.PauseAtMin;
            }

            return;
        }

        if (_state == PulsatorState.Decreasing) {
            _elapsedTime += Time.deltaTime;
            float t = _decreasePulseSpeed <= 0f ? 1f : Mathf.Clamp01(_elapsedTime / _decreasePulseSpeed);
            _currentAlpha = Mathf.Lerp(_minAlpha, _maxAlpha, t);
            _lightSource.meshMode.alpha = _currentAlpha;

            if (Mathf.Abs(_currentAlpha - _maxAlpha) <= epsilon || t >= 1f) {
                _currentAlpha = _maxAlpha;
                _lightSource.meshMode.alpha = _currentAlpha;
                _elapsedTime = 0f;
                _pauseTimer = 0f;
                _state = PulsatorState.PauseAtMax;
            }

            return;
        }

        if (_state == PulsatorState.ReturningToInitial) {
            _elapsedTime += Time.deltaTime;
            float t = _deactivateReturnSpeed <= 0f ? 1f : Mathf.Clamp01(_elapsedTime / _deactivateReturnSpeed);
            _currentAlpha = Mathf.Lerp(_currentAlpha, _initialAlpha, t);
            _lightSource.meshMode.alpha = _currentAlpha;

            if (Mathf.Abs(_currentAlpha - _initialAlpha) <= epsilon || t >= 1f) {
                _currentAlpha = _initialAlpha;
                _lightSource.meshMode.alpha = _initialAlpha;
                _state = PulsatorState.Idle;
            }
        }
    }
}
