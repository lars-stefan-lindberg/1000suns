using System;
using System.Collections.Generic;
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
    public static event Action OnGamepadConnected;
    public static event Action<int> OnGamepadCountChanged;

    public enum Device {
        None,
        Keyboard,
        Gamepad
    }

    public enum JoinDeviceType
    {
        Keyboard,
        XboxGamepad,
        PlayStationGamepad,
        OtherGamepad
    }

    [Serializable]
    public struct AvailableInputDeviceInfo
    {
        public JoinDeviceType DeviceType;
        public InputDevice Device;
    }

    private Device _currentDevice = Device.None;
    private int _lastGamepadCount = 0;

    public Device GetCurrentInputDevice()
    {
        return _currentDevice;
    }

    public int GetAvailableDeviceCount()
    {
        int count = 0;

        if (Keyboard.current != null)
        {
            count += 1;
        }

        count += Gamepad.all.Count;

        return count;
    }

    public List<AvailableInputDeviceInfo> GetAvailableNonActiveDevices()
    {
        var result = new List<AvailableInputDeviceInfo>();

        if (Keyboard.current != null && _currentDevice != Device.Keyboard)
        {
            result.Add(new AvailableInputDeviceInfo
            {
                DeviceType = JoinDeviceType.Keyboard,
                Device = Keyboard.current
            });
        }

        foreach (var gamepad in Gamepad.all)
        {
            if (_currentDevice == Device.Gamepad && Gamepad.current == gamepad)
            {
                continue;
            }

            var type = GetJoinDeviceTypeForGamepad(gamepad);

            result.Add(new AvailableInputDeviceInfo
            {
                DeviceType = type,
                Device = gamepad
            });
        }

        return result;
    }

    public List<AvailableInputDeviceInfo> GetAvailableDevicesExcluding(InputDevice excludedDevice)
    {
        var result = new List<AvailableInputDeviceInfo>();

        if (Keyboard.current != null && Keyboard.current != excludedDevice)
        {
            result.Add(new AvailableInputDeviceInfo
            {
                DeviceType = JoinDeviceType.Keyboard,
                Device = Keyboard.current
            });
        }

        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad == excludedDevice)
            {
                continue;
            }

            var type = GetJoinDeviceTypeForGamepad(gamepad);

            result.Add(new AvailableInputDeviceInfo
            {
                DeviceType = type,
                Device = gamepad
            });
        }

        return result;
    }

    public JoinDeviceType GetJoinDeviceTypeForGamepad(Gamepad gamepad)
    {
        string name = (gamepad.displayName ?? gamepad.name ?? string.Empty).ToLowerInvariant();

        if (name.Contains("xbox") || name.Contains("xinput"))
        {
            return JoinDeviceType.XboxGamepad;
        }

        if (name.Contains("dualshock") || name.Contains("dualsense") || name.Contains("playstation") || name.Contains("ps4") || name.Contains("ps5"))
        {
            return JoinDeviceType.PlayStationGamepad;
        }

        return JoinDeviceType.OtherGamepad;
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
        if(LobbyManager.obj.IsJoiningPlayers || LobbyManager.obj.IsSelectingCharacters) {
            return;
        }
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

        // if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed)
        // {
        //     int currentCount = Gamepad.all.Count;

        //     if (_lastGamepadCount < 2 && currentCount >= 2)
        //     {
        //         var g1 = Gamepad.all[0];
        //         var g2 = Gamepad.all[1];
        //         OnSecondGamepadConnected?.Invoke(g1, g2);
        //     }
        //     else if (_lastGamepadCount >= 2 && currentCount < 2)
        //     {
        //         OnSecondGamepadDisconnected?.Invoke();
        //     }

        //     OnGamepadCountChanged?.Invoke(currentCount);

        //     _lastGamepadCount = currentCount;
        // }
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed) {
            OnGamepadConnected?.Invoke();
        }
    }

    [Serializable]
    public class InputDeviceChangedEvent : UnityEvent<Device>
    {
    }
}
