using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.CompareTag("Player")) {
            Reaper.obj.KillPlayerGeneric();
        }
    }
}
