using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableToHumanTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if(PlayerPowersManager.obj.CanTurnFromBlobToHuman) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            PlayerPowersManager.obj.CanTurnFromBlobToHuman = true;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
