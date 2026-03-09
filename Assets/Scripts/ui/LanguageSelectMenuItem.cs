using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;

[Serializable]
public class LanguageOption
{
    public string identifier;
    public string displayName;
}

public class LanguageSelectMenuItem : MonoBehaviour, IMoveHandler, ISelectHandler, IDeselectHandler
{
    public InputActionAsset inputActions;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private TMP_Text _valueLabel;
    [SerializeField] private List<LanguageOption> _options;
    [SerializeField] private int _defaultOptionIndex = 0;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _deselectedColor;
    [SerializeField] private RectTransform _leftSelector;
    [SerializeField] private RectTransform _rightSelector;
    [SerializeField] private float _colorChangeDuration = 0.15f;
    [SerializeField] private float _selectorScaleDuration = 0.2f;
    public UnityEvent OnLanguageChange;

    private InputAction moveAction;
    private int _currentSelectedIndex = 0;

    void Awake() {
        if(_options == null || _options.Count == 0) {
            return;
        }

        string currentLocaleIdentifier = LocalizationSettings.SelectedLocale?.Identifier.Code;
        
        if(!string.IsNullOrEmpty(currentLocaleIdentifier)) {
            int index = _options.FindIndex(option => option.identifier == currentLocaleIdentifier);
            if(index >= 0) {
                _currentSelectedIndex = index;
                return;
            }
        }
        
        int englishIndex = _options.FindIndex(option => option.identifier == "en");
        if(englishIndex >= 0) {
            _currentSelectedIndex = englishIndex;
        } else if(_defaultOptionIndex >= 0 && _defaultOptionIndex < _options.Count) {
            _currentSelectedIndex = _defaultOptionIndex;
        }
    }

    void OnEnable() {
        if(_options != null && _options.Count > 0 && _currentSelectedIndex >= 0 && _currentSelectedIndex < _options.Count) {
            _valueLabel.text = _options[_currentSelectedIndex].displayName;
        }
    }

    public void OnMove(AxisEventData eventData)
    {
        if(eventData.moveDir == MoveDirection.Up || eventData.moveDir == MoveDirection.Down) {
            UISoundPlayer.obj.PlayBrowse();
        } else if(eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right) {
            UISoundPlayer.obj.PlaySliderTick();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
         var actionMap = inputActions.FindActionMap("UIControls");
        moveAction = actionMap.FindAction("Move");

        if (moveAction != null)
        {
            moveAction.performed += OnMoveActionPerformed;
        }

        _label.DOColor(_selectedColor, _colorChangeDuration).SetUpdate(true);
        _valueLabel.DOColor(_selectedColor, _colorChangeDuration).SetUpdate(true);
        ScaleSelectors(1);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMoveActionPerformed;
        }
        DeselectColorChange();
        ScaleSelectors(0);
    }

    protected void OnDisable()
    {
        DeselectColorChange();
        ScaleSelectors(0);
    }

    private void DeselectColorChange() {
        _label.DOColor(_deselectedColor, _colorChangeDuration).SetUpdate(true);
        _valueLabel.DOColor(_deselectedColor, _colorChangeDuration).SetUpdate(true);
    }

    private void ScaleSelectors(float scaleValue) {
        _leftSelector.DOScale(scaleValue, _selectorScaleDuration).SetUpdate(true);
        _rightSelector.DOScale(new Vector3(-scaleValue, scaleValue, scaleValue), _selectorScaleDuration).SetUpdate(true);
    }

    private void OnMoveActionPerformed(InputAction.CallbackContext context) {
        Vector2 moveDirection = context.ReadValue<Vector2>();
        
        if (moveDirection.x < 0)
        {
            SelectPrevious();
        }
        else if (moveDirection.x > 0)
        {
            SelectNext();
        }
    }

    public void SelectPrevious() {
        _currentSelectedIndex--;
        if(_currentSelectedIndex < 0) {
            _currentSelectedIndex = _options.Count - 1;
        }
        _valueLabel.text = _options[_currentSelectedIndex].displayName;
        SaveLanguagePreference();
        OnLanguageChange?.Invoke();
    }

    public void SelectNext() {
        _currentSelectedIndex++;
        if(_currentSelectedIndex == _options.Count) {
            _currentSelectedIndex = 0;
        }
        _valueLabel.text = _options[_currentSelectedIndex].displayName;
        SaveLanguagePreference();
        OnLanguageChange?.Invoke();
    }

    private void SaveLanguagePreference() {
        string languageIdentifier = GetCurrentLanguageIdentifier();
        if(!string.IsNullOrEmpty(languageIdentifier)) {
            PlayerPrefs.SetString("PreferredLanguage", languageIdentifier);
            PlayerPrefs.Save();
        }
    }

    public string GetCurrentLanguageIdentifier() {
        if(_options != null && _options.Count > 0 && _currentSelectedIndex >= 0 && _currentSelectedIndex < _options.Count) {
            return _options[_currentSelectedIndex].identifier;
        }
        return null;
    }
}
