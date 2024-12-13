using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyPrisonerAlertTrigger : MonoBehaviour
{
    public BabyPrisoner babyPrisoner;
    public float playerFreezeTime = 3f;
    private BoxCollider2D _boxCollider;

    void Awake() {
        if(GameEventManager.obj.BabyPrisonerAlerted) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.gameObject.CompareTag("Player")) {
            PlayerMovement.obj.Freeze(playerFreezeTime);
            babyPrisoner.Alert();
            _boxCollider.enabled = false;
        }
    }
}
