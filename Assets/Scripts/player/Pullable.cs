using UnityEngine;
using DG.Tweening;

public class Pullable : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private float _pullForce = 1f;
    [SerializeField] private bool _isHeavy = false;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private float _outlineFadeSpeed = 0.5f;
    [SerializeField] private float _glowFadeSpeed = 0.5f;
    
    [Header("Grab Shake")]
    [SerializeField] private float _shakeDistance = 0.1f;
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private int _shakeVibrato = 10;
    
    public bool IsPulled {get; set;}
    
    private Material _material;
    private Transform _spriteTransform;
    private Vector3 _originalSpritePosition;
    
    private bool _isOutlineFadingIn = false;
    private bool _isOutlineFadingOut = false;
    private float _currentOutlineFade = 0f;
    private float _outlineElapsedTime = 0f;
    
    private bool _isGlowFadingIn = false;
    private bool _isGlowFadingOut = false;
    private float _currentGlowFade = 0f;
    private float _glowElapsedTime = 0f;

    private void Awake() {
        IsPulled = false;
        _material = _renderer.material;
        _material.SetFloat("_SineGlowFade", 0f);
        _material.SetFloat("_PixelOutlineFade", 0f);
        _spriteTransform = _renderer.transform;
        _originalSpritePosition = _spriteTransform.localPosition;
    }

    public Rigidbody2D GetRigidbody() {
        return _rigidBody;
    }

    public float GetPullForce() {
        return _pullForce;
    }

    public bool IsHeavy() {
        return _isHeavy;
    }
    
    public void StartHighlight() {
        _isOutlineFadingIn = true;
        _isOutlineFadingOut = false;
        // Start from current fade value to avoid jumps
        _outlineElapsedTime = _currentOutlineFade * _outlineFadeSpeed;
    }
    
    public void StopHighlight() {
        _isOutlineFadingIn = false;
        _isOutlineFadingOut = true;
        // Start from current fade value to avoid jumps
        _outlineElapsedTime = (1f - _currentOutlineFade) * _outlineFadeSpeed;
    }
    
    public void StartGrabbed() {
        StopHighlight();
        _isGlowFadingIn = true;
        _isGlowFadingOut = false;
        // Start from current fade value to avoid jumps
        _glowElapsedTime = _currentGlowFade * _glowFadeSpeed;
        TriggerGrabShake();
    }
    
    public void StopGrabbed() {
        _isGlowFadingIn = false;
        _isGlowFadingOut = true;
        // Start from current fade value to avoid jumps
        _glowElapsedTime = (1f - _currentGlowFade) * _glowFadeSpeed;
    }
    
    private void TriggerGrabShake() {
        _spriteTransform.DOKill();
        _spriteTransform.localPosition = _originalSpritePosition;
        _spriteTransform.DOShakePosition(_shakeDuration, new Vector3(_shakeDistance, 0f, 0f), _shakeVibrato, 0, false, false)
            .OnComplete(() => _spriteTransform.localPosition = _originalSpritePosition);
    }
    
    private void Update() {
        if (_isOutlineFadingIn || _isOutlineFadingOut)
            UpdateOutlineFade();
        
        if (_isGlowFadingIn || _isGlowFadingOut)
            UpdateGlowFade();
    }
    
    private void UpdateOutlineFade() {
        if (_isOutlineFadingIn) {
            _outlineElapsedTime += Time.deltaTime;
            _currentOutlineFade = Mathf.Lerp(0f, 1f, _outlineElapsedTime / _outlineFadeSpeed);
            _material.SetFloat("_PixelOutlineFade", _currentOutlineFade);
            
            if (_currentOutlineFade >= 1f) {
                _isOutlineFadingIn = false;
                _currentOutlineFade = 1f;
            }
        }
        else if (_isOutlineFadingOut) {
            _outlineElapsedTime += Time.deltaTime;
            _currentOutlineFade = Mathf.Lerp(1f, 0f, _outlineElapsedTime / _outlineFadeSpeed);
            _material.SetFloat("_PixelOutlineFade", _currentOutlineFade);
            
            if (_currentOutlineFade <= 0f) {
                _isOutlineFadingOut = false;
                _currentOutlineFade = 0f;
            }
        }
    }
    
    private void UpdateGlowFade() {
        if (_isGlowFadingIn) {
            _glowElapsedTime += Time.deltaTime;
            _currentGlowFade = Mathf.Lerp(0f, 1f, _glowElapsedTime / _glowFadeSpeed);
            _material.SetFloat("_SineGlowFade", _currentGlowFade);
            
            if (_currentGlowFade >= 1f) {
                _isGlowFadingIn = false;
                _currentGlowFade = 1f;
            }
        }
        else if (_isGlowFadingOut) {
            _glowElapsedTime += Time.deltaTime;
            _currentGlowFade = Mathf.Lerp(1f, 0f, _glowElapsedTime / _glowFadeSpeed);
            _material.SetFloat("_SineGlowFade", _currentGlowFade);
            
            if (_currentGlowFade <= 0f) {
                _isGlowFadingOut = false;
                _currentGlowFade = 0f;
            }
        }
    }
}
