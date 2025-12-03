using UnityEngine;

public class PlayerBlob : MonoBehaviour
{
    public static PlayerBlob obj;
    public Rigidbody2D rigidBody;
    private Animator _animator;
    private BoxCollider2D _collider;
    private LayerMask _groundLayerMasks;
    public Surface surface = Surface.Rock;
    private float _spawnFreezeDuration = 0.9f;
    private PlayerChargeFlash _playerChargeFlash;
    private PlayerFlash _playerFlash;

    void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
        _groundLayerMasks = LayerMask.GetMask("Ground");
        _animator = GetComponentInChildren<Animator>();
        _playerFlash = GetComponentInChildren<PlayerFlash>();
        _playerChargeFlash = GetComponentInChildren<PlayerChargeFlash>();
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
        rigidBody.bodyType = RigidbodyType2D.Static;
        _collider.enabled = false;
        _animator.SetTrigger("deathGeneric");
    }

    public void PlaySpawn() {
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
        _collider.enabled = true;
        PlayerBlobMovement.obj.Freeze(_spawnFreezeDuration);
        _animator.SetTrigger("spawn");
    }

    public void SetNewPower() {
        _animator.SetTrigger("isNewPower");
    }

    public void SetNewPowerRecevied() {
        _animator.SetTrigger("newPowerReceived");
    }

    public void ForcePushFlash() {
        _playerFlash.FlashFor(0.1f, 0.17f);
    }

    public void FlashFor(float duration) {
        _playerFlash.FlashFor(duration, 0.05f);
    }

    public void FlashOnce() {
        _playerFlash.FlashOnce();
    }

    public void StartChargeFlash() {
        _playerFlash.ChargeFlash();
    }
    public void EndChargeFlash() {
       //_playerChargeFlash.EndFlashing();
    }
    public void AbortFlash() {
        _playerFlash.AbortFlash();
    }

    void OnDestroy()
    {
        obj = null; 
    }
}
