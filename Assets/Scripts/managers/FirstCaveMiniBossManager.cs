using UnityEngine;

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
    }
}
