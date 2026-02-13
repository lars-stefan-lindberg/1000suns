using System.Collections;
using UnityEngine;

public class FadeOutMusicTriggerPrefab : MonoBehaviour
{
    [SerializeField] float _fadeOutDuration = 3f;
    
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
