using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player obj;

    [Header("Dependencies")]
    public Rigidbody2D rigidBody;
    public Collider2D playerCollider;
    public bool hasPowerUp = false;
    public float spawnFreezeDuration = 1.4f;

    private Animator _animator;

    void Awake()
    {
        obj = this;
        _animator = GetComponentInChildren<Animator>();
    }

    public void PlayGenericDeathAnimation() {
        _animator.SetTrigger("genericDeath");
    }

    public void PlayShadowDeathAnimation() {
        _animator.SetTrigger("shadowDeath");
    }

    public void PlaySpawn() {
        PlayerMovement.obj.Freeze(spawnFreezeDuration);
        _animator.SetTrigger("spawn");
    }

    void OnDestroy()
    {
        obj = null; 
    }
}
