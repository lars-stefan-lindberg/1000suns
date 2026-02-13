using UnityEngine;

public class FadeOutAmbienceTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            AmbienceManager.obj.Stop();
        }
    }
}
