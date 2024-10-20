using System.Collections;
using UnityEngine;

public class ForcePushJumpTutorialTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialCanvas;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            StartCoroutine(ShowTutorial());
        }
    }

    private IEnumerator ShowTutorial() {
        if(!GameEventManager.obj.HasSeenForcePushJumpTutorial) {
            PlayerMovement.obj.Freeze();
            Time.timeScale = 0;
            _tutorialCanvas.SetActive(true);
            TutorialDialogManager.obj.StartFadeIn();
            while(!TutorialDialogManager.obj.tutorialCompleted) {
                yield return null;
            }
            _tutorialCanvas.SetActive(false);
            Time.timeScale = 1;
            PlayerMovement.obj.UnFreeze();
            GameEventManager.obj.HasSeenForcePushJumpTutorial = true;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
