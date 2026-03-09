using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RebindInputMenuItem : MonoBehaviour, IMoveHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private TMP_Text _icon;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _deselectedColor;
    [SerializeField] private RectTransform _leftSelector;
    [SerializeField] private RectTransform _rightSelector;
    [SerializeField] private float _colorChangeDuration = 0.15f;
    [SerializeField] private float _selectorScaleDuration = 0.2f;

    public void OnMove(AxisEventData eventData)
    {
        UISoundPlayer.obj.PlayBrowse();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _label.DOColor(_selectedColor, _colorChangeDuration).SetUpdate(true);
        _icon.DOColor(_selectedColor, _colorChangeDuration).SetUpdate(true);
        ScaleSelectors(1);
    }

    public void OnDeselect(BaseEventData eventData)
    {
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
        _icon.DOColor(_deselectedColor, _colorChangeDuration).SetUpdate(true);
    }

    private void ScaleSelectors(float scaleValue) {
        _leftSelector.DOScale(scaleValue, _selectorScaleDuration).SetUpdate(true);
        _rightSelector.DOScale(new Vector3(-scaleValue, scaleValue, scaleValue), _selectorScaleDuration).SetUpdate(true);
    }
}
