using UnityEngine;

public class FirstPrisonerManager : MonoBehaviour
{
    public void PlayCaveIntense1Outro() {
        MusicManager.obj.ScheduleClipOnNextBar(MusicManager.obj.caveIntense1Outro, 210, false);
        GameEventManager.obj.FirstPrisonerKilled = true;
    }
}
