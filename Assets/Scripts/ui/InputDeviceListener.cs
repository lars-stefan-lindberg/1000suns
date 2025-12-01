using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputDeviceListener : MonoBehaviour
{
    public static InputDeviceListener obj;

    public delegate void InputDeviceStream(Device device);

    public static event InputDeviceStream OnInputDeviceStream;

    public static event Action<Gamepad, Gamepad> OnSecondGamepadConnected;
    public static event Action OnSecondGamepadDisconnected;
    public static event Action<int> OnGamepadCountChanged;

    public enum Device {
        None,
        Keyboard,
        Gamepad
    }

    private Device _currentDevice = Device.None;
    private int _lastGamepadCount = 0;

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
        _lastGamepadCount = Gamepad.all.Count;
        OnGamepadCountChanged?.Invoke(_lastGamepadCount);
        if (_lastGamepadCount >= 2)
        {
            var g1 = Gamepad.all[0];
            var g2 = Gamepad.all[1];
            OnSecondGamepadConnected?.Invoke(g1, g2);
        }
        InputSystem.onActionChange += OnAnyInputPerformed;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDestroy()
    {
        InputSystem.onActionChange -= OnAnyInputPerformed;
        InputSystem.onDeviceChange -= OnDeviceChange;
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

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (!(device is Gamepad))
        {
            return;
        }

        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed)
        {
            int currentCount = Gamepad.all.Count;

            if (_lastGamepadCount < 2 && currentCount >= 2)
            {
                var g1 = Gamepad.all[0];
                var g2 = Gamepad.all[1];
                OnSecondGamepadConnected?.Invoke(g1, g2);
            }
            else if (_lastGamepadCount >= 2 && currentCount < 2)
            {
                OnSecondGamepadDisconnected?.Invoke();
            }

            OnGamepadCountChanged?.Invoke(currentCount);

            _lastGamepadCount = currentCount;
        }
    }

    [Serializable]
    public class InputDeviceChangedEvent : UnityEvent<Device>
    {
    }
}
