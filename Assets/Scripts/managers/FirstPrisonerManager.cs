using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstPrisonerManager : MonoBehaviour
{
    public void PlayCaveIntense1Outro() {
        MusicManager.obj.EndCurrentTrack();
        GameManager.obj.FirstPrisonerKilled = true;
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
