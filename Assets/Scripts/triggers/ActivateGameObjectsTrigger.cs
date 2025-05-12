using System.Collections.Generic;
using UnityEngine;

public class ActivateGameObjectsTrigger : MonoBehaviour
{
    public List<GameObject> gameObjectsToActivate;
    public bool disableOnTrigger = true;
    private bool _hasBeenActivated = false;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(_hasBeenActivated && disableOnTrigger) {
            return;
        }
        if (collision.gameObject.CompareTag("Player"))
        { 
            for (int i = 0; i < gameObjectsToActivate.Count; i++)
            {
                gameObjectsToActivate[i].SetActive(true);
            }
            _hasBeenActivated = true;
        }
    }   
}
