using System.Collections.Generic;
using UnityEngine;

public class ActivatePowerUpTrigger : MonoBehaviour
{
    public List<GameObject> gameObjectsToActivate;
    private bool _hasBeenActivated = false;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(!GameManager.obj.FirstPowerUpPicked) 
            return;
        if(_hasBeenActivated) {
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
