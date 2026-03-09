using UnityEngine.Events;

public class OptionsScreen : UIScreen
{
    public UnityEvent OnBack;

    public void OnGameOptionsClicked() {
    }

    public void OnAudioOptionsClicked() {
    }

    public void OnControllerOptionsClicked() {
    }

    public void OnKeyboardOptionsClicked() {
    }

    public void OnBackButtonClicked() {
        UISoundPlayer.obj.PlayBack();
        OnBack?.Invoke();
    }
}
