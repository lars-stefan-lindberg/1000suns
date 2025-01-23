using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CustomUIInputHandler : MonoBehaviour
{
    public InputActionAsset inputActions;
    private InputAction resetBinding;

    private void OnEnable()
    {
        // Get the custom action from the action map
        var actionMap = inputActions.FindActionMap("UIControls"); // Replace with your Action Map's name
        resetBinding = actionMap.FindAction("ResetBinding"); // Replace with your action's name

        if (resetBinding != null)
        {
            resetBinding.performed += OnResetBindingUIAction;
            resetBinding.Enable();
        }
    }

    private void OnDisable()
    {
        if (resetBinding != null)
        {
            resetBinding.performed -= OnResetBindingUIAction;
            resetBinding.Disable();
        }
    }

    private void OnResetBindingUIAction(InputAction.CallbackContext context)
    {
        PerformResetBindingAction();
    }

    private void PerformResetBindingAction()
    {
        GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        RebindUIElement rebindUIElement = currentSelectedGameObject.GetComponentInParent<RebindUIElement>();
        if(rebindUIElement != null) {
            Button[] buttons = rebindUIElement.gameObject.GetComponentsInChildren<Button>();
            buttons[1].onClick.Invoke();
        }
    }
}
