using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LockedDoorOrb : MonoBehaviour
{
    public GameObject lockedDoor;
    public int numberOfSoulsBeforeUnlock = 0;
    private Animator _animator;
    private int _numberOfSoulsCount = 0;

    void Awake() {
        _animator = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("PrisonerSoul")) {
            _numberOfSoulsCount += 1;
            _animator.SetTrigger("absorb");
            Destroy(other.gameObject);
            if(_numberOfSoulsCount == numberOfSoulsBeforeUnlock)
                lockedDoor.GetComponent<LockedDoor>().PlayDeathAnimation();
        }
    }
}
