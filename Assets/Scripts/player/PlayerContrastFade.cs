using UnityEngine;

public class PlayerContrastFade : MonoBehaviour
{
    [SerializeField] private float _minContrast = -0.5f;
    [SerializeField] private float _fadeSpeed = 1f;
    
    private SpriteRenderer _spriteRenderer;
    private Material _material;
    private float _originalContrast = 1f;
    private float _targetContrast = 1f;
    private bool _isFading = false;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;
        _originalContrast = _material.GetFloat("_Contrast");
    }

    private void Update() {
        if (_isFading) {
            float currentContrast = _material.GetFloat("_Contrast");
            float newContrast = Mathf.MoveTowards(currentContrast, _targetContrast, _fadeSpeed * Time.deltaTime);
            _material.SetFloat("_Contrast", newContrast);
            
            if (Mathf.Approximately(newContrast, _targetContrast)) {
                _isFading = false;
            }
        }
    }

    public void StartContrastFade() {
        _targetContrast = _minContrast;
        _isFading = true;
    }

    public void ResetContrast() {
        _targetContrast = _originalContrast;
        _isFading = true;
    }

    public void SetMinContrast(float minContrast) {
        _minContrast = minContrast;
    }

    public void SetFadeSpeed(float fadeSpeed) {
        _fadeSpeed = fadeSpeed;
    }
}
