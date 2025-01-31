using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C215EnablePowerUpTrigger : MonoBehaviour
{
    public GameObject powerUp;
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player") && GameEventManager.obj.C215WallBroken)
            powerUp.SetActive(true);
    }
}
