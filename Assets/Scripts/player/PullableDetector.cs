using System.Collections.Generic;
using UnityEngine;

public class PullableDetector : MonoBehaviour
{
    private HashSet<Pullable> _detectedPullables = new HashSet<Pullable>();
    private Transform _playerTransform;
    private BoxCollider2D _boxCollider;
    
    [SerializeField] private LayerMask _blockingMask;
    [SerializeField] private float _beneathExclusionDistance = 1.5f;
    [SerializeField] private float _triggerBoxBuffer = 1f;
    
    private Vector2 _cachedHighlightBoxSize;
    private Vector2 _cachedBoxOffset;
    
    private void Awake()
    {
        _playerTransform = transform.parent;
        _boxCollider = GetComponent<BoxCollider2D>();
        
        if (_boxCollider == null)
        {
            Debug.LogWarning("PullableDetector: BoxCollider2D not found! Trigger detection will not work.");
        }
        else
        {
            UpdateBoxColliderSize();
        }
    }
    
    private void Start()
    {
        UpdateCachedRanges();
    }
    
    private void UpdateCachedRanges()
    {
        if (ShadowTwinPull.obj != null)
        {
            _cachedHighlightBoxSize = ShadowTwinPull.obj.GetHighlightBoxSize();
            _cachedBoxOffset = ShadowTwinPull.obj.GetBoxOffset();
        }
    }
    
    private void UpdateBoxColliderSize()
    {
        if (_boxCollider != null && ShadowTwinPull.obj != null)
        {
            Vector2 controlBoxSize = ShadowTwinPull.obj.GetControlBoxSize();
            _boxCollider.size = controlBoxSize + Vector2.one * _triggerBoxBuffer * 2f;
            _boxCollider.offset = ShadowTwinPull.obj.GetBoxOffset();
        }
    }
    
    private void OnValidate()
    {
        // Update collider size when values change in editor
        if (_boxCollider == null)
            _boxCollider = GetComponent<BoxCollider2D>();
            
        UpdateBoxColliderSize();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Pullable pullable = collision.GetComponent<Pullable>();
        if (pullable != null)
        {
            _detectedPullables.Add(pullable);
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        Pullable pullable = collision.GetComponent<Pullable>();
        if (pullable != null)
        {
            _detectedPullables.Remove(pullable);
        }
    }
    
    public Pullable GetClosestPullable(bool isFacingLeft)
    {
        if (_detectedPullables.Count == 0)
            return null;
        
        Pullable closestPullable = null;
        float closestDistanceSqr = float.MaxValue;
        
        foreach (Pullable pullable in _detectedPullables)
        {
            if (pullable == null)
                continue;
            
            // Skip immune pullables
            if (pullable.IsImmune)
                continue;
            
            Vector2 directionToPullable = pullable.transform.position - _playerTransform.position;
            
            // Early out: Check facing direction first (cheapest)
            bool isInFront = Vector2.Dot(directionToPullable, isFacingLeft ? Vector2.left : Vector2.right) >= 0;
            if (!isInFront)
                continue;
            
            // Early out: Check beneath player (cheap)
            if (IsBeneathPlayer(pullable))
                continue;
            
            // Early out: Box containment check before expensive raycast
            if (!IsWithinHighlightBox(pullable))
                continue;
            
            // Use squared distance for sorting (cheaper than actual distance)
            float distanceSqr = directionToPullable.sqrMagnitude;
            
            // Only raycast if all other checks pass
            if (distanceSqr < closestDistanceSqr && !IsBlocked(pullable))
            {
                closestDistanceSqr = distanceSqr;
                closestPullable = pullable;
            }
        }
        
        return closestPullable;
    }
    
    private bool IsWithinHighlightBox(Pullable pullable)
    {
        Vector2 playerPos = _playerTransform.position;
        Vector2 pullablePos = pullable.transform.position;
        Vector2 boxCenter = playerPos + _cachedBoxOffset;
        Vector2 halfSize = _cachedHighlightBoxSize * 0.5f;
        Vector2 localPos = pullablePos - boxCenter;
        
        return Mathf.Abs(localPos.x) <= halfSize.x && 
               Mathf.Abs(localPos.y) <= halfSize.y;
    }
    
    private bool IsBeneathPlayer(Pullable pullable)
    {
        Vector2 playerPos = _playerTransform.position;
        Vector2 pullablePos = pullable.transform.position;
        
        // Check if pullable is below player
        if (pullablePos.y >= playerPos.y)
            return false;
        
        // Check horizontal distance from player
        float horizontalDistance = Mathf.Abs(pullablePos.x - playerPos.x);
        
        // If within exclusion distance horizontally, it's beneath the player
        return horizontalDistance <= _beneathExclusionDistance;
    }
    
    private bool IsBlocked(Pullable pullable)
    {
        Vector2 playerPos = _playerTransform.position;
        Vector2 pullablePos = pullable.transform.position;
        Vector2 direction = (pullablePos - playerPos).normalized;
        float distance = Vector2.Distance(playerPos, pullablePos);
        
        RaycastHit2D hit = Physics2D.Raycast(playerPos, direction, distance, _blockingMask);
        
        return hit.collider != null;
    }
    
    public bool IsPullableBlocked(Pullable pullable)
    {
        if (pullable == null)
            return true;
        
        return IsBlocked(pullable);
    }
    
    public int GetDetectedCount()
    {
        return _detectedPullables.Count;
    }
}
