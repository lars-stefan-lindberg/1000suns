using UnityEngine;

public class SwitchToMusicOnlyTrigger : MonoBehaviour
{
    [SerializeField] private MusicTrack _musicTrack;
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            FadeOutAmbienceAndStartMusic();
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void FadeOutAmbienceAndStartMusic() {
        MusicManager.obj.Play(_musicTrack);
        AmbienceManager.obj.Stop();
    }
}
