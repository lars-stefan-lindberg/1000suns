using System.Collections.Generic;
using UnityEngine;

public class DeactivateGameObjectsTrigger : MonoBehaviour
{
    public List<GameObject> gameObjectsToDeactivate;
    public bool disableOnTrigger = true;
    private bool _hasBeenDeactivated = false;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(_hasBeenDeactivated && disableOnTrigger) {
            return;
        }
        if (collision.gameObject.CompareTag("Player"))
        { 
            for (int i = 0; i < gameObjectsToDeactivate.Count; i++)
            {
                gameObjectsToDeactivate[i].SetActive(false);
            }
            _hasBeenDeactivated = true;
        }
    }   
}
