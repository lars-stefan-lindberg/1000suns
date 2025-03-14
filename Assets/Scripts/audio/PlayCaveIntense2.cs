using UnityEngine;

public class PlayCaveIntense2 : MonoBehaviour
{
    public void PlayCaveIntense2Song() {
        if(!MusicManager.obj.IsPlaying()) {
            MusicManager.obj.PlayCaveIntense2();
        }
    }
}
