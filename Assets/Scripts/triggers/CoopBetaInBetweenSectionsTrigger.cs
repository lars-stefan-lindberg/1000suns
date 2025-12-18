using UnityEngine;

public class CoopBetaInBetweenSectionsTrigger : MonoBehaviour
{
void OnTriggerEnter2D(Collider2D col) {
        if(col.CompareTag("Player")) {
            SoundFXManager.obj.PlayEarthquake();
            CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 4.9f);
            transform.GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
