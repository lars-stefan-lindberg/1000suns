using System;
using System.Collections;
using FMODUnity;
using FunkyCode;
using UnityEngine;

public class FloatingPlatformRotated : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private EventReference _impactSfx;
    [SerializeField] private LightSprite2D _lightSprite;
    private Rigidbody2D _rigidBody;
    private Pullable _pullable;
    public float blockingCastDistance = 0.1f;
    public float deceleration = 20f;
    private LayerMask _blockingCastLayerMask;
    private LayerMask _soundTriggeringLayerMask;
    private bool movePlatform = false;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _blockingCastLayerMask = LayerMask.GetMask(new[] { "Ground", "Default", "JumpThroughs", "Enemies", "Block", "Spikes" });
        _soundTriggeringLayerMask = LayerMask.GetMask(new[] { "Ground", "JumpThroughs", "Block", "Spikes" });
        _pullable = GetComponentInChildren<Pullable>();
    }

    public void PlayImpactSfx() {
        SoundFXManager.obj.PlayAtPosition(_impactSfx, transform.position);
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
        } else if(wasJustReleased) {
            _isBeingPulled = false;
        }

        if(wasJustPulled) {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        }
        else if(wasJustReleased) {
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

    public void TemporarilyDisableCollider(float duration)
    {
        // If already running, stop the previous coroutine
        if (_disableColliderCoroutine != null)
        {
            StopCoroutine(_disableColliderCoroutine);
        }
        _disableColliderCoroutine = StartCoroutine(DisableColliderForDuration(duration));
    }

    private IEnumerator DisableColliderForDuration(float duration)
    {
        _collider.enabled = false;
        yield return new WaitForSeconds(duration);
        _collider.enabled = true;
        _disableColliderCoroutine = null;
    }
}
