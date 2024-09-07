using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player obj;

    [Header("Dependencies")]
    public Rigidbody2D rigidBody;
    private BoxCollider2D _collider;
    public bool hasPowerUp = false;
    public float spawnFreezeDuration = 1.4f;
    public Surface surface = Surface.Rock;
    public bool hasCape = false;

    private Animator _animator;
    private LayerMask _groundLayerMasks;

    void Awake()
    {
        obj = this;
        _animator = GetComponentInChildren<Animator>();
        _collider = GetComponentInChildren<BoxCollider2D>();
        _groundLayerMasks = LayerMask.GetMask("Ground");
    }

    void OnCollisionEnter2D(Collision2D other) {
        if((_groundLayerMasks.value & (1 << other.gameObject.layer)) != 0) {
            string surfaceTag = other.gameObject.tag;
            if(surfaceTag == "Rock")
                surface = Surface.Rock;
            else if(surfaceTag == "Roots")
                surface = Surface.Roots;
        }
    }

    public void PlayGenericDeathAnimation() {
        _collider.enabled = false;
        _animator.SetTrigger("genericDeath");
    }

    public void PlayShadowDeathAnimation() {
        _collider.enabled = false;
        _animator.SetTrigger("shadowDeath");
    }

    public void PlaySpawn() {
        _collider.enabled = true;
        PlayerMovement.obj.Freeze(spawnFreezeDuration);
        _animator.SetTrigger("spawn");
    }

    public void SetHasCape() {
        hasCape = true;
        _animator.SetLayerWeight(1, 1);
    }

    void OnDestroy()
    {
        obj = null; 
    }
}
