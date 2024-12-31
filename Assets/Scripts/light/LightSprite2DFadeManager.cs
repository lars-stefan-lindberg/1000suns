using FunkyCode;
using UnityEngine;

public class LightSprite2DFadeManager : MonoBehaviour
{
    private LightSprite2D _lightSprite2D;
    [Range(0.1f, 10f), SerializeField] private float _fadeOutSpeed = 5f;
    [Range(0.1f, 10f), SerializeField] private float _fadeInSpeed = 5f;
    [Range(0f, 1f), SerializeField] private float _fadeOutAlpha = 0f;
    [Range(0f, 1f), SerializeField] private float _fadeInAlpha = 1f;

    private Color _fadeOutStartColor;
    public bool IsFadingIn { get; private set; }
    public bool IsFadingOut { get; private set; }

    void Awake() {
        _lightSprite2D = GetComponent<LightSprite2D>();
        _fadeOutStartColor = _lightSprite2D.color;
        _fadeOutStartColor.a = 0f;
    }

    void Update() {
        if(IsFadingIn) {
            if(_lightSprite2D.color.a < _fadeInAlpha) {
                _fadeOutStartColor.a += Time.deltaTime * _fadeOutSpeed;
                _lightSprite2D.color = _fadeOutStartColor;
            } else {
                IsFadingIn = false;
            }
        }
        if(IsFadingOut) {
            if(_lightSprite2D.color.a > _fadeOutAlpha) {
                _fadeOutStartColor.a -= Time.deltaTime * _fadeInSpeed;
                _lightSprite2D.color = _fadeOutStartColor;
            } else {
                IsFadingOut = false;
            }
        }
    }

    public void StartFadeIn() {
        _lightSprite2D.color = _fadeOutStartColor;
        IsFadingIn = true;
    }

    public void StartFadeOut() {
        if(_lightSprite2D.color.a >= _fadeInAlpha) {
            _lightSprite2D.color = _fadeOutStartColor;
            IsFadingOut = true;
        }
    }
    public void SetFadedInState() {
        _fadeOutStartColor.a = _fadeInAlpha;
        _lightSprite2D.color = _fadeOutStartColor;
    }
}
