using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyPrisonerAlertTrigger : MonoBehaviour
{
    public BabyPrisoner babyPrisoner;
    public float playerFreezeTime = 3f;
    void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.CompareTag("Player")) {
            PlayerMovement.obj.Freeze(playerFreezeTime);
            babyPrisoner.Alert();
        }
    }
}
