using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrisonerTrigger : MonoBehaviour
{
    public GameObject prisoner;
    private bool isSpawned = false;

    private void OnTriggerExit2D(Collider2D collider) {
        if(collider.transform.CompareTag("Block") && !isSpawned && prisoner != null) {
            prisoner.SetActive(true);
            isSpawned = true;
        }
    }
}
