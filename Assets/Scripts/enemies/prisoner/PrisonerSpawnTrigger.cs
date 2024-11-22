using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PrisonerSpawnTrigger : MonoBehaviour
{
    public List<GameObject> prisonersToSpawn;
    private bool _hasBeenSpawned = false;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(_hasBeenSpawned) {
            return;
        }
        if (collision.gameObject.CompareTag("Player"))
        { 
            foreach (GameObject prisoner in prisonersToSpawn)
            {
                prisoner.SetActive(true);
            }
            _hasBeenSpawned = true;
        }
    }   
}
