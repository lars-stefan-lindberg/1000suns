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
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        obj = this;
        _animator = GetComponentInChildren<Animator>();
        _collider = GetComponentInChildren<BoxCollider2D>();
        _groundLayerMasks = LayerMask.GetMask("Ground");
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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

    public void SetHasPowerUp(bool _hasPowerUp) {
        if(_hasPowerUp) {
            _animator.SetLayerWeight(1, 0);
            _animator.SetLayerWeight(2, 1);
        } else {
            _animator.SetLayerWeight(1, 1);
            _animator.SetLayerWeight(2, 0);
        }
        hasPowerUp = _hasPowerUp;
    }

    public void SetCaveStartingCoordinates() {
        transform.position = new Vector2(233.875f, 78.375f);
    }

    void OnDestroy()
    {
        obj = null; 
    }
}
