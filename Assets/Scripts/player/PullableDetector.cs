using System.Collections.Generic;
using UnityEngine;

public class PullableDetector : MonoBehaviour
{
    private HashSet<Pullable> _detectedPullables = new HashSet<Pullable>();
    private Transform _playerTransform;
    private CircleCollider2D _circleCollider;
    
    [SerializeField] private LayerMask _blockingMask;
    [SerializeField] private float _beneathExclusionDistance = 1.5f;
    [SerializeField] private float _triggerRadiusBuffer = 1f;
    
    private float _cachedHighlightRange;
    private float _highlightRangeSqr;
    
    private void Awake()
    {
        _playerTransform = transform.parent;
        _circleCollider = GetComponent<CircleCollider2D>();
        
        if (_circleCollider == null)
        {
            Debug.LogWarning("PullableDetector: CircleCollider2D not found! Trigger detection will not work.");
        }
        else
        {
            UpdateCircleColliderRadius();
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
            _cachedHighlightRange = ShadowTwinPull.obj.GetHighlightRange();
            _highlightRangeSqr = _cachedHighlightRange * _cachedHighlightRange;
        }
    }
    
    private void UpdateCircleColliderRadius()
    {
        if (_circleCollider != null && ShadowTwinPull.obj != null)
        {
            _circleCollider.radius = ShadowTwinPull.obj.GetControlRange() + _triggerRadiusBuffer;
        }
    }
    
    private void OnValidate()
    {
        // Update collider radius when values change in editor
        if (_circleCollider == null)
            _circleCollider = GetComponent<CircleCollider2D>();
            
        UpdateCircleColliderRadius();
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
            
            Vector2 directionToPullable = pullable.transform.position - _playerTransform.position;
            
            // Early out: Check facing direction first (cheapest)
            bool isInFront = Vector2.Dot(directionToPullable, isFacingLeft ? Vector2.left : Vector2.right) >= 0;
            if (!isInFront)
                continue;
            
            // Early out: Check beneath player (cheap)
            if (IsBeneathPlayer(pullable))
                continue;
            
            // Use squared distance to avoid expensive sqrt
            float distanceSqr = directionToPullable.sqrMagnitude;
            
            // Early out: Range check before expensive raycast
            if (distanceSqr > _highlightRangeSqr)
                continue;
            
            // Only raycast if all other checks pass
            if (distanceSqr < closestDistanceSqr && !IsBlocked(pullable))
            {
                closestDistanceSqr = distanceSqr;
                closestPullable = pullable;
            }
        }
        
        return closestPullable;
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
