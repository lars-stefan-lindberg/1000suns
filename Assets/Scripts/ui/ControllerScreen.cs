using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using DG.Tweening;

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
        LoadRebinds();
    }

    void OnEnable()
    {
        LoadRebinds();
        if(Gamepad.current != null) {
            ShowRebindMenu();
        } else {
            ShowNoControllerInfo();
            SetBackSelectable(_saveButton);
        }
    }

    private void LoadRebinds() {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
    }

    public override Tween Hide()
    {
        // Auto-save rebindings when hiding the screen (whether via Save button or global back button)
        SaveRebindings();
        return base.Hide();
    }

    public void Save() {
        SaveRebindings();
        UISoundPlayer.obj.PlaySelect();
        OnBack?.Invoke();
    }
    
    private void SaveRebindings() {
        if(Gamepad.current != null) {
            var rebinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
            // Immediately reload the bindings to ensure they take effect
            actions.LoadBindingOverridesFromJson(rebinds);
            
            // Apply rebindings to all active PlayerInput instances (they use cloned InputActionAssets)
            if(PlayerSwitcher.obj != null) {
                ApplyRebindsToPlayerInput(PlayerSwitcher.obj.eliInput, rebinds);
                ApplyRebindsToPlayerInput(PlayerSwitcher.obj.deeInput, rebinds);
                ApplyRebindsToPlayerInput(PlayerSwitcher.obj.blobInput, rebinds);
            }
        }
    }
    
    private void ApplyRebindsToPlayerInput(UnityEngine.InputSystem.PlayerInput playerInput, string rebinds) {
        if(playerInput != null && playerInput.actions != null) {
            playerInput.actions.LoadBindingOverridesFromJson(rebinds);
        }
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
