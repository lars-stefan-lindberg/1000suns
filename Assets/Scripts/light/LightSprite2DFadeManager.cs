using FunkyCode;
using UnityEngine;

public class LightSprite2DFadeManager : MonoBehaviour
{
    private LightSprite2D _lightSprite2D;
    [Range(0.1f, 10f), SerializeField] private float _fadeOutSpeed = 5f;
    [Range(0.1f, 10f), SerializeField] private float _fadeInSpeed = 5f;
    [Range(0f, 1f), SerializeField] private float _fadeOutAlpha = 0f;

    private Color _fadeOutStartColor;
    private float _maxAlpha;

    public bool IsFadingOut { get; private set; }
    public bool IsFadingIn { get; private set; }

    void Awake() {
        _lightSprite2D = GetComponent<LightSprite2D>();
        _fadeOutStartColor = _lightSprite2D.color;
        _fadeOutStartColor.a = 0f;
        _maxAlpha = _lightSprite2D.color.a;
    }

    void Update() {
        if(IsFadingOut) {
            if(_lightSprite2D.color.a < _maxAlpha) {
                _fadeOutStartColor.a += Time.deltaTime * _fadeOutSpeed;
                _lightSprite2D.color = _fadeOutStartColor;
            } else {
                IsFadingOut = false;
            }
        }
        if(IsFadingIn) {
            if(_lightSprite2D.color.a > _fadeOutAlpha) {
                _fadeOutStartColor.a -= Time.deltaTime * _fadeInSpeed;
                _lightSprite2D.color = _fadeOutStartColor;
            } else {
                IsFadingIn = false;
            }
        }
    }

    public void StartFadeOut() {
        _lightSprite2D.color = _fadeOutStartColor;
        IsFadingOut = true;
    }

    public void StartFadeIn() {
        if(_lightSprite2D.color.a >= _maxAlpha) {
            _lightSprite2D.color = _fadeOutStartColor;
            IsFadingIn = true;
        }
    }
    public void SetFadedOutState() {
        _fadeOutStartColor.a = _maxAlpha;
        _lightSprite2D.color = _fadeOutStartColor;
    }
}
