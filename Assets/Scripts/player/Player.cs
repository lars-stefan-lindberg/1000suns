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
    private PlayerFlash _playerFlash;
    private PlayerChargeFlash _playerChargeFlash;

    void Awake()
    {
        obj = this;
        _animator = GetComponentInChildren<Animator>();
        _collider = GetComponent<BoxCollider2D>();
        _groundLayerMasks = LayerMask.GetMask("Ground");
        _playerFlash = GetComponentInChildren<PlayerFlash>();
        _playerChargeFlash = GetComponentInChildren<PlayerChargeFlash>();

        SetHasCape(true);
    }

    void OnEnable()
    {
        SetHasCape(true);
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

    [ContextMenu("PlayToPlayerAnimation")] 
    public void PlayToPlayerAnimation() {
        _animator.SetTrigger("toPlayer");
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
        if(_hasCape)
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

    [ContextMenu("Force push flash")]
    public void ForcePushFlash() {
        _playerFlash.FlashFor(0.1f, 0.17f);
    }

    [ContextMenu("Flash")]
    public void FlashFor() {
        FlashFor(5);
        //_playerFlash.StartFlashing();
    }
    public void FlashFor(float duration) {
        _playerFlash.FlashFor(duration, 0.05f);
    }

    //Not sure about this flashing effect when charging, so leaving it out for now
    public void StartChargeFlash() {
        //_playerChargeFlash.StartFlashing();
    }
    public void EndChargeFlash() {
       // _playerChargeFlash.EndFlashing();
    }

    public void SetCaveStartingCoordinates() {
        transform.position = new Vector2(191.975f, 24.89f);
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
