using UnityEngine;

public class PlayCaveIntense2 : MonoBehaviour
{
    [SerializeField] private MusicTrack _musicTrack;
    public void PlayCaveIntense2Song() {
        MusicManager.obj.Play(_musicTrack);
    }
}
