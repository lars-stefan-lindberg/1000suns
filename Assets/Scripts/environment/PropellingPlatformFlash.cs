using UnityEngine;

public class PropellingPlatformFlash : MonoBehaviour
{
    [Header("Idle Pulsating Flash")]
    [SerializeField] private float _idleFlashIntensity = 0.7f;
    [SerializeField] private float _idleFlashSpeed = 1f;
    
    [Header("VFX Flash")]
    [SerializeField] private float _vfxFlashIntensity = 0.3f;
    [SerializeField] private float _vfxFlashSpeed = 0.15f;
    [SerializeField] private float _vfxFlashReturnSpeed = 0.5f;

    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private float _elapsedTime = 0f;
    private float _currentFlashAmount = 1f;
    private bool _blended = false;
    
    private bool _isVfxFlashing = false;
    private float _vfxElapsedTime = 0f;
    private float _vfxStartValue = 1f;
    
    private bool _isIdleFlashing = false;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
    }

    public void StartIdleFlashing()
    {
        _isIdleFlashing = true;
    }

    public void StopIdleFlashing()
    {
        _isIdleFlashing = false;
        _currentFlashAmount = 1f;
        _material.SetFloat("_Contrast", 1f);
    }

    private void Update()
    {
        if (!_isIdleFlashing && !_isVfxFlashing)
            return;

        // Handle VFX flash (takes priority)
        if (_isVfxFlashing)
        {
            _vfxElapsedTime += Time.deltaTime;
            
            // Flash down to intensity and back up
            if (_vfxElapsedTime < _vfxFlashSpeed)
            {
                // Flash down
                _currentFlashAmount = Mathf.Lerp(_vfxStartValue, _vfxFlashIntensity, _vfxElapsedTime / _vfxFlashSpeed);
            }
            else if (_vfxElapsedTime < _vfxFlashSpeed + _vfxFlashReturnSpeed)
            {
                // Flash back up (slower)
                _currentFlashAmount = Mathf.Lerp(_vfxFlashIntensity, 1f, (_vfxElapsedTime - _vfxFlashSpeed) / _vfxFlashReturnSpeed);
            }
            else
            {
                // VFX flash complete, return to idle pulsating
                _isVfxFlashing = false;
                _currentFlashAmount = 1f;
                _elapsedTime = 0f;
                _blended = true;
            }
            
            _material.SetFloat("_Contrast", _currentFlashAmount);
        }
        else
        {
            // Handle idle pulsating flash
            if (_currentFlashAmount <= _idleFlashIntensity)
            {
                _elapsedTime = 0f;
                _blended = false;
            }
            else if (_currentFlashAmount >= 1f)
            {
                _elapsedTime = 0f;
                _blended = true;
            }

            _elapsedTime += Time.deltaTime;

            if (_blended)
            {
                // Pulsate down from 1 to intensity
                _currentFlashAmount = Mathf.Lerp(1f, _idleFlashIntensity, _elapsedTime / _idleFlashSpeed);
            }
            else
            {
                // Pulsate up from intensity to 1
                _currentFlashAmount = Mathf.Lerp(_idleFlashIntensity, 1f, _elapsedTime / _idleFlashSpeed);
            }

            _material.SetFloat("_Contrast", _currentFlashAmount);
        }
    }

    public void TriggerVfxFlash()
    {
        _isVfxFlashing = true;
        _vfxElapsedTime = 0f;
        _vfxStartValue = _currentFlashAmount;
    }
}
