using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CustomUIInputHandler : MonoBehaviour
{
    public InputActionAsset inputActions;
    public UnityEvent onCancel;
    private InputAction resetBinding;
    private InputAction cancelBinding;

    private void OnEnable()
    {
        var actionMap = inputActions.FindActionMap("UIControls");
        resetBinding = actionMap.FindAction("ResetBinding");
        cancelBinding = actionMap.FindAction("Cancel");

        if (resetBinding != null)
        {
            resetBinding.performed += OnResetBindingUIAction;
            resetBinding.Enable();
        }

        if(cancelBinding != null) {
            cancelBinding.performed += OnCancelBindingAction;
            cancelBinding.Enable();
        }
    }

    private void OnDisable()
    {
        if (resetBinding != null)
        {
            resetBinding.performed -= OnResetBindingUIAction;
            resetBinding.Disable();
        }
        if (cancelBinding != null)
        {
            cancelBinding.performed -= OnCancelBindingAction;
            cancelBinding.Disable();
        }
    }

    private void OnCancelBindingAction(InputAction.CallbackContext context) {
        if(context.performed)
            PerformCancelBindingAction();
    }

    private void PerformCancelBindingAction() {
        onCancel.Invoke();
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

    public void OnRebindCancelInProgress() {
        if (cancelBinding != null)
        {
            cancelBinding.performed -= OnCancelBindingAction;
            cancelBinding.Disable();
        }
    }

    public void OnRebindCancelFinished() {
        StartCoroutine(DelayedEnableCancelButton());
    }

    private IEnumerator DelayedEnableCancelButton() {
        yield return new WaitForSeconds(0.1f);
        if (cancelBinding != null)
        {
            cancelBinding.performed += OnCancelBindingAction;
            cancelBinding.Enable();
        }
    }
}
