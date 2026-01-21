using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstPrisonerManager : MonoBehaviour
{
    public void PlayCaveIntense1Outro() {
        MusicManager.obj.ScheduleClipOnNextBar(MusicManager.obj.caveIntense1Outro, 210, false);
        GameManager.obj.FirstPrisonerKilled = true;
        MusicManager.obj.SetCurrentMusicId(MusicManager.MusicId.None);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
