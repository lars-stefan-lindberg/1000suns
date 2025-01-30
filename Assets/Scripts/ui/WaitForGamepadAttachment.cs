using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WaitForGamepadAttachment : MonoBehaviour
{
    [SerializeField] private GameObject _configMenu;
    [SerializeField] private GameObject _firstInteractibleElement;
    [SerializeField] private GameObject _attachGamepad;
    private bool isGamepadAttached = false;

    void Update() {
        if(Gamepad.current != null) {
            if(!isGamepadAttached) {
                _configMenu.SetActive(true);
                EventSystem.current.SetSelectedGameObject(_firstInteractibleElement);
                _attachGamepad.SetActive(false);
            }
            isGamepadAttached = true;
        } else {
            isGamepadAttached = false;
            _configMenu.SetActive(false);
            _attachGamepad.SetActive(true);
        }
    }
}
