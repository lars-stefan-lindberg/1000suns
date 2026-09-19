using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InputIconManager : MonoBehaviour
{
    public static InputIconManager obj;
    public TMP_SpriteAsset xbox;
    public TMP_SpriteAsset playstation;
    public TMP_SpriteAsset keyboard;

    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
    
    public enum StickType
    {
        None,      // Not a stick (e.g., dpad, arrow keys, single button)
        LeftStick, // Left analog stick
        RightStick // Right analog stick
    }

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
        return GetSpriteNameForAction(actionReference, Direction.None, StickType.None);
    }
    
    //Assumes there's a current input device set
    public string GetSpriteNameForAction(InputActionReference actionReference, Direction direction)
    {
        return GetSpriteNameForAction(actionReference, direction, StickType.None);
    }
    
    //Assumes there's a current input device set
    public string GetSpriteNameForAction(InputActionReference actionReference, Direction direction, StickType stickType)
    {
        var action = actionReference.action;
        var currentDevice = InputDeviceListener.obj.GetCurrentInputDevice();
        
        string deviceLayoutName = default(string);
        string controlPath = default(string);

        if (currentDevice == InputDeviceListener.Device.Keyboard)
        {
            // Special handling for stick types on keyboard - get the specific directional key
            if (stickType == StickType.RightStick)
            {
                // Find the composite binding and get the specific direction part
                controlPath = GetCompositeDirectionBinding(action, "Keyboard", direction);
            }
            else
            {
                var bindingIndex = action.bindings.IndexOf(x => x.groups.Contains("Keyboard"));
                if (bindingIndex != -1)
                {
                    action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath);
                }
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
        
        // If a direction is specified, append it to the icon name
        if (direction != Direction.None)
        {
            iconName = GetDirectionalIconName(iconName, direction, stickType, currentDevice);
        }
        
        return iconName;
    }
    
    private string GetCompositeDirectionBinding(InputAction action, string controlScheme, Direction direction)
    {
        // Find the composite binding for this control scheme
        // Note: The composite itself may have empty groups, but its parts will have the control scheme
        int compositeIndex = -1;
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];
            
            if (binding.isComposite)
            {
                // Check if any of the composite's parts belong to this control scheme
                bool hasMatchingPart = false;
                for (int j = i + 1; j < action.bindings.Count; j++)
                {
                    var partBinding = action.bindings[j];
                    if (!partBinding.isPartOfComposite)
                        break;
                    
                    if (partBinding.groups.Contains(controlScheme))
                    {
                        hasMatchingPart = true;
                        break;
                    }
                }
                
                if (hasMatchingPart)
                {
                    compositeIndex = i;
                    break;
                }
            }
        }
        
        if (compositeIndex == -1)
        {
            return string.Empty;
        }
        
        // Map direction to composite part name
        string partName = direction switch
        {
            Direction.Up => "up",
            Direction.Down => "down",
            Direction.Left => "left",
            Direction.Right => "right",
            _ => string.Empty
        };
        
        // Find the part of the composite that matches the direction
        for (int i = compositeIndex + 1; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];
            
            // Stop if we hit another composite or non-part binding
            if (!binding.isPartOfComposite)
            {
                break;
            }
            
            if (binding.name == partName)
            {
                // Extract the control name from the path (e.g., "<Keyboard>/i" -> "i")
                string path = binding.path;
                int lastSlash = path.LastIndexOf('/');
                if (lastSlash >= 0 && lastSlash < path.Length - 1)
                {
                    return path.Substring(lastSlash + 1);
                }
                return path;
            }
        }
        
        return string.Empty;
    }
    
    private string GetDirectionalIconName(string baseIconName, Direction direction, StickType stickType, InputDeviceListener.Device currentDevice)
    {
        // Map direction to common icon naming conventions
        string directionSuffix = direction switch
        {
            Direction.Up => "up",
            Direction.Down => "down",
            Direction.Left => "left",
            Direction.Right => "right",
            _ => ""
        };
        
        // Handle gamepad controls
        if (currentDevice == InputDeviceListener.Device.Gamepad)
        {
            // If a stick type is specified, use stick icons
            if (stickType == StickType.LeftStick)
            {
                return $"leftStick{char.ToUpper(directionSuffix[0])}{directionSuffix.Substring(1)}";
            }
            else if (stickType == StickType.RightStick)
            {
                return $"rightStick{char.ToUpper(directionSuffix[0])}{directionSuffix.Substring(1)}";
            }
            // Otherwise assume dpad
            else if (baseIconName.ToLower().Contains("dpad"))
            {
                return $"{directionSuffix}Dpad";
            }
            // Fallback for other gamepad controls
            else
            {
                return $"{directionSuffix}Dpad";
            }
        }
        // Handle keyboard controls
        else
        {
            // Special case: If RightStick is specified on keyboard, use the actual bound key
            // (e.g., for CameraLook which uses I/K keys, not arrow keys)
            if (stickType == StickType.RightStick)
            {
                // Return the actual bound key icon (baseIconName already contains it)
                return baseIconName;
            }
            // For traditional directional input (dpad equivalent), use arrow keys
            else
            {
                return $"{directionSuffix}Arrow";
            }
        }
    }

    void OnDestroy() {
        obj = null;
    }
}
