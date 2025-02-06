using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CustomUIInputHandlerCreditsScreen : MonoBehaviour
{
    public InputActionAsset inputActions;
    public UnityEvent onScrollPerformed;
    public UnityEvent onScrollCanceled;
    private InputAction scrollBinding;

    private void OnEnable()
    {
        var actionMap = inputActions.FindActionMap("UIControls");
        scrollBinding = actionMap.FindAction("CreditsScroll");

        if (scrollBinding != null)
        {
            scrollBinding.performed += OnScrollBindingUIActionPerformed;
            scrollBinding.canceled += OnScrollBindingUIActionCanceled;
            scrollBinding.Enable();
        }
    }

    private void OnDisable()
    {
        if (scrollBinding != null)
        {
            scrollBinding.performed -= OnScrollBindingUIActionPerformed;
            scrollBinding.canceled -= OnScrollBindingUIActionCanceled;
            scrollBinding.Disable();
        }
    }

    private void OnScrollBindingUIActionPerformed(InputAction.CallbackContext context)
    {
        PerformScrollBindingActionPerformed();
    }

    private void PerformScrollBindingActionPerformed()
    {
        onScrollPerformed.Invoke();
    }

    private void OnScrollBindingUIActionCanceled(InputAction.CallbackContext context)
    {
        PerformScrollBindingActionCanceled();
    }

    private void PerformScrollBindingActionCanceled()
    {
        onScrollCanceled.Invoke();
    }
}
