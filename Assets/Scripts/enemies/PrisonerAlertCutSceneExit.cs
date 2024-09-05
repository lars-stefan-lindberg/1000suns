using System.Collections;
using UnityEngine;

public class PrisonerAlertCutSceneExit : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            CutsceneManager.obj.firstPrisonerSpawned = true;
        }
    }
}
