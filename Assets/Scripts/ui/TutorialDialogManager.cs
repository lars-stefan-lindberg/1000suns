using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialDialogManager : MonoBehaviour
{
    public static TutorialDialogManager obj;

    public bool IsFadingOut { get; private set; }
    public bool IsFadingIn { get; private set; }

    [Range(0.1f, 10f), SerializeField] private float _fadeOutSpeed = 2f;
    [Range(0.1f, 10f), SerializeField] private float _fadeInSpeed = 2f;

    [SerializeField] private Color _fadeAlpha;
    [SerializeField] private InputActionReference _usePowerActionReference;
    [SerializeField] private Image _gamepadConfigInstructionsUsePowerActionKeyIcon;

    private List<Image> _images = new();
    private List<TextMeshProUGUI> _texts = new();
    private Button _confirmButton;

    public bool tutorialCompleted = false;
    private string userPowerActionKeyboardDisplayString;
    [SerializeField] private TextMeshProUGUI _keyboardConfigInstructionsUsePowerActionKeyText;

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
                for (int i = 0; i < _images.Count; i++) {
                    _images[i].color = new Color(_images[i].color.r, _images[i].color.g, _images[i].color.b, _fadeAlpha.a);
                }
                for (int i = 0; i < _texts.Count; i++) {
                    _texts[i].color = new Color(_texts[i].color.r, _texts[i].color.g, _texts[i].color.b, _fadeAlpha.a);
                }
            } else {
                IsFadingIn = false;
                StartCoroutine(EnableConfirmButton());
            }
        }
        if(IsFadingOut) {
            if(_fadeAlpha.a > 0f) {
                _fadeAlpha.a -= Time.unscaledDeltaTime * _fadeOutSpeed;
                for (int i = 0; i < _images.Count; i++) {
                    _images[i].color = new Color(_images[i].color.r, _images[i].color.g, _images[i].color.b, _fadeAlpha.a);
                }
                for (int i = 0; i < _texts.Count; i++) {
                    _texts[i].color = new Color(_texts[i].color.r, _texts[i].color.g, _texts[i].color.b, _fadeAlpha.a);
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

        //Show keyboard, or gamepad, use power icon/text
        if(_gamepadConfigInstructionsUsePowerActionKeyIcon != null)  {
            if(InputDeviceListener.obj.GetCurrentInputDevice() == InputDeviceListener.Device.Gamepad) {
                _gamepadConfigInstructionsUsePowerActionKeyIcon.gameObject.SetActive(true);
                _keyboardConfigInstructionsUsePowerActionKeyText.gameObject.SetActive(false);
                Sprite userPowerButtonSprite = GamepadIconManager.obj.GetIcon(_usePowerActionReference.action);
                _gamepadConfigInstructionsUsePowerActionKeyIcon.sprite = userPowerButtonSprite;
            } else { //Use keyboard
                _gamepadConfigInstructionsUsePowerActionKeyIcon.gameObject.SetActive(false);
                _keyboardConfigInstructionsUsePowerActionKeyText.gameObject.SetActive(true);
                userPowerActionKeyboardDisplayString = _usePowerActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));
                _keyboardConfigInstructionsUsePowerActionKeyText.text = userPowerActionKeyboardDisplayString;
            }
        }

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

    public void Focus() {
        if(_confirmButton.enabled == true)
            EventSystem.current.SetSelectedGameObject(_confirmButton.gameObject);
    }

    void OnDestroy() {
        obj = null;
    }
}
