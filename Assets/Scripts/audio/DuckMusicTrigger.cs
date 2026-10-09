using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckMusicTrigger : MonoBehaviour
{
   void OnTriggerEnter2D(Collider2D collider) {
        if(!collider.CompareTag("Player"))
            return;

        AudioStateManager.obj.SetDucked(true);        
    }

    void OnTriggerExit2D(Collider2D collider) {
        if(!collider.CompareTag("Player"))
            return;

        AudioStateManager.obj.SetDucked(false);        
    }
}
