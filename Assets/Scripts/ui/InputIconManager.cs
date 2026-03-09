using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InputIconManager : MonoBehaviour
{
    public static InputIconManager obj;
    public TMP_SpriteAsset xbox;
    public TMP_SpriteAsset playstation;
    public TMP_SpriteAsset keyboard;

    void Awake() {
        obj = this;
    }

    public TMP_SpriteAsset GetSpriteAsset(string deviceLayout)
    {
        if(deviceLayout == null)
            return keyboard;
            
        if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayout, "DualShockGamepad"))
            return playstation;

        if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayout, "Gamepad"))
            return xbox;

        return keyboard;
    }

    public string GetIconName(string controlPath)
    {
        if (string.IsNullOrEmpty(controlPath))
            return string.Empty;

        switch (controlPath)
        {
            case "leftCtrl":
            case "rightCtrl":
                return "ctrl";

            case "leftShift":
            case "rightShift":
                return "shift";

            case "leftAlt":
            case "rightAlt":
                return "alt";

            case "leftMeta":
            case "rightMeta":
                return "meta";
        }

        return controlPath;
    }

    //Assumes there's a current input device set
    public string GetSpriteNameForAction(InputActionReference actionReference)
    {
        var action = actionReference.action;
        var currentDevice = InputDeviceListener.obj.GetCurrentInputDevice();
        
        string deviceLayoutName = default(string);
        string controlPath = default(string);

        if (currentDevice == InputDeviceListener.Device.Keyboard)
        {
            var bindingIndex = action.bindings.IndexOf(x => x.groups.Contains("Keyboard"));
            if (bindingIndex != -1)
            {
                action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath);
            }
        }
        else if (currentDevice == InputDeviceListener.Device.Gamepad)
        {
            var bindingIndex = action.bindings.IndexOf(x => x.groups.Contains("Gamepad"));
            if (bindingIndex != -1)
            {
                action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath);
            }
        }
        
        if (string.IsNullOrEmpty(controlPath))
        {
            Debug.LogWarning($"InputIconManager: Could not find binding for action {action.name} on device {currentDevice}");
            return "buttonSouth";
        }
        
        string iconName = GetIconName(controlPath);
        return iconName;
    }

    void OnDestroy() {
        obj = null;
    }
}
