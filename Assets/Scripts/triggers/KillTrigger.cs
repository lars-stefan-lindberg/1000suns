using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            Reaper.obj.KillPlayerGeneric();
        }
        if(other.gameObject.CompareTag("Enemy")) {
            Reaper.obj.KillPrisoner(other.gameObject.GetComponent<Prisoner>());
        }
    }
}
