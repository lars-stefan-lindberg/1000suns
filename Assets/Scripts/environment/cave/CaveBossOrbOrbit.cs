using UnityEngine;

// Moves the magical orbs clockwise in a circle around a fixed center (the big magical orb), evenly spaced.
// Each orb owns a slot on the circle. An orb out of the circle (grabbed, in play, returning or consumed) leaves its
// slot empty while the others keep moving. A returning orb flies to its slot's current position and rejoins the circle.
public class CaveBossOrbOrbit : MonoBehaviour
{
    [Tooltip("Center of the circle, e.g. the big magical orb. It doesn't move.")]
    [SerializeField] private Transform _center;
    [Tooltip("Orbs in clockwise order. They get evenly spaced slots on the circle.")]
    [SerializeField] private CaveBossOrb[] _orbs;
    [Tooltip("Radius of the circle in units.")]
    [SerializeField, Min(0f)] private float _radius = 3.5f;
    [Tooltip("Rotation speed in degrees per second, clockwise. Negative values rotate counterclockwise.")]
    [SerializeField] private float _degreesPerSecond = 30f;
    [Tooltip("Angle in degrees of the first orb's slot when the room starts (0 = right, 90 = up, -90 = down).")]
    [SerializeField] private float _startAngle = -90f;

    private const float FullCircleDegrees = 360f;
    private const int GizmoCircleSegments = 48;
    private const float GizmoSlotRadius = 0.3f;

    private float _currentAngle;

    void Start() {
        // Runs after the orbs' Awake, so their initial home position is overridden with their slot
        _currentAngle = _startAngle;
        for (int i = 0; i < _orbs.Length; i++) {
            if (_orbs[i] != null && _orbs[i].isActiveAndEnabled)
                _orbs[i].SnapToHomePosition(GetSlotPosition(_currentAngle, i));
        }
    }

    void FixedUpdate() {
        // Clockwise means a decreasing angle
        _currentAngle = Mathf.Repeat(_currentAngle - _degreesPerSecond * Time.fixedDeltaTime, FullCircleDegrees);
        for (int i = 0; i < _orbs.Length; i++) {
            if (_orbs[i] != null && _orbs[i].isActiveAndEnabled)
                _orbs[i].FollowHomePosition(GetSlotPosition(_currentAngle, i));
        }
    }

    // World position of an orb's slot for the given rotation of the circle
    private Vector2 GetSlotPosition(float circleAngle, int slotIndex) {
        float slotAngle = (circleAngle - slotIndex * FullCircleDegrees / _orbs.Length) * Mathf.Deg2Rad;
        return (Vector2)_center.position + new Vector2(Mathf.Cos(slotAngle), Mathf.Sin(slotAngle)) * _radius;
    }

    // Shows the circle and the slots (at the start angle in edit mode) when selected
    void OnDrawGizmosSelected() {
        if (_center == null) return;

        Gizmos.color = Color.cyan;
        Vector3 center = _center.position;
        Vector3 previousPoint = center + Vector3.right * _radius;
        for (int i = 1; i <= GizmoCircleSegments; i++) {
            float angle = i * FullCircleDegrees / GizmoCircleSegments * Mathf.Deg2Rad;
            Vector3 point = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * _radius;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        if (_orbs == null || _orbs.Length == 0) return;
        float circleAngle = Application.isPlaying ? _currentAngle : _startAngle;
        for (int i = 0; i < _orbs.Length; i++)
            Gizmos.DrawWireSphere(GetSlotPosition(circleAngle, i), GizmoSlotRadius);
    }
}
