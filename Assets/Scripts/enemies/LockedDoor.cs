using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    private Animator _animator;

    void Awake() {
        _animator = GetComponent<Animator>();
    }

    void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.CompareTag("Player")) {
            Reaper.obj.KillPlayerShadow();
        }
    }

    [ContextMenu("PlayDeathAnimation")]
    public void PlayDeathAnimation() {
        _animator.SetTrigger("death");
    }

    public void Kill() {
        Destroy(gameObject);
    }
}
