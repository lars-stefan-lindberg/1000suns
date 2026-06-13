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
    [SerializeField] private bool _useSineGlowFade = false;
    [SerializeField] private ParticleSystem _heldParticles;
    // [SerializeField] private ParticleSystem _trailingParticles;
    //[SerializeField] private ParticleSystem _grabbedParticles;
    // [SerializeField] private int _grabbedNumberOfParticles = 20;
    // [SerializeField] private int _grabbedParticlesRadius = 1;
    // [SerializeField] private int _grabbedParticlesSpeed = 5;
    
    [Header("Grab Shake")]
    [SerializeField] private float _shakeDistance = 0.1f;
    [SerializeField] private float _shakeDuration = 0.2f;
    [SerializeField] private int _shakeVibrato = 10;
    
    [Header("Brightness Pulse")]
    [SerializeField] private float _maxBrightness = 2f;
    [SerializeField] private float _pulseFrequency = 1f;
    
    [Header("Held Particles")]
    [SerializeField] private float _velocityThreshold = 0.1f;
    
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
    
    private bool _isBrightnessPulsing = false;
    private float _currentBrightness = 1f;
    private float _brightnessElapsedTime = 0f;
    
    //private bool _ghostTrailArmed = false;
    private float _previousVelocityMagnitude = 0f;
    private bool _heldParticlesPlaying = false;

    private void Start() {
        IsPulled = false;
        _material = _renderer.material;
        _material.SetFloat("_SineGlowFade", 0f);
        _material.SetFloat("_PixelOutlineFade", 0f);
        _material.SetFloat("_Brightness", 1f);
        _spriteTransform = _renderer.transform;
        _originalSpritePosition = _spriteTransform.localPosition;
        
        if (_heldParticles != null) {
            _heldParticles.Stop();
        }
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
        if (_useSineGlowFade) {
            _isGlowFadingIn = true;
            _isGlowFadingOut = false;
            // Start from current fade value to avoid jumps
            _glowElapsedTime = _currentGlowFade * _glowFadeSpeed;
        } else {
            // Start brightness pulse
            _isBrightnessPulsing = true;
            _brightnessElapsedTime = 0f;
        }

        //VFX
        TriggerGrabShake();
        ShockWaveManager.obj.CallShockWave(transform.position, 0.2f, 0.05f, 0.15f);
        //EmitEvenCircle(_grabbedParticles, _grabbedNumberOfParticles, _grabbedParticlesRadius, _grabbedParticlesSpeed);
        // _trailingParticles.Play();
        
        // if (PullableGhostTrailManager.obj != null) {
        //     PullableGhostTrailManager.obj.SetupGhostTrail(_renderer, transform);
        // }
        // _ghostTrailArmed = true;
        
        if (_heldParticles != null) {
            _heldParticles.Play();
            _heldParticlesPlaying = true;
        }
    }

    // private void EmitEvenCircle(ParticleSystem ps, int count, float radius, float speed)
    // {
    //     var emitParams = new ParticleSystem.EmitParams();
    //     float angleStep = 360f / count;

    //     for (int i = 0; i < count; i++)
    //     {
    //         float angle = i * angleStep * Mathf.Deg2Rad;
    //         Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

    //         emitParams.position = ps.transform.position + direction * radius;
    //         emitParams.velocity = direction * speed;

    //         ps.Emit(emitParams, 1);
    //     }
    // }
    
    public void StopGrabbed() {
        if (_useSineGlowFade) {
            _isGlowFadingIn = false;
            _isGlowFadingOut = true;
            // Start from current fade value to avoid jumps
            _glowElapsedTime = (1f - _currentGlowFade) * _glowFadeSpeed;
        } else {
            _isBrightnessPulsing = false;
            _currentBrightness = 1f;
            _material.SetFloat("_Brightness", 1f);
        }
        
        //_ghostTrailArmed = false;
        _previousVelocityMagnitude = 0f;
        // _trailingParticles.Stop();
        
        if (_heldParticles != null) {
            _heldParticles.Stop();
            _heldParticlesPlaying = false;
        }
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
        
        if (_useSineGlowFade && (_isGlowFadingIn || _isGlowFadingOut))
            UpdateGlowFade();
        
        if (_isBrightnessPulsing)
            UpdateBrightnessPulse();
        
        // if (IsPulled) {
        //     UpdateGhostTrail();
        // }
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
    
    private void UpdateBrightnessPulse() {
        _brightnessElapsedTime += Time.deltaTime;
        
        // Calculate brightness using sine wave for smooth pulsing
        // Frequency determines how many pulses per second
        float sineValue = Mathf.Sin(_brightnessElapsedTime * _pulseFrequency * 2f * Mathf.PI);
        // Map sine wave from [-1, 1] to [0, 1]
        float normalizedSine = (sineValue + 1f) / 2f;
        // Lerp between default (1) and max brightness
        _currentBrightness = Mathf.Lerp(1f, _maxBrightness, normalizedSine);
        _material.SetFloat("_Brightness", _currentBrightness);
    }
    
    // private void UpdateGhostTrail() {
    //     float currentVelocityMagnitude = _rigidBody.velocity.magnitude;
        
    //     if (currentVelocityMagnitude > _previousVelocityMagnitude && _ghostTrailArmed) {
    //         if (PullableGhostTrailManager.obj != null) {
    //             PullableGhostTrailManager.obj.ShowGhosts();
    //         }
    //         _ghostTrailArmed = false;
    //     }
    //     else if (currentVelocityMagnitude < _previousVelocityMagnitude) {
    //         _ghostTrailArmed = true;
    //     }
        
    //     _previousVelocityMagnitude = currentVelocityMagnitude;
    // }
    
}
