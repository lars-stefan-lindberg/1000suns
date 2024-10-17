using UnityEngine;

public class RaiseMusicTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            StartCoroutine(SoundMixerManager.obj.StartMusicVolumeRaise(5f));
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
