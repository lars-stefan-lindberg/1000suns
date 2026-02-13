using System.Collections;
using UnityEngine;

public class SwitchToAmbienceOnlyTrigger : MonoBehaviour
{
    [SerializeField] private AmbienceTrack _caveMainAmbience;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            FadeOutMusicAndStartAmbience();
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void FadeOutMusicAndStartAmbience() {
        MusicManager.obj.Stop();
        AmbienceManager.obj.Play(_caveMainAmbience);
    }
}
