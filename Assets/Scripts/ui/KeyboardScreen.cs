using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using DG.Tweening;

public class KeyboardScreen : UIScreen
{
    [SerializeField] private InputActionAsset actions;

    public UnityEvent OnBack;

    void Start() {
        LoadRebinds();
    }

    void OnEnable() {
        LoadRebinds();
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
        if(Keyboard.current != null) {
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
}
