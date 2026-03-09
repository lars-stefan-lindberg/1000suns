using System;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;


/// <summary>
/// A reusable component with a self-contained UI for rebinding a single action.
/// </summary>
public class RebindActionUI : MonoBehaviour
{
    /// <summary>
    /// Reference to the action that is to be rebound.
    /// </summary>
    public InputActionReference actionReference
    {
        get => m_Action;
        set
        {
            m_Action = value;
            UpdateBindingDisplay();
        }
    }

    /// <summary>
    /// ID (in string form) of the binding that is to be rebound on the action.
    /// </summary>
    /// <seealso cref="InputBinding.id"/>
    public string bindingId
    {
        get => m_BindingId;
        set
        {
            m_BindingId = value;
            UpdateBindingDisplay();
        }
    }

    /// <summary>
    /// Optional UI that is activated when an interactive rebind is started and deactivated when the rebind
    /// is finished. This is normally used to display an overlay over the current UI while the system is
    /// waiting for a control to be actuated.
    /// </summary>
    /// <remarks>
    /// If neither <see cref="rebindPrompt"/> nor <c>rebindOverlay</c> is set, the component will temporarily
    /// replaced the <see cref="bindingText"/> (if not <c>null</c>) with <c>"Waiting..."</c>.
    /// </remarks>
    /// <seealso cref="startRebindEvent"/>
    /// <seealso cref="rebindPrompt"/>
    public GameObject rebindOverlay
    {
        get => m_RebindOverlay;
        set => m_RebindOverlay = value;
    }

    /// <summary>
    /// Event that is triggered every time the UI updates to reflect the current binding.
    /// This can be used to tie custom visualizations to bindings.
    /// </summary>
    public UpdateBindingUIEvent updateBindingUIEvent
    {
        get
        {
            if (m_UpdateBindingUIEvent == null)
                m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
            return m_UpdateBindingUIEvent;
        }
    }

    /// <summary>
    /// Event that is triggered when an interactive rebind is started on the action.
    /// </summary>
    public InteractiveRebindEvent startRebindEvent
    {
        get
        {
            if (m_RebindStartEvent == null)
                m_RebindStartEvent = new InteractiveRebindEvent();
            return m_RebindStartEvent;
        }
    }

    /// <summary>
    /// Event that is triggered when an interactive rebind has been completed or canceled.
    /// </summary>
    public InteractiveRebindEvent stopRebindEvent
    {
        get
        {
            if (m_RebindStopEvent == null)
                m_RebindStopEvent = new InteractiveRebindEvent();
            return m_RebindStopEvent;
        }
    }

    /// <summary>
    /// Event that is triggered when an interactive rebind resulted in an actual binding change.
    /// </summary>
    public InteractiveRebindEvent rebindAppliedEvent
    {
        get
        {
            if (m_RebindAppliedEvent == null)
                m_RebindAppliedEvent = new InteractiveRebindEvent();
            return m_RebindAppliedEvent;
        }
    }

    /// <summary>
    /// Event that is triggered when no binding change was made (e.g. canceled or no-op completion).
    /// </summary>
    public InteractiveRebindEvent rebindNotAppliedEvent
    {
        get
        {
            if (m_RebindNotAppliedEvent == null)
                m_RebindNotAppliedEvent = new InteractiveRebindEvent();
            return m_RebindNotAppliedEvent;
        }
    }

    /// <summary>
    /// When an interactive rebind is in progress, this is the rebind operation controller.
    /// Otherwise, it is <c>null</c>.
    /// </summary>
    public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

    /// <summary>
    /// Return the action and binding index for the binding that is targeted by the component
    /// according to
    /// </summary>
    /// <param name="action"></param>
    /// <param name="bindingIndex"></param>
    /// <returns></returns>
    public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
    {
        bindingIndex = -1;

        action = m_Action?.action;
        if (action == null)
            return false;

        if (string.IsNullOrEmpty(m_BindingId))
            return false;

        // Look up binding index.
        var bindingId = new Guid(m_BindingId);
        bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
        if (bindingIndex == -1)
        {
            Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Trigger a refresh of the currently displayed binding.
    /// </summary>
    public void UpdateBindingDisplay()
    {
        var displayString = string.Empty;
        var deviceLayoutName = default(string);
        var controlPath = default(string);

        // Get display string from action.
        var action = m_Action?.action;
        if (action != null)
        {
            var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
            if (bindingIndex != -1)
                displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath);
        }

        //Update action binding icon
        if (EditorApplication.isPlaying)
            m_ActionBindingIcon.SetAction(deviceLayoutName, controlPath);

        // Give listeners a chance to configure UI in response.
        m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
    }

    /// <summary>
    /// Remove currently applied binding overrides.
    /// </summary>
    public void ResetToDefault()
    {
        if (!ResolveActionAndBinding(out var action, out var bindingIndex))
            return;

        if (action.bindings[bindingIndex].isComposite)
        {
            // It's a composite. Remove overrides from part bindings.
            for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                action.RemoveBindingOverride(i);
        }
        else
        {
            action.RemoveBindingOverride(bindingIndex);
        }
        UpdateBindingDisplay();
    }

    /// <summary>
    /// Initiate an interactive rebind that lets the player actuate a control to choose a new binding
    /// for the action.
    /// </summary>
    public void StartInteractiveRebind()
    {
        if (!ResolveActionAndBinding(out var action, out var bindingIndex))
            return;

        // If the binding is a composite, we need to rebind each part in turn.
        if (action.bindings[bindingIndex].isComposite)
        {
            var firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
        }
        else
        {
            PerformInteractiveRebind(action, bindingIndex);
        }
    }

    private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
    {
        m_RebindOperation?.Cancel(); // Will null out m_RebindOperation.

        var originalOverridePath = action.bindings[bindingIndex].overridePath;
        var originalOverrideInteractions = action.bindings[bindingIndex].overrideInteractions;
        var originalOverrideProcessors = action.bindings[bindingIndex].overrideProcessors;
        var originalEffectivePath = action.bindings[bindingIndex].effectivePath;

        void CleanUp()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;
            action.Enable();

            // Re-enable UI controls
            var uiControls = action.actionMap.asset.FindActionMap("UIControls");
            uiControls?.Enable();
            
            if (action.bindings[bindingIndex].groups.Contains("Keyboard"))
            {
                EnableControlScheme("Gamepad");
            } else if (action.bindings[bindingIndex].groups.Contains("Gamepad"))
            {
                EnableControlScheme("Keyboard");
            }
        }
        // Disable UI controls during rebinding
        var uiControlsMap = action.actionMap.asset.FindActionMap("UIControls");
        uiControlsMap?.Disable();

        action.Disable();
        
        if (action.bindings[bindingIndex].groups.Contains("Keyboard")) {
            DisableControlScheme("Gamepad");
        } else if(action.bindings[bindingIndex].groups.Contains("Gamepad")) {
            DisableControlScheme("Keyboard");
        }

        // Configure the rebind.
        m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithTimeout(5.0f)
            .OnCancel(
                operation =>
                {
                    m_RebindNotAppliedEvent?.Invoke(this, operation);
                    m_RebindStopEvent?.Invoke(this, operation);
                    m_RebindOverlay?.SetActive(false);
                    UpdateBindingDisplay();
                    CleanUp();
                })
            .OnComplete(
                operation =>
                {
                    if (!IsIncludedControl(bindingIndex))
                        RestoreBindingOverride(action, bindingIndex, originalOverridePath, originalOverrideInteractions,
                            originalOverrideProcessors);

                    var bindingChanged = action.bindings[bindingIndex].effectivePath != originalEffectivePath;
                    if (bindingChanged)
                    {
                        m_RebindAppliedEvent?.Invoke(this, operation);
                        
                        // Mirror keyboard directional bindings to UIControls
                        MirrorDirectionalBindingToUIControls(action, bindingIndex);
                        
                        // Mirror Jump and Shoot bindings to UIControls Confirm and Cancel
                        MirrorActionBindingToUIControls(action, bindingIndex);
                    }
                    else
                        m_RebindNotAppliedEvent?.Invoke(this, operation);

                    m_RebindOverlay?.SetActive(false);
                    m_RebindStopEvent?.Invoke(this, operation);
                    UpdateBindingDisplay();
                    CleanUp();

                    // If there's more composite parts we should bind, initiate a rebind
                    // for the next part.
                    if (allCompositeParts)
                    {
                        var nextBindingIndex = bindingIndex + 1;
                        if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                            PerformInteractiveRebind(action, nextBindingIndex, true);
                    }
                });

        // Bring up rebind overlay, if we have one.
        m_RebindOverlay?.SetActive(true);

        // Give listeners a chance to act on the rebind starting.
        m_RebindStartEvent?.Invoke(this, m_RebindOperation);

        m_RebindOperation.Start();
    }

    private void MirrorDirectionalBindingToUIControls(InputAction sourceAction, int sourceBindingIndex)
    {
        var sourceBinding = sourceAction.bindings[sourceBindingIndex];
        
        // Only mirror keyboard directional bindings
        if (!sourceBinding.groups.Contains("Keyboard"))
            return;
        
        // Only mirror if it's part of a composite
        if (!sourceBinding.isPartOfComposite)
            return;
        
        // Only mirror directional parts (up, down, left, right)
        var partName = sourceBinding.name;
        if (partName != "up" && partName != "down" && partName != "left" && partName != "right")
            return;
        
        // Only mirror from PlayerControls/Movement action
        if (sourceAction.actionMap.name != "PlayerControls" || sourceAction.name != "Movement")
            return;
        
        // Find the UIControls action map
        var uiControlsMap = sourceAction.actionMap.asset.FindActionMap("UIControls");
        if (uiControlsMap == null)
        {
            Debug.LogWarning("UIControls action map not found, cannot mirror directional binding.");
            return;
        }
        
        // Find the Move action in UIControls
        var moveAction = uiControlsMap.FindAction("Move");
        if (moveAction == null)
        {
            Debug.LogWarning("Move action not found in UIControls, cannot mirror directional binding.");
            return;
        }
        
        // Find the matching directional binding in UIControls/Move
        var targetBindingIndex = -1;
        for (int i = 0; i < moveAction.bindings.Count; i++)
        {
            var binding = moveAction.bindings[i];
            if (binding.isPartOfComposite && 
                binding.name == partName && 
                binding.groups.Contains("Keyboard"))
            {
                targetBindingIndex = i;
                break;
            }
        }
        
        if (targetBindingIndex == -1)
        {
            Debug.LogWarning($"Matching {partName} binding not found in UIControls/Move, cannot mirror directional binding.");
            return;
        }
        
        // Apply the same override to the UIControls binding
        var newOverridePath = sourceBinding.overridePath;
        if (!string.IsNullOrEmpty(newOverridePath))
        {
            moveAction.ApplyBindingOverride(targetBindingIndex, new InputBinding
            {
                overridePath = newOverridePath
            });
            //Debug.Log($"Mirrored {partName} binding to UIControls/Move: {newOverridePath}");
        }
    }

    private void MirrorActionBindingToUIControls(InputAction sourceAction, int sourceBindingIndex)
    {
        var sourceBinding = sourceAction.bindings[sourceBindingIndex];
        
        // Only mirror keyboard bindings
        if (!sourceBinding.groups.Contains("Keyboard"))
            return;
        
        // Only mirror non-composite bindings (Jump and Shoot are simple bindings)
        if (sourceBinding.isPartOfComposite || sourceBinding.isComposite)
            return;
        
        // Only mirror from PlayerControls action map
        if (sourceAction.actionMap.name != "PlayerControls")
            return;
        
        // Determine which action to mirror to based on source action
        string targetActionName = null;
        if (sourceAction.name == "Jump")
            targetActionName = "Confirm";
        else if (sourceAction.name == "Shoot")
            targetActionName = "Cancel";
        else
            return; // Not an action we want to mirror
        
        // Find the UIControls action map
        var uiControlsMap = sourceAction.actionMap.asset.FindActionMap("UIControls");
        if (uiControlsMap == null)
        {
            Debug.LogWarning("UIControls action map not found, cannot mirror action binding.");
            return;
        }
        
        // Find the target action in UIControls
        var targetAction = uiControlsMap.FindAction(targetActionName);
        if (targetAction == null)
        {
            Debug.LogWarning($"{targetActionName} action not found in UIControls, cannot mirror action binding.");
            return;
        }
        
        // Find the keyboard binding that is NOT Enter (for Confirm) or Escape (for Cancel)
        // We need to find the binding that matches the default key (z for Jump/Confirm, x for Shoot/Cancel)
        var targetBindingIndex = -1;
        for (int i = 0; i < targetAction.bindings.Count; i++)
        {
            var binding = targetAction.bindings[i];
            
            // Skip composite bindings
            if (binding.isComposite || binding.isPartOfComposite)
                continue;
            
            // Must be keyboard binding
            if (!binding.groups.Contains("Keyboard"))
                continue;
            
            // Skip Enter key for Confirm
            if (targetActionName == "Confirm" && binding.path == "<Keyboard>/enter")
                continue;
            
            // Skip Escape key for Cancel
            if (targetActionName == "Cancel" && binding.path == "<Keyboard>/escape")
                continue;
            
            // This is the binding we want to update
            targetBindingIndex = i;
            break;
        }
        
        if (targetBindingIndex == -1)
        {
            Debug.LogWarning($"Matching keyboard binding not found in UIControls/{targetActionName}, cannot mirror action binding.");
            return;
        }
        
        // Apply the same override to the UIControls binding
        var newOverridePath = sourceBinding.overridePath;
        if (!string.IsNullOrEmpty(newOverridePath))
        {
            targetAction.ApplyBindingOverride(targetBindingIndex, new InputBinding
            {
                overridePath = newOverridePath
            });
            //Debug.Log($"Mirrored {sourceAction.name} binding to UIControls/{targetActionName}: {newOverridePath}");
        }
    }

    private bool IsIncludedControl(int bindingIndex)
    {
        var deviceLayoutName = default(string);
        var controlPath = default(string);
        m_Action.action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath);

        if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Keyboard"))
        {
            return ALLOWED_KEYBOARD_KEYS.Contains(controlPath);
        }
        else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
        {
            return ALLOWED_GAMEPAD_BUTTONS.Contains(controlPath);
        }

        return false;
    }

    private static void RestoreBindingOverride(InputAction action, int bindingIndex, string overridePath,
        string overrideInteractions, string overrideProcessors)
    {
        if (string.IsNullOrEmpty(overridePath) && string.IsNullOrEmpty(overrideInteractions) &&
            string.IsNullOrEmpty(overrideProcessors))
        {
            action.RemoveBindingOverride(bindingIndex);
            return;
        }

        action.ApplyBindingOverride(bindingIndex, new InputBinding
        {
            overridePath = overridePath,
            overrideInteractions = overrideInteractions,
            overrideProcessors = overrideProcessors
        });
    }

    private void DisableControlScheme(string bindingGroup)
    {
        List<InputAction> actions = GetActionsForControlScheme(bindingGroup);
        foreach (var action in actions)
        {
            action.Disable();
        }
    }
    private void EnableControlScheme(string schemeName)
    {
        List<InputAction> actions = GetActionsForControlScheme(schemeName);
        foreach (var action in actions)
        {
            action.Enable();
        }
    }

    private List<InputAction> GetActionsForControlScheme(string bindingGroup)
    {
        var actions = new List<InputAction>();
        foreach (var actionMap in m_Action.action.actionMap.asset.actionMaps)
        {
            foreach (var action in actionMap.actions)
            {
                var bindings = action.bindings;
                for (int i = 0; i < bindings.Count; i++)
                {
                    if (bindings[i].groups.Contains(bindingGroup))
                    {
                        actions.Add(action);
                        break;
                    }
                }
            }
        }
        return actions;
    }


    protected void OnEnable()
    {
        if (s_RebindActionUIs == null)
            s_RebindActionUIs = new List<RebindActionUI>();
        s_RebindActionUIs.Add(this);
        if (s_RebindActionUIs.Count == 1)
            InputSystem.onActionChange += OnActionChange;
    }

    protected void OnDisable()
    {
        m_RebindOperation?.Dispose();
        m_RebindOperation = null;

        s_RebindActionUIs.Remove(this);
        if (s_RebindActionUIs.Count == 0)
        {
            s_RebindActionUIs = null;
            InputSystem.onActionChange -= OnActionChange;
        }
    }

    // When the action system re-resolves bindings, we want to update our UI in response. While this will
    // also trigger from changes we made ourselves, it ensures that we react to changes made elsewhere. If
    // the user changes keyboard layout, for example, we will get a BoundControlsChanged notification and
    // will update our UI to reflect the current keyboard layout.
    private static void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.BoundControlsChanged)
            return;

        var action = obj as InputAction;
        var actionMap = action?.actionMap ?? obj as InputActionMap;
        var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

        for (var i = 0; i < s_RebindActionUIs.Count; ++i)
        {
            var component = s_RebindActionUIs[i];
            var referencedAction = component.actionReference?.action;
            if (referencedAction == null)
                continue;

            if (referencedAction == action ||
                referencedAction.actionMap == actionMap ||
                referencedAction.actionMap?.asset == actionAsset)
                component.UpdateBindingDisplay();
        }
    }

    [Tooltip("Reference to action that is to be rebound from the UI.")]
    [SerializeField]
    private InputActionReference m_Action;

    [SerializeField]
    private InputIconTMP m_ActionBindingIcon;

    [SerializeField]
    private string m_BindingId;

    [Tooltip("Optional UI that will be shown while a rebind is in progress.")]
    [SerializeField]
    private GameObject m_RebindOverlay;

    [Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying "
        + "bindings in custom ways, e.g. using images instead of text.")]
    [SerializeField]
    private UpdateBindingUIEvent m_UpdateBindingUIEvent;

    [Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, "
        + "to implement custom UI behavior while a rebind is in progress. It can also be used to further "
        + "customize the rebind.")]
    [SerializeField]
    private InteractiveRebindEvent m_RebindStartEvent;

    [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
    [SerializeField]
    private InteractiveRebindEvent m_RebindStopEvent;

    [Tooltip("Event that is triggered when an interactive rebind resulted in an actual binding change.")]
    [SerializeField]
    private InteractiveRebindEvent m_RebindAppliedEvent;

    [Tooltip("Event that is triggered when no binding change was made (e.g. canceled or no-op completion).")]
    [SerializeField]
    private InteractiveRebindEvent m_RebindNotAppliedEvent;

    //Contains InputSystem's own names for keys
    private static readonly HashSet<string> ALLOWED_KEYBOARD_KEYS = new HashSet<string>
        {
            "a", 
            "b", 
            "c",
            "d",
            "e",
            "f",
            "g",
            "h",
            "i",
            "j",
            "k",
            "l",
            "m",
            "n",
            "o",
            "p",
            "q",
            "r",
            "s",
            "t",
            "u",
            "v",
            "w",
            "x",
            "y",
            "z",
            "tab",
            "leftShift",
            "rightShift",
            "leftCtrl",
            "rightCtrl",
            "leftAlt",
            "rightAlt",
            "leftMeta",
            "rightMeta",
            "capsLock",
            "backspace",
            "backquote",
            "upArrow",
            "rightArrow",
            "leftArrow",
            "downArrow",
            "space",
            "insert",
            "delete",
            "home",
            "end",
            "pageUp",
            "pageDown",
            "slash",
            "asterisk",
            "minus",
            "plus",
            "equals",
            "graveAccent",
            "leftBracket",
            "rightBracket",
            "backslash",
            "semicolon",
            "quote",
            "comma",
            "period",
            "exclamationMark",
            "at",
            "numberSign",
            "dollarSign",
            "percentSign",
            "circumflexAccent",
            "ampersand",
            "leftParenthesis",
            "rightParenthesis",
            "lowLine",
            "colon",
            "doubleQuotation",
            "lessThan",
            "greaterThan",
            "tilde",
            "leftBracket",
            "rightBracket",
            "questionMark",
            "verticalBar",
            "f1",
            "f2",
            "f3",
            "f4",
            "f5",
            "f6",
            "f7",
            "f8",
            "f9",
            "f10",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"
        };

    //Contains InputSystem's own names for gamepad
    private static readonly HashSet<string> ALLOWED_GAMEPAD_BUTTONS = new HashSet<string>
        {
            "buttonWest", 
            "buttonEast", 
            "buttonNorth", 
            "buttonSouth",
            "leftTrigger",
            "rightTrigger",
            "leftShoulder",
            "rightShoulder"
        };

    private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

    private static List<RebindActionUI> s_RebindActionUIs;

    // We want the label for the action name to update in edit mode, too, so
    // we kick that off from here.
    #if UNITY_EDITOR
    protected void OnValidate()
    {
        UpdateBindingDisplay();
    }

    #endif

    [Serializable]
    public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string>
    {
    }

    [Serializable]
    public class InteractiveRebindEvent : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation>
    {
    }
}
