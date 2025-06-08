using System.Collections;
using UnityEngine;

public class PlayCapeRoomAmbienceTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            StartCoroutine(PlayCapeRoomAmbience());
        }
    }

    private IEnumerator PlayCapeRoomAmbience() {
        if(!AmbienceManager.obj.IsAmbienceSource2Playing()) {
            AmbienceManager.obj.PlayCapeRoomAmbience();
            AmbienceManager.obj.FadeInAmbienceSource2And3(1.5f);
            yield return new WaitForSeconds(1f);
            if(AmbienceManager.obj.IsAmbienceSource1Playing()) {
                AmbienceManager.obj.FadeOutAmbienceSource1(1f);
            }
        }
    }
}
