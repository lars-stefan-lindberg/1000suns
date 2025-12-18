using System.Collections.Generic;
using UnityEngine;

public class AnchorPointDetector : MonoBehaviour
{
    public bool isAnchorPointDetected = false;
    private HashSet<CircleCollider2D> _anchorPoints = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("AnchorPoint")) {
            _anchorPoints.Add(collision.GetComponent<CircleCollider2D>());
            isAnchorPointDetected = true;
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
        float closestDistance = float.MaxValue;
        foreach (CircleCollider2D anchorPoint in _anchorPoints) {
            //First check if the anchorPoint is in front of the object
            Vector2 directionToAnchor = anchorPoint.transform.position - obj.position;
            bool isAnchorInFront = Vector2.Dot(directionToAnchor, isFacingLeft ? Vector2.left : Vector2.right) >= 0;
            if (!isAnchorInFront) continue;
            
            float distance = Vector3.Distance(obj.position, anchorPoint.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestAnchorPoint = anchorPoint;
            }
        }
        return closestAnchorPoint;
    }
}
