using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ControllerScreen : UIScreen
{
    [SerializeField] private InputActionAsset actions;
    [SerializeField] private GameObject _rebindMenu;
    [SerializeField] private GameObject _noControllerInfo;
    [SerializeField] private GameObject _firstRebindableButton;
    [SerializeField] private GameObject _saveButton;
    public UnityEvent OnBack;

    private bool _isRebindMenuShown = false;
    private bool _isNoControllerInfoShown = false;

    void Start() {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
    }

    void OnEnable()
    {
        if(Gamepad.current != null) {
            ShowRebindMenu();
        } else {
            ShowNoControllerInfo();
            SetBackSelectable(_saveButton);
        }
    }

    public void Save() {
        if(Gamepad.current != null) {
            var rebinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
        }
        UISoundPlayer.obj.PlaySelect();
        OnBack?.Invoke();
    }

    void Update()
    {
        if(Gamepad.current != null) {
            if(!_isRebindMenuShown) {
                ShowRebindMenu();
                EventSystem.current.SetSelectedGameObject(_firstRebindableButton);
            }
        } else {
            if(!_isNoControllerInfoShown) {
                ShowNoControllerInfo();
                EventSystem.current.SetSelectedGameObject(_saveButton);
            }
        }
    }

    private void ShowRebindMenu() {
        _noControllerInfo.SetActive(false);
        _rebindMenu.SetActive(true);
        _isRebindMenuShown = true;
        _isNoControllerInfoShown = false;
    }

    private void ShowNoControllerInfo() {
        _rebindMenu.SetActive(false);
        _noControllerInfo.SetActive(true);
        _isNoControllerInfoShown = true;
        _isRebindMenuShown = false;
    }
}
