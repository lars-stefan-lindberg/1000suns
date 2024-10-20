using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialFooterManager : MonoBehaviour
{
    public static TutorialFooterManager obj;

    public bool IsFadingOut { get; private set; }
    public bool IsFadingIn { get; private set; }

    [Range(0.1f, 10f), SerializeField] private float _fadeOutSpeed = 2f;
    [Range(0.1f, 10f), SerializeField] private float _fadeInSpeed = 2f;

    [SerializeField] private Color _fadeAlpha;

    [SerializeField] private Image _panel;
    private float _panelMaxAlpha;

    private List<Image> _images = new();
    private List<TextMeshProUGUI> _texts = new();

    [ContextMenu("StartFadeIn")]
    public void StartFadeIn() {
        IsFadingIn = true;
    }
    
    [ContextMenu("StartFadeOut")]
    public void StartFadeOut() {
        IsFadingOut = true;
    }

    void Update() {
        if (IsFadingIn) {
            if(_fadeAlpha.a < 1f) {
                _fadeAlpha.a += Time.deltaTime * _fadeInSpeed;
                foreach(Image image in _images) {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, _fadeAlpha.a);
                }
                foreach(TextMeshProUGUI text in _texts) {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, _fadeAlpha.a);
                }
                if(_fadeAlpha.a <= _panelMaxAlpha)
                    _panel.color =  new Color(_panel.color.r, _panel.color.g, _panel.color.b, _fadeAlpha.a);
            } else {
                IsFadingIn = false;
                StartCoroutine(DelayedFadeOut());
            }
        }
        if(IsFadingOut) {
            if(_fadeAlpha.a > 0f) {
                _fadeAlpha.a -= Time.deltaTime * _fadeOutSpeed;
                foreach(Image image in _images) {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, _fadeAlpha.a);
                }
                foreach(TextMeshProUGUI text in _texts) {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, _fadeAlpha.a);
                }
                if(_fadeAlpha.a <= _panelMaxAlpha)
                    _panel.color = new Color(_panel.color.r, _panel.color.g, _panel.color.b, _fadeAlpha.a);
            } else {
                IsFadingOut = false;
                gameObject.SetActive(false);
            }
        }
    }

    public void OnConfirm() {
        IsFadingOut = true;
    }

    private IEnumerator DelayedFadeOut() {
        float timer = 0;
        while(timer < 8f) {
            timer += Time.deltaTime;
            yield return null;
        }
        IsFadingOut = true;
    }

    void Awake() {
        obj = this;
        Image[] images = GetComponentsInChildren<Image>();
        foreach(Image image in images) {
            if(!image.gameObject.CompareTag("TutorialPanel")) {
                _images.Add(image);
            }
        }
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach(TextMeshProUGUI text in texts) {
            _texts.Add(text);
        }
        _panelMaxAlpha = 246f/255f;
    }

    void Destroy() {
        obj = null;
    }
}
