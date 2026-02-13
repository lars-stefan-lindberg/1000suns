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
        MusicManager.obj.EndCurrentTrack();
        GameManager.obj.FirstCaveMiniBossKilled = true;
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
