using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCanFallDashTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialCanvas;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            Player.obj.CanFallDash = true;
            StartCoroutine(ShowTutorial());
        }
    }

    private IEnumerator ShowTutorial() {
        if(!GameEventManager.obj.HasSeenForcePushDashTutorial) {
            PlayerMovement.obj.Freeze();
            Time.timeScale = 0;
            _tutorialCanvas.SetActive(true);
            TutorialManager.obj.StartFadeIn();
            while(!TutorialManager.obj.tutorialCompleted) {
                yield return null;
            }
            _tutorialCanvas.SetActive(false);
            Time.timeScale = 1;
            PlayerMovement.obj.UnFreeze();
            GameEventManager.obj.HasSeenForcePushDashTutorial = true;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
