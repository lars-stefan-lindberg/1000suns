using System;
using System.Collections;
using FMODUnity;
using FunkyCode;
using UnityEngine;
using DG.Tweening;

public class PropellingPlatform : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private EventReference _impactSfx;
    [SerializeField] private LightSprite2D _lightSprite;
    [SerializeField] private ParticleSystem _propelParticles;
    private Rigidbody2D _rigidBody;
    private Pullable _pullable;
    public float blockingCastDistance = 0.1f;
    public float deceleration = 20f;
    private LayerMask _blockingCastLayerMask;
    private LayerMask _soundTriggeringLayerMask;
    private bool movePlatform = false;
    private PropellingPlatformFlash _propellingPlatformFlash;
    
    [Header("VFX Movement")]
    [SerializeField] private float _vfxMoveDistance = 0.2f;
    [SerializeField] private float _vfxMoveDuration = 0.1f;
    [SerializeField] private float _vfxReturnDuration = 0.3f;
    [SerializeField] private Ease _vfxMoveEase = Ease.OutQuad;
    [SerializeField] private Ease _vfxReturnEase = Ease.InOutQuad;
    
    [Header("Particle System")]
    [SerializeField] private float _particleFollowDuration = 0.3f;
    [SerializeField] private float _particleFadeOutDelay = 0.2f;
    
    private Transform _spriteTransform;
    private Vector3 _originalSpritePosition;
    private Vector3 _originalParticlePosition;
    private Coroutine _particleCoroutine;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _blockingCastLayerMask = LayerMask.GetMask(new[] { "Ground", "Default", "JumpThroughs", "Enemies", "Block", "HazardCollider" });
        _soundTriggeringLayerMask = LayerMask.GetMask(new[] { "Ground", "JumpThroughs", "Block", "HazardCollider" });
        _pullable = GetComponentInChildren<Pullable>();
        _propellingPlatformFlash = GetComponentInChildren<PropellingPlatformFlash>();
        _spriteTransform = _spriteRenderer.transform;
        _originalSpritePosition = _spriteTransform.localPosition;
        
        if (_propelParticles != null)
        {
            _originalParticlePosition = _propelParticles.transform.localPosition;
        }
    }

    void Start() {
        _propellingPlatformFlash.StartIdleFlashing();
    }

    private void StopIdleFlashing() {
        _propellingPlatformFlash.StopIdleFlashing();
    }

    private void StartIdleFlashing() {
        _propellingPlatformFlash.StartIdleFlashing();
    }

    public void PlayImpactSfx() {
        SoundFXManager.obj.PlayAtPosition(_impactSfx, transform.position);
    }

    public void TriggerVfx(Vector2 direction) {
        _propellingPlatformFlash.TriggerVfxFlash();

        // Handle particle system
        if (_propelParticles != null)
        {
            // Stop any existing particle coroutine
            if (_particleCoroutine != null)
            {
                StopCoroutine(_particleCoroutine);
            }
            
            // Start new particle behavior
            _particleCoroutine = StartCoroutine(ParticleFollowCoroutine());
        }
        
        // Kill any existing tweens on the sprite transform
        _spriteTransform.DOKill();
        _spriteTransform.localPosition = _originalSpritePosition;
        
        // Calculate target position based on direction
        Vector3 targetPosition = _originalSpritePosition + (Vector3)(direction.normalized * _vfxMoveDistance);
        
        // Move to target position, then return to original
        _spriteTransform.DOLocalMove(targetPosition, _vfxMoveDuration)
            .SetEase(_vfxMoveEase)
            .OnComplete(() => {
                _spriteTransform.DOLocalMove(_originalSpritePosition, _vfxReturnDuration)
                    .SetEase(_vfxReturnEase);
            });
    }

    private bool somethingToTheRight = false;
    private bool somethingToTheLeft = false;
    private bool somethingAbove = false;
    private bool somethingBelow = false;

    private bool _isBeingPulled = false;

    private void Update()
    {
        bool isPullablePulled = _pullable != null && _pullable.IsPulled;
        bool wasJustPulled = isPullablePulled && !_isBeingPulled;
        bool wasJustReleased = !isPullablePulled && _isBeingPulled;
        if(wasJustPulled) {
            _isBeingPulled = true;
            movePlatform = true;
            StopIdleFlashing();
        } else if(wasJustReleased) {
            _isBeingPulled = false;
            StartIdleFlashing();
        }

        if(wasJustPulled) {
            _collider.isTrigger = false;
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        }
        else if(wasJustReleased) {
            _collider.isTrigger = true;
            _rigidBody.bodyType = RigidbodyType2D.Kinematic;
        }

        if(!_isBeingPulled) {            
            // Only check for walls when not being pulled
            RaycastHit2D hitRight = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.right, blockingCastDistance, _blockingCastLayerMask);
            RaycastHit2D hitLeft = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.left, blockingCastDistance, _blockingCastLayerMask);
            RaycastHit2D hitAbove = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.up, blockingCastDistance, _blockingCastLayerMask);
            RaycastHit2D hitBelow = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.down, blockingCastDistance, _blockingCastLayerMask);
            
            somethingToTheRight = hitRight.collider != null;
            somethingToTheLeft = hitLeft.collider != null;
            somethingAbove = hitAbove.collider != null;
            somethingBelow = hitBelow.collider != null;

            if (hitRight.collider != null && _rigidBody.velocity.x > 0) {
                movePlatform = false;
                if (((1 << hitRight.collider.gameObject.layer) & _soundTriggeringLayerMask) != 0) {
                    SoundFXManager.obj.PlayAtPosition(_impactSfx, transform.position);
                }
            }
            if (hitLeft.collider != null && _rigidBody.velocity.x < 0) {
                movePlatform = false;
                if (((1 << hitLeft.collider.gameObject.layer) & _soundTriggeringLayerMask) != 0) {
                    SoundFXManager.obj.PlayAtPosition(_impactSfx, transform.position);
                }
            }
            if (hitAbove.collider != null && _rigidBody.velocity.y > 0) {
                movePlatform = false;
                if (((1 << hitAbove.collider.gameObject.layer) & _soundTriggeringLayerMask) != 0) {
                    SoundFXManager.obj.PlayAtPosition(_impactSfx, transform.position);
                }
            }
            if (hitBelow.collider != null && _rigidBody.velocity.y < 0) {
                movePlatform = false;
                if (((1 << hitBelow.collider.gameObject.layer) & _soundTriggeringLayerMask) != 0) {
                    SoundFXManager.obj.PlayAtPosition(_impactSfx, transform.position);
                }
            }
        }
        else
        {
            // Check for walls when being pulled
            RaycastHit2D hitRight = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.right, blockingCastDistance, _blockingCastLayerMask);
            RaycastHit2D hitLeft = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.left, blockingCastDistance, _blockingCastLayerMask);
            RaycastHit2D hitAbove = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.up, blockingCastDistance, _blockingCastLayerMask);
            RaycastHit2D hitBelow = Physics2D.BoxCast(_collider.bounds.center, _collider.size, 0, Vector2.down, blockingCastDistance, _blockingCastLayerMask);
            
            somethingToTheRight = hitRight.collider != null;
            somethingToTheLeft = hitLeft.collider != null;
            somethingAbove = hitAbove.collider != null;
            somethingBelow = hitBelow.collider != null;

            if (hitRight.collider != null && _rigidBody.velocity.x > 0 && ((1 << hitRight.collider.gameObject.layer) & _soundTriggeringLayerMask) != 0) {
                SoundFXManager.obj.PlayAtPosition(_impactSfx, transform.position);
            }
            if (hitLeft.collider != null && _rigidBody.velocity.x < 0 && ((1 << hitLeft.collider.gameObject.layer) & _soundTriggeringLayerMask) != 0) {
                SoundFXManager.obj.PlayAtPosition(_impactSfx, transform.position);
            }
            if (hitAbove.collider != null && _rigidBody.velocity.y > 0 && ((1 << hitAbove.collider.gameObject.layer) & _soundTriggeringLayerMask) != 0) {
                SoundFXManager.obj.PlayAtPosition(_impactSfx, transform.position);
            }
            if (hitBelow.collider != null && _rigidBody.velocity.y < 0 && ((1 << hitBelow.collider.gameObject.layer) & _soundTriggeringLayerMask) != 0) {
                SoundFXManager.obj.PlayAtPosition(_impactSfx, transform.position);
            }
            // When being pulled, ensure movePlatform is true so velocity isn't zeroed
            movePlatform = true;
        }

        if (movePlatform)
        {
            // If the platform is being pulled, let the external pull logic control velocity.
            if (!_isBeingPulled)
            {
                _rigidBody.velocity = Vector2.MoveTowards(_rigidBody.velocity, Vector2.zero, deceleration * Time.deltaTime);
            }
        }
        else
        {
            _rigidBody.velocity = new Vector2(0, 0);
        }

        if(!_isBeingPulled && Mathf.Approximately(_rigidBody.velocity.sqrMagnitude, 0f))
        {
            movePlatform = false;
        }
    }

    private Coroutine _disableColliderCoroutine;
    
    private IEnumerator ParticleFollowCoroutine()
    {
        // Start playing particles
        _propelParticles.Play();
        
        // Follow the player for the specified duration
        float elapsedTime = 0f;
        while (elapsedTime < _particleFollowDuration)
        {
            if (ShadowTwinPlayer.obj != null)
            {
                // Set particle system position to follow player (in world space)
                _propelParticles.transform.position = ShadowTwinPlayer.obj.transform.position;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Stop emitting new particles
        _propelParticles.Stop();
        
        // Wait for particles to fade out
        yield return new WaitForSeconds(_particleFadeOutDelay);
        
        // Return particle system to original position
        _propelParticles.transform.localPosition = _originalParticlePosition;
        
        _particleCoroutine = null;
    }
}
