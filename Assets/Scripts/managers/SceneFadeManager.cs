using UnityEngine.UI;
using UnityEngine;

public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager obj;

    [SerializeField] private Image _fadeOutImage;
    [Range(0.1f, 10f), SerializeField] private float _fadeOutSpeed = 5f;
    private float _tempFadeOutSpeed = 0;
    [Range(0.1f, 10f), SerializeField] private float _fadeInSpeed = 5f;

    [SerializeField] private Color _fadeOutStartColor;

    public bool IsFadingOut { get; private set; }
    public bool IsFadingIn { get; private set; }

    void Awake() {
        obj = this;

        _fadeOutStartColor.a = 0f;
    }

    void Update() {
        if(IsFadingOut) {
            if(_fadeOutImage.color.a < 1f) {
                _fadeOutStartColor.a += Time.deltaTime * _fadeOutSpeed;
                _fadeOutImage.color = _fadeOutStartColor;
            } else {
                if(_tempFadeOutSpeed != 0)
                    _fadeOutSpeed = _tempFadeOutSpeed;
                _tempFadeOutSpeed = 0;
                IsFadingOut = false;
            }
        }
        if(IsFadingIn) {
            if(_fadeOutImage.color.a > 0f) {
                _fadeOutStartColor.a -= Time.deltaTime * _fadeInSpeed;
                _fadeOutImage.color = _fadeOutStartColor;
            } else {
                IsFadingIn = false;
            }
        }
    }

    public void StartFadeOut() {
        _fadeOutImage.color = _fadeOutStartColor;
        IsFadingOut = true;
    }
    public void StartFadeOut(float fadeOutSpeed) {
        _fadeOutImage.color = _fadeOutStartColor;
        _tempFadeOutSpeed = _fadeOutSpeed;
        _fadeOutSpeed = fadeOutSpeed;
        IsFadingOut = true;
    }
    public void StartFadeIn() {
        if(_fadeOutImage.color.a >= 1f) {
            _fadeOutImage.color = _fadeOutStartColor;
            IsFadingIn = true;
        }
    }
    public void SetFadedOutState() {
        _fadeOutStartColor.a = 1;
        _fadeOutImage.color = _fadeOutStartColor;
    }
    public void SetFadeInSpeed(float speed) {
        _fadeInSpeed = speed;
    }
}
