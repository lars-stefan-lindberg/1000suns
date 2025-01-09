using System.Collections;
using UnityEngine;

public class CrossfadeAmbienceTrigger : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _otherAmbienceTrigger;

    void OnTriggerEnter2D(Collider2D other) {
        if(GameEventManager.obj.CapePicked) {
            return;
        }
        if(other.CompareTag("Player")) {
            StartCoroutine(CrossFadeAmbience());
            _otherAmbienceTrigger.enabled = true;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator CrossFadeAmbience() {
        if(AmbienceManager.obj.IsAmbienceSource1Playing()) {
            AmbienceManager.obj.PlayCapeRoomAmbience();
            AmbienceManager.obj.FadeInAmbienceSource2And3(1.5f);
            yield return new WaitForSeconds(1f);
            AmbienceManager.obj.FadeOutAmbienceSource1(1f);
        } else {
            AmbienceManager.obj.PlayCaveAmbience();
            AmbienceManager.obj.FadeInAmbienceSource1(1.5f);
            yield return new WaitForSeconds(1f);
            AmbienceManager.obj.FadeOutAmbienceSource2And3(1f);
        }
    }
}
