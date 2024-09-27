using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cape : MonoBehaviour
{
    private Animator _animator;

    void Awake() {
        _animator = GetComponent<Animator>();
        _animator.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            CapeRoomBackgroundBlobManager.obj.StartCutscene();
            CutsceneManager.obj.capePicked = true;
            
            StartCoroutine(DelayCapePickupSetInactive());
        }
    }

    public void StartAnimation() {
        _animator.enabled = true;
    }

    //Since we are waiting for the light to fill the screen before removing cape and updating player sprite
    private IEnumerator DelayCapePickupSetInactive() {
        yield return new WaitForSeconds(2);
        Player.obj.SetHasCape();
        gameObject.SetActive(false);
    }
}
