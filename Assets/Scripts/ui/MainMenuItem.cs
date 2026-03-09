using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class MainMenuItem : MonoBehaviour, IMoveHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _deselectedColor;
    [SerializeField] private RectTransform _leftSelector;
    [SerializeField] private RectTransform _rightSelector;
    [SerializeField] private float _colorChangeDuration = 0.15f;
    [SerializeField] private float _selectorScaleDuration = 0.2f;

    public void OnMove(AxisEventData eventData)
    {
        if(eventData.moveDir == MoveDirection.Up || eventData.moveDir == MoveDirection.Down) {
            UISoundPlayer.obj.PlayBrowse();
        } 
    }

    public void OnSelect(BaseEventData eventData)
    {
        _text.DOColor(_selectedColor, _colorChangeDuration).SetUpdate(true);
        ScaleSelectors(1);
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
    }

    protected void OnDisable()
    {
        DeselectColorChange();
        ScaleSelectors(0);
    }
}
