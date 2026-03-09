using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

public class PowerUpScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text _headingText;
    [SerializeField] private LocalizedString _headingString;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private LocalizedString _descriptionString;
    [SerializeField] private TMP_Text _inputText;
    [SerializeField] private LocalizedString _inputString;
    [SerializeField] private TMP_Text _detailsText;
    [SerializeField] private LocalizedString _detailsString;
    [SerializeField] private Image _continueIcon;
    [SerializeField] private GameObject _confirmButton;
    [SerializeField] private List<InputActionReference> _inputActions = new List<InputActionReference>();
    [SerializeField] private float _blinkVisibleDuration = 0.8f;
    [SerializeField] private float _blinkInvisibleDuration = 0.3f;
    public bool PowerUpScreenCompleted = false;

    private CanvasGroup _canvasGroup;
    private readonly float _fadeInDuration = 1f;
    private readonly float _fadeOutDuration = 1f;
    private bool _isConfirmIconBlinking = false;
    private float _blinkTimer = 0f;
    private bool _isIconVisible = true;

    void Awake() {
        Canvas canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        canvas.sortingLayerName = "UI";

        _canvasGroup = GetComponent<CanvasGroup>();
        _headingText.text = _headingString.GetLocalizedString();
        _descriptionText.text = _descriptionString.GetLocalizedString();
        _detailsText.text = _detailsString.GetLocalizedString();

        UpdateInputStringWithIcons();
    }

    public void Show() {
        gameObject.SetActive(true);

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _canvasGroup
            .DOFade(1f, _fadeInDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            });

        StartCoroutine(DelayedEnableConfirmButton());
    }

    public void OnConfirm() {
        _isConfirmIconBlinking = false;
        Color color = _continueIcon.color;
        color.a = 1f;
        _continueIcon.color = color;
        Hide();
    }

    void Update() {
        if(_isConfirmIconBlinking) {
            _blinkTimer += Time.unscaledDeltaTime;
            
            float currentDuration = _isIconVisible ? _blinkVisibleDuration : _blinkInvisibleDuration;
            
            if(_blinkTimer >= currentDuration) {
                _blinkTimer = 0f;
                _isIconVisible = !_isIconVisible;
                
                Color color = _continueIcon.color;
                color.a = _isIconVisible ? 1f : 0f;
                _continueIcon.color = color;
            }
        }
    }

    private void Hide() {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _canvasGroup
            .DOFade(0f, _fadeOutDuration)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                PowerUpScreenCompleted = true;
            });
    }

    //TODO update PauseMenuManager "find power up button"
    public void Focus() {
        if(_confirmButton.GetComponent<Button>().enabled == true)
            EventSystem.current.SetSelectedGameObject(_confirmButton);
    }

    private IEnumerator DelayedEnableConfirmButton() {
        float timer = 0;
        while(timer < 5f) {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        _confirmButton.GetComponent<Button>().enabled = true;
        
        Color color = _continueIcon.color;
        color.a = 1f;
        _continueIcon.color = color;

        EventSystem.current.SetSelectedGameObject(_confirmButton);
        _isConfirmIconBlinking = true;
    }

    private void UpdateInputStringWithIcons() {
        string deviceLayoutName = InputDeviceListener.obj.GetCurrentDeviceLayoutName();
        
        TMP_SpriteAsset spriteAsset = InputIconManager.obj.GetSpriteAsset(deviceLayoutName);
        _inputText.spriteAsset = spriteAsset;
        
        var spriteArguments = new List<object>();
        
        foreach (var actionReference in _inputActions)
        {
            if (actionReference == null || actionReference.action == null)
            {
                Debug.LogWarning("PowerUpScreen: InputActionReference is null or has no action");
                continue;
            }
            
            string spriteName = InputIconManager.obj.GetSpriteNameForAction(actionReference);
            spriteArguments.Add($"<voffset=-4px><sprite name=\"{spriteName}\" tint=1></voffset>");
        }
        
        _inputString.Arguments = spriteArguments.ToArray();
        _inputString.StringChanged += UpdateInputText;
        _inputString.RefreshString();
    }
    
    private void UpdateInputText(string value) {
        _inputText.text = value;
    }

    void OnDestroy() {
        _inputString.StringChanged -= UpdateInputText;
    }
}
