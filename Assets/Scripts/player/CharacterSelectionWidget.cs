using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSelectionWidget : MonoBehaviour
{
    public int playerIndex;
    public TextMeshProUGUI _playerLabel;
    [SerializeField] private ScriptableStats _stats;
    [SerializeField] private GameObject _xboxJoinIcon;
    [SerializeField] private GameObject _psJoinIcon;
    [SerializeField] private GameObject _keyboardJoinIcon;
    [SerializeField] private GameObject _readyLabel;
    [SerializeField] private GameObject _confirmedReadyIcon;
    public bool IsReady = false;
    private RectTransform _rectTransform;
    private PlayerSlot _playerSlot;

    private float _widgetPositionLeft = -360;
    private float _widgetPositionRight = 360;

    private enum WidgetPosition {
        Left,
        Middle,
        Right
    }

    private WidgetPosition _widgetPosition = WidgetPosition.Middle;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        HideAllIcons();
    }

    public RectTransform GetRectTransform() {
        return _rectTransform;
    }

    public void OnMove(InputAction.CallbackContext context) {
        if(context.performed) {
            Vector2 moveDirection = context.ReadValue<Vector2>();
            moveDirection.x = GetHorizontalInput(moveDirection.x);
            if (moveDirection.x > 0)
            {
                if (_widgetPosition == WidgetPosition.Left)
                {
                    _widgetPosition = WidgetPosition.Right;
                    SetNonReady();
                    MoveToX(_widgetPositionRight);
                }
                else if (_widgetPosition == WidgetPosition.Middle)
                {
                    _widgetPosition = WidgetPosition.Right;
                    SetNonReady();
                    MoveToX(_widgetPositionRight);
                }
            }
            else if (moveDirection.x < 0)
            {
                if (_widgetPosition == WidgetPosition.Right)
                {
                    _widgetPosition = WidgetPosition.Left;
                    SetNonReady();
                    MoveToX(_widgetPositionLeft);
                }
                else if (_widgetPosition == WidgetPosition.Middle)
                {
                    _widgetPosition = WidgetPosition.Left;
                    SetNonReady();
                    MoveToX(_widgetPositionLeft);
                }
            }
        }
    }

    private void SetNonReady() {
        IsReady = false;
        _confirmedReadyIcon.SetActive(false);
        _playerSlot.character = PlayerSlot.CharacterType.None;
        UpdateUI();
    }

    private void UpdateUI() {
        if(_widgetPosition == WidgetPosition.Left || _widgetPosition == WidgetPosition.Right) {
            _readyLabel.SetActive(true);
            if(_playerSlot.device is Keyboard) {
                _keyboardJoinIcon.SetActive(true);
            } else {
                InputDeviceListener.JoinDeviceType joinDeviceType = InputDeviceListener.obj.GetJoinDeviceTypeForGamepad(_playerSlot.device as Gamepad);
                if(joinDeviceType == InputDeviceListener.JoinDeviceType.XboxGamepad || joinDeviceType == InputDeviceListener.JoinDeviceType.OtherGamepad) {
                    _xboxJoinIcon.SetActive(true);
                } else if(joinDeviceType == InputDeviceListener.JoinDeviceType.PlayStationGamepad) {
                    _psJoinIcon.SetActive(true);
                }
            }
        } else {
            IsReady = false;
            HideAllIcons();
        }
    }

    private void MoveToX(float x)
    {
        _rectTransform.anchoredPosition = 
            new Vector2(x, _rectTransform.anchoredPosition.y);
        UpdateUI();
        SoundFXManager.obj.PlayUIBrowse();
    }

    private float GetHorizontalInput(float originInput) {
        if(originInput < -_stats.HorizontalStrongDeadZoneThreshold) {
            return -1;
        } else if(originInput > _stats.HorizontalDeadZoneThreshold) {
            return 1;
        } else {
            return 0;
        }
    }

    public void OnConfirm(InputAction.CallbackContext context) {
        if(context.performed) {
            if(!IsReady && (_widgetPosition == WidgetPosition.Left || _widgetPosition == WidgetPosition.Right)) {
                //Check that selected player hasn't been selected by the other player
                PlayerSlot otherPlayerSlot = LobbyManager.obj.GetPlayerSlots().Where(slot => slot.slotIndex != _playerSlot.slotIndex).First();
                if(_widgetPosition == WidgetPosition.Right && otherPlayerSlot.character != PlayerSlot.CharacterType.Dee ||
                    _widgetPosition == WidgetPosition.Left && otherPlayerSlot.character != PlayerSlot.CharacterType.Eli) {
                        
                    _playerSlot.character = _widgetPosition == WidgetPosition.Right ? PlayerSlot.CharacterType.Dee : PlayerSlot.CharacterType.Eli;
                    IsReady = true;
                    HideAllIcons();
                    _confirmedReadyIcon.SetActive(true);
                    SoundFXManager.obj.PlayUIConfirm();
                } else {
                    SoundFXManager.obj.PlayUIBack();
                }
            }
        }
    }

    public void OnCancel(InputAction.CallbackContext context) {
        if(context.performed) {
            if(IsReady) {
                SetNonReady();
                SoundFXManager.obj.PlayUIBack();
            } else {
                //if both players are not ready, go back to player device setup
                bool noCharactersSelected = LobbyManager.obj.GetPlayerSlots().TrueForAll(slot => slot.character == PlayerSlot.CharacterType.None);
                if(noCharactersSelected) {
                    MainMenuManager.obj.OnNavigateBack();
                    SoundFXManager.obj.PlayUIBack();
                }
            }
        }
    }

    public void SetPlayerSlot(PlayerSlot playerSlot) {
        _playerSlot = playerSlot;
        _playerLabel.text = "Player " + (playerSlot.slotIndex + 1);
        HideAllIcons();
    }

    private void HideAllIcons() {
        _xboxJoinIcon.SetActive(false);
        _psJoinIcon.SetActive(false);
        _keyboardJoinIcon.SetActive(false);
        _confirmedReadyIcon.SetActive(false);
        _readyLabel.SetActive(false);
    }
}
