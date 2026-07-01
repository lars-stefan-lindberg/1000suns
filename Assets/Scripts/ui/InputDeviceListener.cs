using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputDeviceListener : MonoBehaviour
{
    public static InputDeviceListener obj;

    public delegate void InputDeviceStream(Device device);

    public static event InputDeviceStream OnInputDeviceStream;

    public static event Action OnGamepadConnected;

    public enum Device {
        None,
        Keyboard,
        Gamepad
    }

    private Device _currentDevice = Device.None;
    private Gamepad _activeGamepad = null;

    public Device GetCurrentInputDevice()
    {
        return _currentDevice;
    }
    
    public Gamepad GetActiveGamepad()
    {
        return _activeGamepad;
    }

    public string GetCurrentDeviceLayoutName()
    {
        if (_currentDevice == Device.Keyboard)
        {
            return "Keyboard";
        }
        else if (_currentDevice == Device.Gamepad && Gamepad.current != null)
        {
            return Gamepad.current.layout;
        }
        
        return null;
    }

    void Awake() {
        obj = this;
        if(Gamepad.current != null) {
            _currentDevice = Device.Gamepad;
            _activeGamepad = Gamepad.current;
        } else {
            _currentDevice = Device.Keyboard;
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
            
            Device newDevice = Device.None;
            bool gamepadChanged = false;
            
            if (lastDevice is Keyboard)
            {
                newDevice = Device.Keyboard;
            }
            else if (lastDevice is Gamepad currentGamepad)
            {
                newDevice = Device.Gamepad;
                
                // Check if it's a different gamepad than the currently active one
                if (_activeGamepad != currentGamepad)
                {
                    _activeGamepad = currentGamepad;
                    gamepadChanged = true;
                }
            }
            
            // Trigger event if device type changed OR if it's a different gamepad
            if (newDevice != Device.None && (newDevice != _currentDevice || gamepadChanged))
            {
                _currentDevice = newDevice;
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

        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed) {
            OnGamepadConnected?.Invoke();
        }
    }

    [Serializable]
    public class InputDeviceChangedEvent : UnityEvent<Device>
    {
    }
}
