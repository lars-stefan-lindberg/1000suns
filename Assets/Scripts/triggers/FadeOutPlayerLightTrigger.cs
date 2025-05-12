using UnityEngine;

public class FadeOutPlayerLightTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            PlayerLightManager.obj.FadeOut();
        }
    }
}
