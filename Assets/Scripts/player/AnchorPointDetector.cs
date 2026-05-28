using System.Collections.Generic;
using UnityEngine;

public class AnchorPointDetector : MonoBehaviour
{
    public bool isAnchorPointDetected = false;
    private HashSet<CircleCollider2D> _anchorPoints = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("AnchorPoint")) {
            var circleCollider = collision.GetComponent<CircleCollider2D>();
            if (circleCollider != null) {
                _anchorPoints.Add(circleCollider);
                isAnchorPointDetected = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("AnchorPoint")) {
            _anchorPoints.Remove(collision.GetComponent<CircleCollider2D>());
            isAnchorPointDetected = _anchorPoints.Count > 0;
        }
    }

    public CircleCollider2D GetClosestFacingAnchorPoint(Transform obj, bool isFacingLeft) {
        if(_anchorPoints.Count == 0) return null;
        
        CircleCollider2D closestAnchorPoint = null;
        float closestDistanceSqr = float.MaxValue;
        foreach (CircleCollider2D anchorPoint in _anchorPoints) {
            if (anchorPoint == null) continue;
            
            Vector2 directionToAnchor = anchorPoint.transform.position - obj.position;
            
            // Early out: Check if the anchorPoint is in front of the object
            bool isAnchorInFront = Vector2.Dot(directionToAnchor, isFacingLeft ? Vector2.left : Vector2.right) >= 0;
            if (!isAnchorInFront) continue;
            
            // Use squared distance to avoid expensive sqrt
            float distanceSqr = directionToAnchor.sqrMagnitude;
            if (distanceSqr < closestDistanceSqr) {
                closestDistanceSqr = distanceSqr;
                closestAnchorPoint = anchorPoint;
            }
        }
        return closestAnchorPoint;
    }
}
