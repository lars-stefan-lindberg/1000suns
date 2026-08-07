using System.Collections.Generic;
using UnityEngine;

public class AnchorPointDetector : MonoBehaviour
{
    public bool isAnchorPointDetected = false;
    private HashSet<BoxCollider2D> _anchorPoints = new();
    
    [SerializeField] private LayerMask _blockingMask;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("AnchorPoint")) {
            var boxCollider = collision.GetComponent<BoxCollider2D>();
            if (boxCollider != null) {
                _anchorPoints.Add(boxCollider);
                isAnchorPointDetected = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("AnchorPoint")) {
            _anchorPoints.Remove(collision.GetComponent<BoxCollider2D>());
            isAnchorPointDetected = _anchorPoints.Count > 0;
        }
    }

    public BoxCollider2D GetClosestFacingAnchorPoint(Transform obj, bool isFacingLeft) {
        if(_anchorPoints.Count == 0) return null;
        
        BoxCollider2D closestAnchorPoint = null;
        float closestDistanceSqr = float.MaxValue;
        foreach (BoxCollider2D anchorPoint in _anchorPoints) {
            if (anchorPoint == null) continue;
            
            Vector2 directionToAnchor = anchorPoint.transform.position - obj.position;
            
            // Early out: Check if the anchorPoint is in front of the object
            bool isAnchorInFront = Vector2.Dot(directionToAnchor, isFacingLeft ? Vector2.left : Vector2.right) >= 0;
            if (!isAnchorInFront) continue;
            
            // Early out: Check if the path to the anchor point is blocked
            if (IsBlocked(obj.position, anchorPoint.transform.position)) continue;
            
            // Use squared distance to avoid expensive sqrt
            float distanceSqr = directionToAnchor.sqrMagnitude;
            if (distanceSqr < closestDistanceSqr) {
                closestDistanceSqr = distanceSqr;
                closestAnchorPoint = anchorPoint;
            }
        }
        return closestAnchorPoint;
    }
    
    private bool IsBlocked(Vector2 fromPos, Vector2 toPos)
    {
        Vector2 direction = (toPos - fromPos).normalized;
        float distance = Vector2.Distance(fromPos, toPos);
        
        RaycastHit2D hit = Physics2D.Raycast(fromPos, direction, distance, _blockingMask);
        
        return hit.collider != null;
    }
}
