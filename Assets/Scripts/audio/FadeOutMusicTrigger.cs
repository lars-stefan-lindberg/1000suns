using UnityEngine;

public class FadeOutMusicTrigger : MonoBehaviour
{
    [SerializeField] private AmbienceTrack _capeRoomAmbience;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            FadeOutAndStopMusic();
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void FadeOutAndStopMusic() {
        MusicManager.obj.Stop();
        AmbienceManager.obj.Play(_capeRoomAmbience);
    }
}
