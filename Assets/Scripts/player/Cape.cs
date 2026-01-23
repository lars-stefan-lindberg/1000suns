using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cape : MonoBehaviour
{
    [SerializeField] private GameObject _container;
    private Animator _animator;

    void Awake() {
        if(GameManager.obj.CapePicked) {
            _container.SetActive(false);
            Destroy(gameObject, 3);
        }
        _animator = GetComponent<Animator>();
        _animator.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            CapeRoomBackgroundBlobManager.obj.StartCutscene();
            GameManager.obj.CapePicked = true;
            
            StartCoroutine(DelayCapePickupSetInactive());
        }
    }

    public void StopHover() {
        StartCoroutine(StopHoverCoroutine());
    }

    public void StartAnimation() {
        _animator.enabled = true;
    }

    //Since we are waiting for the light to fill the screen before removing cape and updating player sprite
    private IEnumerator DelayCapePickupSetInactive() {
        yield return new WaitForSeconds(2);
        Player.obj.SetAnimatorLayerAndHasCape(true);
        gameObject.SetActive(false);
    }

    private IEnumerator StopHoverCoroutine() {
        _container.GetComponent<Animator>().SetTrigger("returnToStart");
        yield return new WaitForSeconds(2);
        _container.GetComponent<Animator>().enabled = false;
    }
}
