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
    [SerializeField] private float _lashSpeed = 10f;
    [SerializeField] private float _lashDistance = 10f;
    [SerializeField] private LayerMask _collisionLayerMask;
    [SerializeField] private float _raycastDistance = 0.5f;
    [SerializeField] private float _fadeOutDuration = 0.5f;

    private Coroutine _lashCoroutine;
    private Vector3 _headMaskStartPosition;
    private Vector3 _beamHeadStartPosition;
    private float _tailOffsetX;
    private bool _tailOffsetInitialized;
    private bool _hasHitSurface;
    private int _direction = 1; // 1 for right, -1 for left

    void Awake()
    {
        InitializeStartPositions();

        if (_beamHeadCollider != null)
        {
            _beamHeadCollider.OnSurfaceHit += HandleSurfaceHit;
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

        // Initialize the offset on first call
        if (!_tailOffsetInitialized)
        {
            float tailWorldX = transform.TransformPoint(_tailMask.localPosition).x;
            _tailOffsetX = tailWorldX - playerPosition.x;
            _tailOffsetInitialized = true;
        }

        // Update tail position maintaining the initial offset
        float newTailWorldX = playerPosition.x + _tailOffsetX;
        Vector3 newTailLocalPosition = transform.InverseTransformPoint(new Vector3(newTailWorldX, playerPosition.y, playerPosition.z));
        _tailMask.localPosition = new Vector3(newTailLocalPosition.x, _tailMask.localPosition.y, _tailMask.localPosition.z);
    }

    public IEnumerator FadeOut()
    {
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
        Lash(1); // Default to right direction for context menu
    }

    public void Lash(int direction) {
        _direction = direction;
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

        float directionMultiplier = _direction;
        Vector3 maskTargetPosition = new Vector3(_headMaskStartPosition.x + (_lashDistance * directionMultiplier), _headMaskStartPosition.y, _headMaskStartPosition.z);
        Vector3 headTargetPosition = new Vector3(_beamHeadStartPosition.x + (_lashDistance * directionMultiplier), _beamHeadStartPosition.y, _beamHeadStartPosition.z);

        while (Vector3.Distance(_headMask.localPosition, maskTargetPosition) > 0.01f && !_hasHitSurface)
        {
            yield return null;
            
            float moveDistance = _lashSpeed * Time.deltaTime;
            
            // Raycast ahead to check for collision before moving
            if (_beamHeadCollider != null)
            {
                Vector3 currentWorldPos = _beamHeadCollider.transform.position;
                Vector3 rayDirection = _direction > 0 ? transform.right : -transform.right;
                
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
}
