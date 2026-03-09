using UnityEngine.Events;

public class SelectSaveFileScreen : UIScreen
{
    public UnityEvent OnBack;

    public void Back() {
        UISoundPlayer.obj.PlayBack();
        OnBack?.Invoke();
    }

    public void OnSaveProfileSelected(int saveSlotId) {
        SaveManager.obj.SetActiveSaveProfile(saveSlotId);
        
        if (SaveManager.obj.HasValidSave(saveSlotId)) {
            MainMenuManager.obj.ContinueGame();
        } else {
            MainMenuManager.obj.StartGame();
        }
    }
}
