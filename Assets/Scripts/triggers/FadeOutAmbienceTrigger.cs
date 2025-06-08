using UnityEngine;

public class FadeOutAmbienceTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            if(AmbienceManager.obj.IsAmbienceSource1Playing()) {
                AmbienceManager.obj.FadeOutAmbienceSource1(1f);
            }
        }
    }
}
