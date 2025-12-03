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
    public float _forcePushFlashSpeed = 0.17f;

    private Animator _animator;
    private LayerMask _groundLayerMasks;
    private PlayerFlash _playerFlash;
    private PlayerChargeFlash _playerChargeFlash;
    private PlayerLightManager _playerLightManager;

    void Awake()
    {
        obj = this;
        _animator = GetComponentInChildren<Animator>();
        _collider = GetComponent<BoxCollider2D>();
        _groundLayerMasks = LayerMask.GetMask("Ground");
        _playerFlash = GetComponentInChildren<PlayerFlash>();
        _playerChargeFlash = GetComponentInChildren<PlayerChargeFlash>();
        _playerLightManager = GetComponentInChildren<PlayerLightManager>();
    }

    void OnEnable()
    {
        _animator = GetComponentInChildren<Animator>();
        SetHasCape(hasCape);
    }

    public void ResetAnimator() {
        _animator.Play("main_character_with_cape_idle", 0, 0);
    }

    public void SetStatic() {
        rigidBody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        rigidBody.velocity = Vector2.zero;
        rigidBody.bodyType = RigidbodyType2D.Static;

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
        _animator.SetTrigger("genericDeath");
    }

    public void PlayShadowDeathAnimation() {
        rigidBody.bodyType = RigidbodyType2D.Static;
        _collider.enabled = false;
        _animator.SetTrigger("shadowDeath");
    }

    [ContextMenu("PlayGetUpAnimation")]
    public void PlayGetUpAnimation() {
        _animator.SetTrigger("getUp");
        _animator.speed = 0;
    }

    [ContextMenu("PlayToBlobAnimation")] 
    public void PlayToBlobAnimation() {
        _animator.SetTrigger("toBlob");
    }

    public void PlayToShadowTwinAnimation() {
        _animator.SetTrigger("toShadowTwin");
    }

    [ContextMenu("PlayToPlayerAnimation")] 
    public void PlayToPlayerAnimation() {
        _animator.SetTrigger("toPlayer");
    }

    public void PlayPullRoots() {
        _animator.SetTrigger("pullRoots");
    }

    public void EndPullRoots() {
        _animator.SetTrigger("endPullRoots");
    }

    [ContextMenu("StartAnimator")]
    public void StartAnimator() {
        _animator.speed = 1;
    }

    public void SetAnimatorSpeed(float speed) {
        _animator.speed = speed;
    }

    public void PlaySpawn() {
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
        _collider.enabled = true;
        PlayerMovement.obj.Freeze(spawnFreezeDuration);
        _animator.SetTrigger("spawn");
    }

    public void SetHasCape(bool _hasCape) {
        if(_hasCape && hasPowerUp)
            _animator.SetLayerWeight(2, 1);
        else if(_hasCape)
            _animator.SetLayerWeight(1, 1);
        else
            _animator.SetLayerWeight(1, 0);
        hasCape = _hasCape;
    }

    public void SetBlackCape() {
        _animator.SetLayerWeight(1, 0);
        _animator.SetLayerWeight(2, 1);
    }

    public void SetHasPowerUp(bool _hasPowerUp) {
        if(_hasPowerUp) {
            _animator.SetLayerWeight(1, 0);
            _animator.SetLayerWeight(2, 1);
            //Flash animation
            _playerFlash.FlashOnce();
        } else {
            _animator.SetLayerWeight(1, 1);
            _animator.SetLayerWeight(2, 0);
        }
        hasPowerUp = _hasPowerUp;
    }

    public void FlashOnce() {
        _playerFlash.FlashOnce();
    }

    [ContextMenu("Force push flash")]
    public void ForcePushFlash() {
        _playerFlash.FlashFor(0.1f, _forcePushFlashSpeed);
    }

    [ContextMenu("Flash")]
    public void FlashFor() {
        FlashFor(5);
        //_playerFlash.StartFlashing();
    }
    public void FlashFor(float duration) {
        _playerFlash.FlashFor(duration, 0.05f);
    }

    public void AbortFlash() {
        _playerFlash.AbortFlash();
    }

    public void StartFullyChargedVfx() {
        _playerFlash.StartFullyChargedVfx();
    }
    public void EndFullyChargedVfx() {
        _playerFlash.EndFullyChargedVfx();
    }

    //Not sure about this flashing effect when charging, so leaving it out for now
    public void StartChargeFlash() {
        _playerFlash.ChargeFlash();
        //_playerChargeFlash.StartFlashing();
    }
    public void EndChargeFlash() {
       // _playerChargeFlash.EndFlashing();
    }

    public void SetCaveStartingCoordinates() {
        transform.position = new Vector2(191.975f, 24.89f);
    }

    public void PlayerPushLight() {
        _playerLightManager.IncreaseLightSize();
    }

    public void RestorePlayerPushLight() {
        _playerLightManager.RestoreLightSize();
    }

    public void FadeOutPlayerLight() {
        _playerLightManager.FadeOut();
    }

    void OnDestroy()
    {
        obj = null; 
    }

    public float GetColliderHeight()
    {
        if (_collider == null)
            return 0f;
            
        return _collider.bounds.size.y;
    }
}
