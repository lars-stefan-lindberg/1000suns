using UnityEngine;

public class ForcePushJumpTutorialTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            TutorialFooterManager.obj.StartFadeIn();
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
