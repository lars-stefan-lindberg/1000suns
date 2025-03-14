using UnityEngine;

public class FirstCaveDoorDestroyedManager : MonoBehaviour
{
    public void PlayCaveIntense1Outro() {
        MusicManager.obj.ScheduleClipOnNextBar(MusicManager.obj.caveIntense1Outro, 210, false);
        GameEventManager.obj.FirstPrisonerKilled = true;
    }
}
