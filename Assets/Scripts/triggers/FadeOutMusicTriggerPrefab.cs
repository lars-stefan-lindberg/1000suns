using System.Collections;
using UnityEngine;

public class FadeOutMusicTriggerPrefab : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            FadeOutAndStopMusic();
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void FadeOutAndStopMusic() {
        MusicManager.obj.Stop();
    }
}
