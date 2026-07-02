using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitcher : MonoBehaviour
{
    public static PlayerSwitcher obj;
    public PlayerInput eliInput;
    public PlayerInput deeInput;
    public PlayerInput blobInput;
    public bool isDeeDevMode = false;
    
    private PlayerInput _activePlayerInput;

    public string keyboardControlSchemeName;
    public string gamepadControlSchemeName;

    void Awake()
    {
        obj = this;
    }

    void OnEnable()
    {
        InputDeviceListener.OnInputDeviceStream += OnInputDeviceChange;
    }

    void OnDisable()
    {
        InputDeviceListener.OnInputDeviceStream -= OnInputDeviceChange;
    }

    void Start()
    {
        if(isDeeDevMode) {
            SwitchToDee();
        } else {
            SwitchToEli();
        }
    }

    public void SwitchToEli()
    {
        deeInput.enabled = false;
        blobInput.enabled = false;
        eliInput.enabled = true;
        _activePlayerInput = eliInput;
    }

    public void SwitchToDee()
    {
        eliInput.enabled = false;
        blobInput.enabled = false;
        deeInput.enabled = true;
        _activePlayerInput = deeInput;
    }

    public void SwitchToBlob()
    {
        eliInput.enabled = false;
        deeInput.enabled = false;
        blobInput.enabled = true;
        _activePlayerInput = blobInput;
    }

    public void DisableAll() {
        eliInput.enabled = false;
        deeInput.enabled = false;
        blobInput.enabled = false;
        _activePlayerInput = null;
    }

    public PlayerInput GetActivePlayerInput() {
        return _activePlayerInput;
    }

    void OnDestroy()
    {
        InputDeviceListener.OnInputDeviceStream -= OnInputDeviceChange;
        obj = null;
    }

    public bool IsEliActive() {
        return eliInput.enabled;
    }

    public bool IsBlobActive() {
        return blobInput.enabled;
    }

    public bool IsDeeActive() {
        return deeInput.enabled;
    }

    private void OnInputDeviceChange(InputDeviceListener.Device device)
    {
        if(_activePlayerInput == null)
            return;

        if(device == InputDeviceListener.Device.Keyboard) {
            _activePlayerInput.SwitchCurrentControlScheme(
                keyboardControlSchemeName,
                Keyboard.current, Mouse.current
            );
        } else if(device == InputDeviceListener.Device.Gamepad) {
            _activePlayerInput.SwitchCurrentControlScheme(
                gamepadControlSchemeName,
                Gamepad.current
            );
        }
    }
}
