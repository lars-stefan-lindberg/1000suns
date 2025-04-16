using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("Device UIs")]
    [SerializeField] private GameObject _keyboardInstructions;
    [SerializeField] private GameObject _gamepadInstructions;

    [Header("Action references")]
    [SerializeField] private InputActionReference _usePowerActionReference;
    [SerializeField] private InputActionReference _jumpActionReference;
    [SerializeField] private InputActionReference _directionActionReference;

    [Header("Gamepad icon containers")]
    [SerializeField] private Image _gamepadUsePowerIcon;
    [SerializeField] private Image _gamepadJumpIcon;

    [Header("Keyboard text containers")]
    [SerializeField] private TextMeshProUGUI _keyboardUsePowerText;
    [SerializeField] private TextMeshProUGUI _keyboardForwardText;
    [SerializeField] private TextMeshProUGUI _keyboardJumpText;

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
                for (int i = 0; i < _images.Count; i++) {
                    _images[i].color = new Color(_images[i].color.r, _images[i].color.g, _images[i].color.b, _fadeAlpha.a);
                }
                for (int i = 0; i < _texts.Count; i++) {
                    _texts[i].color = new Color(_texts[i].color.r, _texts[i].color.g, _texts[i].color.b, _fadeAlpha.a);
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
                for (int i = 0; i < _images.Count; i++) {
                    _images[i].color = new Color(_images[i].color.r, _images[i].color.g, _images[i].color.b, _fadeAlpha.a);
                }
                for (int i = 0; i < _texts.Count; i++) {
                    _texts[i].color = new Color(_texts[i].color.r, _texts[i].color.g, _texts[i].color.b, _fadeAlpha.a);
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

        if(_gamepadInstructions != null) {
            if(InputDeviceListener.obj.GetCurrentInputDevice() == InputDeviceListener.Device.Gamepad) {
                _gamepadInstructions.SetActive(true);
                if(_gamepadJumpIcon != null) {
                    _gamepadJumpIcon.gameObject.SetActive(true);
                    _gamepadJumpIcon.sprite = GamepadIconManager.obj.GetIcon(_jumpActionReference.action);
                }
                if(_gamepadUsePowerIcon != null) {
                    _gamepadUsePowerIcon.gameObject.SetActive(true);
                    _gamepadUsePowerIcon.sprite = GamepadIconManager.obj.GetIcon(_usePowerActionReference.action);
                }
            } else {
                _keyboardInstructions.SetActive(true);
                if(_keyboardForwardText != null) {
                    _keyboardForwardText.gameObject.SetActive(true);
                    _keyboardForwardText.text = GetKeyboardForwardDisplayString();
                }
                if(_keyboardUsePowerText != null) {
                    _keyboardUsePowerText.gameObject.SetActive(true);
                    _keyboardUsePowerText.text = _usePowerActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));
                }
                if(_keyboardJumpText != null) {
                    _keyboardJumpText.gameObject.SetActive(true);
                    _keyboardJumpText.text = _jumpActionReference.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard"));
                }
            }
        }

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

    private string GetKeyboardForwardDisplayString() {
        InputAction action = _directionActionReference.action;
        int bindingIndex = action.bindings.IndexOf(x => x.effectivePath == "<Keyboard>/rightArrow");

        if (bindingIndex != -1)
        {
            return action.GetBindingDisplayString(bindingIndex);
        }
        return "";
    }

    void OnDestroy() {
        obj = null;
    }
}
