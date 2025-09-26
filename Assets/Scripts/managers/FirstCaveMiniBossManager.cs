using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstCaveMiniBossManager : MonoBehaviour
{
    void Awake()
    {
        if(GameEventManager.obj.FirstCaveMiniBossKilled) {
            Destroy(gameObject);
        }
    }
    
    public void PlayCaveIntense1Outro() {
        MusicManager.obj.ScheduleClipOnNextBar(MusicManager.obj.caveIntense1Outro, 210, false);
        GameEventManager.obj.FirstCaveMiniBossKilled = true;
        MusicManager.obj.SetCurrentMusicId(MusicManager.MusicId.None);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
