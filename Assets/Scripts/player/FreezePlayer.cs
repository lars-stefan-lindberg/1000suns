using System.Collections;
using UnityEngine;

public class FreezePlayer : MonoBehaviour
{
    private bool _hasBeenTriggered = false;

    void Awake() {
        if(GameManager.obj.FirstPowerUpPicked) {
            _hasBeenTriggered = true;
            Destroy(gameObject, 3);
        }
    }

     void OnTriggerEnter2D(Collider2D other) {
        if(_hasBeenTriggered)
            return;
        if(other.CompareTag("Player")) {
            StartCoroutine(FreezeAndPlayIntro());
            _hasBeenTriggered = true;
        }
    }

    private IEnumerator FreezeAndPlayIntro() {
        PlayerMovement.obj.Freeze(6f);
        MusicManager.obj.PlayPowerUpIntroSong();
        Destroy(gameObject);
        yield return null;
    }
}
