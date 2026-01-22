using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class UIElement : MonoBehaviour, IMoveHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] Color changeColor = new Color(1, 1, 1);
    [SerializeField] private bool _isBouncy = false;
    private Tween _colorChange;
    private TextMeshProUGUI _label;
    private Color _startColor;

    void Start() {
        CreateColorChangeTween();
    }

    private void InitalizeLabel() {
        if(_label == null) {
            TextMeshProUGUI[] textMeshProUGUIs = GetComponentsInChildren<TextMeshProUGUI>();
            if(textMeshProUGUIs.Length == 1) {
                _label = textMeshProUGUIs[0];
            } else {
                //Assume rebind element
                _label = textMeshProUGUIs[1];
            }

            if (_label == null) {
                Debug.LogError("TextMeshProUGUI not found!", gameObject);
                return;
            }

            _startColor = _label.color;
        }
    }

    private void CreateColorChangeTween() {
        InitalizeLabel();
        if(_colorChange == null) {
            if(_label == null) {
                Debug.Log("label is null!");
            }
            _colorChange = DOTween.To(() => _label.color, x => _label.color = x, changeColor, 0.25f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetAutoKill(false)
                .SetUpdate(true)
                .Pause();
        }
    }

    public void OnMove(AxisEventData eventData)
    {
        if(eventData.moveDir == MoveDirection.Up || eventData.moveDir == MoveDirection.Down) {
            SoundFXManager.obj.PlayUIBrowse();
        } 
        // else if(eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right) {
        //     SoundFXManager.obj.PlayUIBrowse();
        // }
    }

    private readonly float moveDownLength = 5f;
    private readonly float moveDownDuration = 0.05f;
    private readonly float moveUpLength = 2f;
    private readonly float moveUpDuration = 0.1f;
    private readonly float returnLength = 0.2f;
    public void OnSelect(BaseEventData eventData)
    {
        if (_colorChange == null) {
            CreateColorChangeTween();
        }
        _colorChange.Play();

        if(MainMenuManager.obj != null && MainMenuManager.obj.isNavigatingToMenu) {
            MainMenuManager.obj.isNavigatingToMenu = false;
            return;
        }
        if(PauseMenuManager.obj != null && PauseMenuManager.obj.isNavigatingToMenu) {
            PauseMenuManager.obj.isNavigatingToMenu = false;
            return;
        }

        if(_isBouncy) {
            float originY = transform.localPosition.y;
            transform.DOLocalMoveY(originY - moveDownLength, moveDownDuration)
                .OnStepComplete(
                    () => transform.DOLocalMoveY(originY + moveUpLength, moveUpDuration).SetUpdate(true)
                        .OnComplete(() => transform.DOLocalMoveY(originY, returnLength)).SetUpdate(true)
                ).SetUpdate(true);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        PauseAndResetColorChange();
    }

    private void PauseAndResetColorChange() {
        _colorChange.Pause();
        if(_label != null)
            _label.color = _startColor;
    }

    protected void OnDisable()
    {
        PauseAndResetColorChange();
    }
}
