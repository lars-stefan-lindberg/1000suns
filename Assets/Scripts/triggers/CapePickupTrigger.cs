using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapePickupTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            BackgroundBlobManager.obj.StartCutscene();
            CutsceneManager.obj.capePicked = true;
            Player.obj.SetHasCape();
            gameObject.SetActive(false);
        }
    }
}
