using System.Collections;
using UnityEngine;

public class SetCanFallDashTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialCanvas;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            Player.obj.CanFallDash = true;
            StartCoroutine(MaybeTutorial());
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator MaybeTutorial() {
        if(!GameEventManager.obj.HasSeenForcePushDashTutorial) {
            TutorialFooterManager.obj.StartFadeIn();
            GameEventManager.obj.HasSeenForcePushDashTutorial = true;
        }
        yield return null;
    }
}
