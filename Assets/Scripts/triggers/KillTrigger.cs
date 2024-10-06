using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    [SerializeField] private bool _onlyKillPlayer = false;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            Reaper.obj.KillPlayerGeneric();
        }
        if(other.gameObject.CompareTag("Enemy") && !_onlyKillPlayer) {
            Reaper.obj.KillPrisoner(other.gameObject.GetComponent<Prisoner>());
        }
    }
}
