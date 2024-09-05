using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapeCutsceneRoom : MonoBehaviour
{
    public GameObject cape;
    public GameObject backgroundBlobs;
    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            if(CutsceneManager.obj.capePicked) {
                cape.SetActive(false);
                backgroundBlobs.SetActive(false);
            }

        }
    }
}
