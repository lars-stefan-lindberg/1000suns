using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D other) {
        Debug.Log("Collision");
        if(other.gameObject.CompareTag("Player")) {
            Destroy(gameObject);
        }
    }
}
