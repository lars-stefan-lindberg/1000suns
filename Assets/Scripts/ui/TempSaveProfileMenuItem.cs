using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using UnityEngine.Localization;

public class TempSaveProfileMenuItem : MonoBehaviour, IMoveHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    [SerializeField] private SelectSaveFileScreen _selectSaveFileScreen;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private TMP_Text _idText;
    [SerializeField] private int _id;
    [SerializeField] private LocalizedString _newGame;
    [SerializeField] private LocalizedString _continueGame;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _deselectedColor;
    [SerializeField] private RectTransform _leftSelector;
    [SerializeField] private RectTransform _rightSelector;
    [SerializeField] private float _colorChangeDuration = 0.15f;
    [SerializeField] private float _selectorScaleDuration = 0.2f;

    void Start() {
        _idText.text = _id.ToString();

        if(SaveManager.obj.HasValidSave(_id)) {
            _text.text = _continueGame.GetLocalizedString();
        } else {
            _text.text = _newGame.GetLocalizedString();
        }
    }

    public void OnMove(AxisEventData eventData)
    {
        if(eventData.moveDir == MoveDirection.Up || eventData.moveDir == MoveDirection.Down) {
            UISoundPlayer.obj.PlayBrowse();
        } 
    }

    public void OnSelect(BaseEventData eventData)
    {
        _text.DOColor(_selectedColor, _colorChangeDuration).SetUpdate(true);
        _idText.DOColor(_selectedColor, _colorChangeDuration).SetUpdate(true);
        ScaleSelectors(1);
    }

    public int GetId() {
        return _id;
    }

    private void ScaleSelectors(float scaleValue) {
        _leftSelector.DOScale(scaleValue, _selectorScaleDuration).SetUpdate(true);
        _rightSelector.DOScale(new Vector3(-scaleValue, scaleValue, scaleValue), _selectorScaleDuration).SetUpdate(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        DeselectColorChange();
        ScaleSelectors(0);
    }

    private void DeselectColorChange() {
        _text.DOColor(_deselectedColor, _colorChangeDuration).SetUpdate(true);
        _idText.DOColor(_deselectedColor, _colorChangeDuration).SetUpdate(true);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        _selectSaveFileScreen.OnSaveProfileSelected(_id);
    }

    protected void OnDisable()
    {
        DeselectColorChange();
        ScaleSelectors(0);
    }
}
