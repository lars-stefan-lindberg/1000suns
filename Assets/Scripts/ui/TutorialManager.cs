using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager obj;

    public bool IsFadingOut { get; private set; }
    public bool IsFadingIn { get; private set; }

    [Range(0.1f, 10f), SerializeField] private float _fadeOutSpeed = 2f;
    [Range(0.1f, 10f), SerializeField] private float _fadeInSpeed = 2f;

    [SerializeField] private Color _fadeAlpha;

    private List<Image> _images = new();
    private List<TextMeshProUGUI> _texts = new();
    private Button _confirmButton;

    public bool tutorialCompleted = false;

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
                _fadeAlpha.a += Time.unscaledDeltaTime * _fadeInSpeed;
                foreach(Image image in _images) {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, _fadeAlpha.a);
                }
                foreach(TextMeshProUGUI text in _texts) {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, _fadeAlpha.a);
                }
            } else {
                IsFadingIn = false;
                StartCoroutine(EnableConfirmButton());
            }
        }
        if(IsFadingOut) {
            if(_fadeAlpha.a > 0f) {
                _fadeAlpha.a -= Time.unscaledDeltaTime * _fadeOutSpeed;
                foreach(Image image in _images) {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, _fadeAlpha.a);
                }
                foreach(TextMeshProUGUI text in _texts) {
                    text.color = new Color(text.color.r, text.color.g, text.color.b, _fadeAlpha.a);
                }
            } else {
                IsFadingOut = false;
                tutorialCompleted = true;
            }
        }
    }

    public void OnConfirm() {
        IsFadingOut = true;
    }

    private IEnumerator EnableConfirmButton() {
        float timer = 0;
        while(timer < 5f) {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        _confirmButton.enabled = true;
        EventSystem.current.SetSelectedGameObject(_confirmButton.gameObject);
    }

    void Awake() {
        obj = this;
        Image[] images = GetComponentsInChildren<Image>();
        foreach(Image image in images) {
            if(!image.gameObject.CompareTag("TutorialDialogConfirmButton")) {
                _images.Add(image);
            }
        }
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
        foreach(TextMeshProUGUI text in texts) {
            if(!text.gameObject.CompareTag("TutorialDialogConfirmButton")) {
                _texts.Add(text);
            }
        }
        _confirmButton = GetComponentInChildren<Button>();
        _confirmButton.enabled = false;
    }

    void Destroy() {
        obj = null;
    }
}
