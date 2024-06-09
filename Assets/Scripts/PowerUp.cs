using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("triggered");
        if(other.gameObject.CompareTag("Player")) {
            Player.obj.hasPowerUp = true;
        }
    }
}
