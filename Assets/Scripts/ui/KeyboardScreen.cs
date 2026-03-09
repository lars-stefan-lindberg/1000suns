using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class KeyboardScreen : UIScreen
{
    [SerializeField] private InputActionAsset actions;

    public UnityEvent OnBack;

    void Start() {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
    }

    public void Save() {
        if(Keyboard.current != null) {
            var rebinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
        }
        UISoundPlayer.obj.PlaySelect();
        OnBack?.Invoke();
    }
}
