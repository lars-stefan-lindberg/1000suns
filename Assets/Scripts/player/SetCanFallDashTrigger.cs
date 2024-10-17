using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCanFallDashTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            Player.obj.CanFallDash = true;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
