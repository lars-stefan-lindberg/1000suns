using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class PrisonerSpawnTrigger : MonoBehaviour
{
    public List<GameObject> prisonersToSpawn;
    private bool _hasBeenSpawned = false;
    
    // Event that will be invoked before prisoners are spawned
    public UnityEvent OnSpawn;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(_hasBeenSpawned) {
            return;
        }
        if (collision.gameObject.CompareTag("Player"))
        { 
            // Invoke the BeforeSpawn event
            OnSpawn?.Invoke();
            
            foreach (GameObject prisoner in prisonersToSpawn)
            {
                prisoner.SetActive(true);
            }
            _hasBeenSpawned = true;
        }
    }   
}
