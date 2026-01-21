using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstCaveMiniBossManager : MonoBehaviour
{
    void Awake()
    {
        if(GameManager.obj.FirstCaveMiniBossKilled) {
            Destroy(gameObject);
        }
    }
    
    public void PlayCaveIntense1Outro() {
        MusicManager.obj.ScheduleClipOnNextBar(MusicManager.obj.caveIntense1Outro, 210, false);
        GameManager.obj.FirstCaveMiniBossKilled = true;
        MusicManager.obj.SetCurrentMusicId(MusicManager.MusicId.None);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
