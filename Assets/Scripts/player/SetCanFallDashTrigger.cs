using System.Collections;
using UnityEngine;

public class SetCanFallDashTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _tutorialCanvas;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            PlayerPowersManager.obj.CanFallDash = true;
            TutorialFooterManager.obj.StartFadeIn();
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
