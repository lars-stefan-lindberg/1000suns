using System.Collections;
using UnityEngine;

public class SwitchToSecondCaveSongOnlyTrigger : MonoBehaviour
{
    [SerializeField] private MusicTrack _musicTrack;

    void Awake() {
        if(GameManager.obj.AfterPowerUpRoomsCompletedWallBreak) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
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
