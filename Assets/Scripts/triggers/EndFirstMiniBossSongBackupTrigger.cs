using UnityEngine;

public class EndFirstMiniBossSongBackupTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            //Check if first mini boss song is still playing due to rare bug. If it is playing, schedule the outro
            if(MusicManager.obj.GetLoopSource() != null && 
                (MusicManager.obj.GetLoopSource().clip == MusicManager.obj.caveIntense2Intro ||
                MusicManager.obj.GetLoopSource().clip == MusicManager.obj.caveIntense2Loop)) {
                MusicManager.obj.ScheduleClipOnNextBar(MusicManager.obj.caveIntense1Outro, 210, false);
            }
        }
    }
}
