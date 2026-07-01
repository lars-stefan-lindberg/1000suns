using UnityEngine.Events;
using System.Collections;
using UnityEngine;

public class SelectSaveFileScreen : UIScreen
{
    public UnityEvent OnBack;

    public void Back() {
        UISoundPlayer.obj.PlayBack();
        OnBack?.Invoke();
    }

    public void OnSaveProfileSelected(int saveSlotId) {
        SaveManager.obj.SetActiveSaveProfile(saveSlotId);
        StartCoroutine(CheckSaveAndProceed(saveSlotId));
    }

    private IEnumerator CheckSaveAndProceed(int saveSlotId) {
        var hasValidTask = SaveManager.obj.HasValidSave(saveSlotId);
        while (!hasValidTask.IsCompleted) {
            yield return null;
        }
        
        if (hasValidTask.Result) {
            MainMenuManager.obj.ContinueGame();
        } else {
            MainMenuManager.obj.StartGame();
        }
    }
}
