using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputDeviceListener : MonoBehaviour
{
    public static InputDeviceListener obj;

    public delegate void InputDeviceStream(Device device);

    public static event InputDeviceStream OnInputDeviceStream;

    public enum Device {
        None,
        Keyboard,
        Gamepad
    }

    private Device _currentDevice = Device.None;

    public Device GetCurrentInputDevice()
    {
        return _currentDevice;
    }

    void Awake() {
        obj = this;
        if(Gamepad.current != null) {
            _currentDevice = Device.Gamepad;
        } else {
            _currentDevice = Device.Keyboard;
        }
        InputSystem.onActionChange += OnAnyInputPerformed;
    }

    private void OnDestroy()
    {
        InputSystem.onActionChange -= OnAnyInputPerformed;
        obj = null;
    }

    private void OnAnyInputPerformed(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            InputAction receivedInputAction = (InputAction) obj;
            InputDevice lastDevice = receivedInputAction.activeControl.device;
            if(lastDevice.name.Contains("Keyboard") && _currentDevice != Device.Keyboard) {
                _currentDevice = Device.Keyboard;
                OnInputDeviceStream?.Invoke(_currentDevice);
            } else if(lastDevice.name.Contains("Gamepad") && _currentDevice != Device.Gamepad) {
                _currentDevice = Device.Gamepad;
                OnInputDeviceStream?.Invoke(_currentDevice);
            }
        }
    }

    [Serializable]
    public class InputDeviceChangedEvent : UnityEvent<Device>
    {
    }
}
