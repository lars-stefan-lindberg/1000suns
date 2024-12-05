using System.Collections.Generic;
using UnityEngine;

public class ActivateGameObjectsTrigger : MonoBehaviour
{
    public List<GameObject> gameObjectsToActivate;
    private bool _hasBeenActivated = false;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(_hasBeenActivated) {
            return;
        }
        if (collision.gameObject.CompareTag("Player"))
        { 
            foreach (GameObject gameObject in gameObjectsToActivate)
            {
                gameObject.SetActive(true);
            }
            _hasBeenActivated = true;
        }
    }   
}
