using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class SaveProfileMenuItem : MonoBehaviour, IMoveHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
{
    [SerializeField] private SelectSaveFileScreen _selectSaveFileScreen;
    [SerializeField] private int _id;
    [SerializeField] private SaveFileCard _saveFileCard;
    [SerializeField] private GameObject _saveFileCardButton;
    [SerializeField] private GameObject _deleteSaveFileMenuItem;
    [SerializeField] private RectTransform _leftSelector;
    [SerializeField] private RectTransform _rightSelector;
    [SerializeField] private float _colorChangeDuration = 0.15f;
    [SerializeField] private float _selectorScaleDuration = 0.2f;
    [SerializeField] private InputActionReference _cancelActionReference;
    [SerializeField] private CanvasGroup _backgroundCanvasGroup;
    [SerializeField] private CanvasGroup _newGameCanvasGroup;
    [SerializeField] private CanvasGroup _detailsCanvasGroup;
    [SerializeField] private float _canvasGroupDeselectedAlpha = 0.6f;

    private enum Type {
        NewGame,
        SavedFile
    }

    private Type _type = Type.NewGame;
    private bool _isDeletePromptActive = false;
    private bool _isSelected = false;
    private Coroutine _startCanvasGroupFadeCoroutine;

    void Start() {
        _backgroundCanvasGroup.alpha = _canvasGroupDeselectedAlpha;
        _newGameCanvasGroup.alpha = _canvasGroupDeselectedAlpha;
        _detailsCanvasGroup.alpha = _canvasGroupDeselectedAlpha;
        StartCoroutine(UpdateSaveFileCard());
        _cancelActionReference.action.performed += OnCancelInput;
    }

    void OnDestroy() {
        _cancelActionReference.action.performed -= OnCancelInput;
        
        // Kill all tweens on this component's transforms and canvas groups
        _leftSelector?.DOKill();
        _rightSelector?.DOKill();
        _backgroundCanvasGroup?.DOKill();
        _newGameCanvasGroup?.DOKill();
        _detailsCanvasGroup?.DOKill();
    }

    private void OnCancelInput(InputAction.CallbackContext context) {
        // If delete prompt is active, treat cancel as canceling the delete
        if (_isDeletePromptActive) {
            OnDeleteSaveFileCancel();
        }
    }

    private IEnumerator UpdateSaveFileCard() {
        var hasValidTask = SaveManager.obj.HasValidSave(_id);
        while (!hasValidTask.IsCompleted) {
            yield return null;
        }

        if(hasValidTask.Result) {
            var loadTask = SaveManager.obj.LoadSaveData(_id);
            while (!loadTask.IsCompleted) {
                yield return null;
            }
            SaveData saveData = loadTask.Result;
            _saveFileCard.CreateFromSave(saveData);
            _type = Type.SavedFile;
            _deleteSaveFileMenuItem.GetComponent<Button>().enabled = true;
            var textColor = _deleteSaveFileMenuItem.GetComponentInChildren<TMP_Text>().color;
            _deleteSaveFileMenuItem.GetComponentInChildren<TMP_Text>().color = new Color(textColor.r, textColor.g, textColor.b, 1);
            
            // If this menu item is already selected when the portrait finishes loading, start animations
            if(_isSelected) {
                _saveFileCard.StartAnimations();
            }
        }
    }

    public void OnMove(AxisEventData eventData)
    {
        if(eventData.moveDir == MoveDirection.Up || eventData.moveDir == MoveDirection.Down) {
            UISoundPlayer.obj.PlayBrowse();
        }
        if(_type == Type.SavedFile) {
            if(eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right) {
                UISoundPlayer.obj.PlayBrowse();
            }
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        _isSelected = true;
        _backgroundCanvasGroup.DOFade(1f, _colorChangeDuration).SetUpdate(true);

        if(_startCanvasGroupFadeCoroutine != null) {
            StopCoroutine(_startCanvasGroupFadeCoroutine);
        }
        _startCanvasGroupFadeCoroutine = StartCoroutine(StartCanvasGroupFadeDelayed());
        
        ScaleSelectors(1);
        if(_type == Type.SavedFile) {
            _saveFileCard.StartAnimations();
        }
    }

    private IEnumerator StartCanvasGroupFadeDelayed() {
        //Give first load of the save file some time to load properly so we now _type for sure
        yield return new WaitForSeconds(0.05f);
        
        if(_type == Type.NewGame) {
            _newGameCanvasGroup.DOFade(1f, _colorChangeDuration).SetUpdate(true);
        } else {
            _detailsCanvasGroup.DOFade(1f, _colorChangeDuration).SetUpdate(true);
        }
        _startCanvasGroupFadeCoroutine = null;
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
        if(_startCanvasGroupFadeCoroutine != null) {
            StopCoroutine(_startCanvasGroupFadeCoroutine);
        }

        _isSelected = false;
        _backgroundCanvasGroup.DOFade(_canvasGroupDeselectedAlpha, _colorChangeDuration).SetUpdate(true);
        if(_type == Type.NewGame) {
            _newGameCanvasGroup.DOFade(_canvasGroupDeselectedAlpha, _colorChangeDuration).SetUpdate(true);
        } else {
            _detailsCanvasGroup.DOFade(_canvasGroupDeselectedAlpha, _colorChangeDuration).SetUpdate(true);
        }
        ScaleSelectors(0);
        if(_type == Type.SavedFile) {
            _saveFileCard.StopAnimations();
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        GetComponent<Button>().interactable = false;
        _selectSaveFileScreen.OnSaveProfileSelected(_id);
    }

    protected void OnDisable()
    {
        ScaleSelectors(0);
    }

    public void OnDeleteButtonClick() {
        UISoundPlayer.obj.PlaySelect();
        _saveFileCard.ShowDeleteSaveFileContainer();
        _isDeletePromptActive = true;
        // Prevent global cancel from going back while delete prompt is active
        MainMenuManager.obj.IgnoreCancelInputTemporarily(float.MaxValue);
    }

    public void OnDeleteSaveFileConfirm() {
        UISoundPlayer.obj.PlaySelect();
        _isDeletePromptActive = false;
        SaveManager.obj.DeleteSave(_id);
        _saveFileCard.ShowNewGameContainer();
        _deleteSaveFileMenuItem.GetComponent<Button>().enabled = false;
        var textColor = _deleteSaveFileMenuItem.GetComponentInChildren<TMP_Text>().color;
        _deleteSaveFileMenuItem.GetComponentInChildren<TMP_Text>().color = new Color(textColor.r, textColor.g, textColor.b, 0);
        _type = Type.NewGame;
        EventSystem.current.SetSelectedGameObject(_saveFileCardButton);
        // Re-enable global cancel after prompt is dismissed
        MainMenuManager.obj.IgnoreCancelInputTemporarily(0.3f);
    }

    public void OnDeleteSaveFileCancel() {
        UISoundPlayer.obj.PlayBack();
        _isDeletePromptActive = false;
        _saveFileCard.HideDeleteSaveFileContainer();
        EventSystem.current.SetSelectedGameObject(_deleteSaveFileMenuItem);
        // Re-enable global cancel after prompt is dismissed
        MainMenuManager.obj.IgnoreCancelInputTemporarily(0.3f);
    }
}
