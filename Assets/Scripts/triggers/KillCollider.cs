using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillCollider : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.CompareTag("Player")) {
            Reaper.obj.KillPlayerGeneric(PlayerManager.obj.GetPlayerTypeFromCollision(other));
        }
        if(other.gameObject.CompareTag("Enemy")) {
            Reaper.obj.KillPrisoner(other.gameObject.GetComponent<Prisoner>());
        }
    }
}
