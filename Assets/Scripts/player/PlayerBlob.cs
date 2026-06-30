using UnityEngine;

public class PlayerBlob : MonoBehaviour
{
    public static PlayerBlob obj;
    public Rigidbody2D rigidBody;
    [SerializeField] private Transform _leftAvatarTarget;
    [SerializeField] private Transform _rightAvatarTarget;
    private Animator _animator;
    private BoxCollider2D _collider;
    private float _spawnFreezeDuration = 0.9f;
    private PlayerChargeFlash _playerChargeFlash;
    private PlayerFlash _playerFlash;
    private PlayerLightManager _playerLightManager;

    void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponentInChildren<Animator>();
        _playerFlash = GetComponentInChildren<PlayerFlash>();
        _playerChargeFlash = GetComponentInChildren<PlayerChargeFlash>();
        _playerLightManager = GetComponentInChildren<PlayerLightManager>();
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

    public void FadeOutPlayerLight() {
        _playerLightManager.FadeOut();
    }

    public void FadeInPlayerLight() {
        _playerLightManager.FadeIn();
    }

    public Transform GetLeftAvatarTarget() {
        return _leftAvatarTarget;
    }

    public Transform GetRightAvatarTarget() {
        return _rightAvatarTarget;
    }

    void OnDestroy()
    {
        obj = null; 
    }
}
