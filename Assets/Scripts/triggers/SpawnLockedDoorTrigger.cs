using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnLockedDoorTrigger : MonoBehaviour
{
    public GameObject lockedDoor;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            lockedDoor.SetActive(true);
            Destroy(this);
        }
    }
}
