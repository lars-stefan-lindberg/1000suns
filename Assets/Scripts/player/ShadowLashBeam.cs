using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowLashBeam : MonoBehaviour
{
    [SerializeField] private Transform _headMask;
    [SerializeField] private Transform _tailMask;
    [SerializeField] private SpriteRenderer _beamRenderer;
    [SerializeField] private SpriteRenderer _beamHeadRenderer;
    [SerializeField] private ShadowLashBeamCollider _beamHeadCollider;
    [SerializeField] private ParticleSystem _particles;
    [SerializeField] private float _lashSpeed = 10f;
    [SerializeField] private float _lashDistance = 10f;
    [SerializeField] private LayerMask _collisionLayerMask;
    [SerializeField] private float _raycastDistance = 0.5f;
    [SerializeField] private float _fadeOutDuration = 0.5f;
    [SerializeField] private float _particleSystemDurationAfterDestroy = 1.0f;
    [SerializeField] private Animator _beamHeadAnimator;

    private Coroutine _lashCoroutine;
    private Vector3 _headMaskStartPosition;
    private Vector3 _beamHeadStartPosition;
    private float _tailOffsetX;
    private float _tailOffsetY;
    private bool _tailOffsetInitialized;
    private bool _hasHitSurface;
    private Vector2 _lashDirection = Vector2.right; // Direction of the lash (horizontal or vertical)
    private Vector3 _previousTailLocalPosition;

    void Awake()
    {
        InitializeStartPositions();

        if (_beamHeadCollider != null)
        {
            _beamHeadCollider.OnSurfaceHit += HandleSurfaceHit;
        }

        if (_particles != null)
        {
            _particles.Stop();
        }
    }

    void OnDestroy() {
        if (_beamHeadCollider != null)
        {
            _beamHeadCollider.OnSurfaceHit -= HandleSurfaceHit;
        }
    }

    private void InitializeStartPositions()
    {
        if (_headMask != null)
        {
            _headMaskStartPosition = _headMask.localPosition;
        }

        if (_beamHeadRenderer != null)
        {
            _beamHeadStartPosition = _beamHeadRenderer.transform.localPosition;
        }
    }

    public void UpdateTailPosition(Vector3 playerPosition)
    {
        if (_tailMask == null) return;

        // Initialize the offsets on first call
        if (!_tailOffsetInitialized)
        {
            Vector3 tailWorldPos = transform.TransformPoint(_tailMask.localPosition);
            _tailOffsetX = tailWorldPos.x - playerPosition.x;
            _tailOffsetY = tailWorldPos.y - playerPosition.y;
            _tailOffsetInitialized = true;
            _previousTailLocalPosition = _tailMask.localPosition;
        }

        // Update tail position based on lash direction
        Vector3 newTailLocalPosition;
        if (Mathf.Abs(_lashDirection.y) > 0)
        {
            // Vertical lash - keep X position fixed, update Y to follow player movement
            float newTailWorldY = playerPosition.y + _tailOffsetY;
            newTailLocalPosition = transform.InverseTransformPoint(new Vector3(playerPosition.x, newTailWorldY, playerPosition.z));
            newTailLocalPosition = new Vector3(_tailMask.localPosition.x, newTailLocalPosition.y, _tailMask.localPosition.z);
        }
        else
        {
            // Horizontal lash - keep Y position fixed, update X to follow player movement
            float newTailWorldX = playerPosition.x + _tailOffsetX;
            newTailLocalPosition = transform.InverseTransformPoint(new Vector3(newTailWorldX, playerPosition.y, playerPosition.z));
            newTailLocalPosition = new Vector3(newTailLocalPosition.x, _tailMask.localPosition.y, _tailMask.localPosition.z);
        }
        
        // Calculate the delta movement of the tail
        Vector3 tailDelta = newTailLocalPosition - _previousTailLocalPosition;
        
        // Start playing particles when tail starts moving
        if (tailDelta.magnitude > 0.001f && _particles != null && !_particles.isPlaying)
        {
            _particles.Play();
        }
        
        // Move particles by the same delta
        if (_particles != null)
        {
            Vector3 particlesLocalPosition = _particles.transform.localPosition;
            _particles.transform.localPosition = particlesLocalPosition + tailDelta;
        }
        
        // Update tail mask position
        _tailMask.localPosition = newTailLocalPosition;
        _previousTailLocalPosition = newTailLocalPosition;
    }

    public IEnumerator DisableBeamAndWaitForParticles()
    {
        // Disable beam visual components immediately
        if (_headMask != null)
        {
            _headMask.gameObject.SetActive(false);
        }
        
        if (_tailMask != null)
        {
            _tailMask.gameObject.SetActive(false);
        }
        
        if (_beamRenderer != null)
        {
            _beamRenderer.gameObject.SetActive(false);
        }
        
        if (_beamHeadRenderer != null)
        {
            _beamHeadRenderer.gameObject.SetActive(false);
        }
        
        // Stop emitting new particles but let existing ones finish
        if (_particles != null)
        {
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        
        // Wait for the specified duration to let particles finish
        yield return new WaitForSeconds(_particleSystemDurationAfterDestroy);
    }

    public IEnumerator FadeOut()
    {
        TriggerNoHitSurfaceAnimation();
        if(_lashCoroutine != null)
            yield return _lashCoroutine;
        
        float elapsedTime = 0f;
        Color beamStartColor = _beamRenderer != null ? _beamRenderer.color : Color.white;
        Color beamHeadStartColor = _beamHeadRenderer != null ? _beamHeadRenderer.color : Color.white;
        
        while (elapsedTime < _fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / _fadeOutDuration);
            
            if (_beamRenderer != null)
            {
                Color beamColor = beamStartColor;
                beamColor.a = alpha;
                _beamRenderer.color = beamColor;
            }
            
            if (_beamHeadRenderer != null)
            {
                Color beamHeadColor = beamHeadStartColor;
                beamHeadColor.a = alpha;
                _beamHeadRenderer.color = beamHeadColor;
            }
            
            yield return null;
        }
        
        // Ensure final alpha is exactly 0
        if (_beamRenderer != null)
        {
            Color beamColor = beamStartColor;
            beamColor.a = 0f;
            _beamRenderer.color = beamColor;
        }
        
        if (_beamHeadRenderer != null)
        {
            Color beamHeadColor = beamHeadStartColor;
            beamHeadColor.a = 0f;
            _beamHeadRenderer.color = beamHeadColor;
        }
    }

    private void HandleSurfaceHit()
    {
        _hasHitSurface = true;
    }

    [ContextMenu("Lash")]
    public void Lash() {
        Lash(Vector2.right); // Default to right direction for context menu
    }

    public void Lash(Vector2 direction) {
        _lashDirection = direction.normalized;
        if (_headMaskStartPosition == Vector3.zero && _headMask != null)
        {
            InitializeStartPositions();
        }
        _lashCoroutine = StartCoroutine(LashCoroutine());
    }

    [ContextMenu("Reset")]
    public void Reset() {
        _headMask.transform.localPosition = _headMaskStartPosition;
        _beamHeadRenderer.transform.localPosition = _beamHeadStartPosition;
    }

    private IEnumerator LashCoroutine() {
        if (_headMask == null || _beamRenderer == null)
        {
            yield break;
        }

        _headMask.localPosition = _headMaskStartPosition;

        if (_beamHeadRenderer != null)
        {
            _beamHeadRenderer.transform.localPosition = _beamHeadStartPosition;
        }

        // Calculate target positions based on lash direction
        Vector3 lashOffset = new Vector3(_lashDirection.x * _lashDistance, _lashDirection.y * _lashDistance, 0);
        Vector3 maskTargetPosition = _headMaskStartPosition + lashOffset;
        Vector3 headTargetPosition = _beamHeadStartPosition + lashOffset;

        while (Vector3.Distance(_headMask.localPosition, maskTargetPosition) > 0.01f && !_hasHitSurface)
        {
            yield return null;
            
            float moveDistance = _lashSpeed * Time.deltaTime;
            
            // Raycast ahead to check for collision before moving
            if (_beamHeadCollider != null)
            {
                Vector3 currentWorldPos = _beamHeadCollider.transform.position;
                
                // Convert lash direction to world space for raycasting
                Vector3 rayDirection;
                if (Mathf.Abs(_lashDirection.y) > 0)
                {
                    // Vertical lash
                    rayDirection = _lashDirection.y > 0 ? transform.up : -transform.up;
                }
                else
                {
                    // Horizontal lash
                    rayDirection = _lashDirection.x > 0 ? transform.right : -transform.right;
                }
                
                RaycastHit2D hit = Physics2D.Raycast(currentWorldPos, rayDirection, moveDistance + _raycastDistance, _collisionLayerMask);
                
                if (hit.collider != null)
                {
                    // Calculate how far we can move before hitting
                    float distanceToHit = hit.distance - _raycastDistance;
                    if (distanceToHit > 0)
                    {
                        moveDistance = Mathf.Min(moveDistance, distanceToHit);
                    }
                    else
                    {
                        _hasHitSurface = true;
                        break;
                    }
                }
            }
            
            _headMask.localPosition = Vector3.MoveTowards(_headMask.localPosition, maskTargetPosition, moveDistance);
            
            if (_beamHeadRenderer != null)
            {
                _beamHeadRenderer.transform.localPosition = Vector3.MoveTowards(_beamHeadRenderer.transform.localPosition, headTargetPosition, moveDistance);
            }
        }

        if (!_hasHitSurface)
        {
            _headMask.localPosition = maskTargetPosition;
            
            if (_beamHeadRenderer != null)
            {
                _beamHeadRenderer.transform.localPosition = headTargetPosition;
            }
        }
    }

    public void TriggerHitSurfaceAnimation() {
        if (_beamHeadAnimator != null) {
            _beamHeadAnimator.SetTrigger("hitSurface");
        }
    }

    private void TriggerNoHitSurfaceAnimation() {
        if (_beamHeadAnimator != null) {
            _beamHeadAnimator.SetTrigger("noHitSurface");
        }
    }
}
