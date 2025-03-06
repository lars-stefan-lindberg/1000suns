using FunkyCode;
using UnityEngine;

public class LightSprite2DFadeManager : MonoBehaviour
{
    [SerializeField] private LightSprite2D _lightSprite2D;
    [Range(0f, 10f), SerializeField] private float _fadeOutSpeed = 5f;
    [Range(0f, 10f), SerializeField] private float _fadeInSpeed = 5f;
    [Range(0f, 1f), SerializeField] private float _fadeOutAlpha = 0f;
    [Range(0f, 1f), SerializeField] private float _fadeInAlpha = 1f;
    [Range(0f, 1f), SerializeField] private float _startAlpha = 1f;

    [SerializeField] private Color _lightColor;
    public bool IsFadingIn { get; private set; }
    public bool IsFadingOut { get; private set; }

    void Start() {
        if(_lightSprite2D == null)
            _lightSprite2D = GetComponent<LightSprite2D>();
        else
            _lightColor = _lightSprite2D.color;
            
        _lightColor.a = _startAlpha;
    }

    void Update() {
        if(IsFadingIn) {
            if(_lightSprite2D.color.a < _fadeInAlpha) {
                _lightColor.a += Time.deltaTime * _fadeInSpeed;
                _lightSprite2D.color = _lightColor;
            } else {
                IsFadingIn = false;
            }
        }
        if(IsFadingOut) {
            if(_lightSprite2D.color.a > _fadeOutAlpha) {
                _lightColor.a -= Time.deltaTime * _fadeOutSpeed;
                _lightSprite2D.color = _lightColor;
            } else {
                IsFadingOut = false;
            }
        }
    }

    public void StartFadeIn() {
        _lightSprite2D.color = _lightColor;
        IsFadingIn = true;
    }

    public void StartFadeOut() {
        if(_lightSprite2D.color.a >= _fadeInAlpha) {
            _lightSprite2D.color = _lightColor;
            IsFadingOut = true;
        }
    }
    public void SetFadedInState() {
        _lightColor.a = _fadeInAlpha;
        _lightSprite2D.color = _lightColor;
    }
}
