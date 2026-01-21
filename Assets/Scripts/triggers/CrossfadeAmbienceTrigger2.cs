using System.Collections;
using UnityEngine;

public class CrossfadeAmbienceTrigger2 : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _otherAmbienceTrigger;

    void OnTriggerEnter2D(Collider2D other) {
        if(GameManager.obj.CapePicked) {
            return;
        }
        if(other.CompareTag("Player")) {
            StartCoroutine(CrossFadeAmbience());
            _otherAmbienceTrigger.enabled = true;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator CrossFadeAmbience() {
        AmbienceManager.obj.PlayCaveAmbience();
        AmbienceManager.obj.FadeInAmbienceSource1(1.5f);
        yield return new WaitForSeconds(1f);
        AmbienceManager.obj.FadeOutAmbienceSource2And3(1f);
    }
}
