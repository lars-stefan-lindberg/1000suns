using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitcher : MonoBehaviour
{
    public static PlayerSwitcher obj;
    public PlayerInput eliInput;
    public PlayerInput deeInput;
    public PlayerInput blobInput;

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
        SwitchToEli();
    }

    public void SwitchToEli()
    {
        deeInput.enabled = false;
        blobInput.enabled = false;
        eliInput.enabled = true;
    }

    public void SwitchToDee()
    {
        eliInput.enabled = false;
        blobInput.enabled = false;
        deeInput.enabled = true;
    }

    public void SwitchToBlob()
    {
        eliInput.enabled = false;
        deeInput.enabled = false;
        blobInput.enabled = true;
    }

    public void DisableAll() {
        eliInput.enabled = false;
        deeInput.enabled = false;
        blobInput.enabled = false;
    }

    public PlayerInput GetActivePlayerInput() {
        if(eliInput.enabled)
            return eliInput;
        else if(deeInput.enabled)
            return deeInput;
        else if(blobInput.enabled)
            return blobInput;
        return null;
    }

    void OnDestroy()
    {
        InputDeviceListener.OnInputDeviceStream += OnInputDeviceChange;
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
        var activePlayerInput = GetActivePlayerInput();
        if(activePlayerInput == null)
            return;

        if(device == InputDeviceListener.Device.Keyboard) {
            activePlayerInput.SwitchCurrentControlScheme(
                keyboardControlSchemeName,
                Keyboard.current, Mouse.current
            );
        } else if(device == InputDeviceListener.Device.Gamepad) {
            activePlayerInput.SwitchCurrentControlScheme(
                gamepadControlSchemeName,
                Gamepad.current
            );
        }
    }
}
