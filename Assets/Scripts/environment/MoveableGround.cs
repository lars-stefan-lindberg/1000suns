using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Moves a kinematic Rigidbody2D (for example a tilemap platform) linearly between a start and an end position on command.
// Movement is done through the rigidbody's velocity, so a Moveable component on the collider object passes the
// platform's velocity on to the player standing on top of it (see CheckCollisions in the player movement scripts).
public class MoveableGround : MonoBehaviour
{
    public enum Position { Start, End }

    private const float DefaultSpeed = 4f;
    private const float GizmoMarkerRadius = 0.3f;

    [Tooltip("Kinematic rigidbody to move. Should be the one referenced by the Moveable component on the collider object.")]
    [SerializeField] private Rigidbody2D _rigidbody;
    [Tooltip("World position of the rigidbody when at the start position.")]
    [SerializeField] private Vector2 _startPosition;
    [Tooltip("World position of the rigidbody when at the end position.")]
    [SerializeField] private Vector2 _endPosition;
    [Tooltip("Constant movement speed in units per second.")]
    [SerializeField] private float _speed = DefaultSpeed;
    [Tooltip("Position the ground is placed at when the scene loads.")]
    [SerializeField] private Position _initialPosition = Position.Start;

    [Tooltip("Invoked when the ground has arrived at the start position.")]
    [SerializeField] private UnityEvent _reachedStart;
    [Tooltip("Invoked when the ground has arrived at the end position.")]
    [SerializeField] private UnityEvent _reachedEnd;

    private Position _targetPosition;
    private bool _isMoving;
    // Set on the step where the remaining distance is covered, so the ground is stopped and snapped on the next step
    private bool _isArriving;

    /// <summary>True while the ground is moving towards a position.</summary>
    public bool IsMoving => _isMoving;

    /// <summary>The position the ground is at, or moving towards.</summary>
    public Position TargetPosition => _targetPosition;

    void Awake() {
        SnapTo(_initialPosition);
    }

    void OnDisable() {
        Stop();
    }

    /// <summary>Starts moving the ground towards the end position.</summary>
    public void MoveToEnd() {
        MoveTo(Position.End);
    }

    /// <summary>Starts moving the ground towards the start position.</summary>
    public void MoveToStart() {
        MoveTo(Position.Start);
    }

    /// <summary>Starts moving the ground towards the opposite of its current target position.</summary>
    public void Toggle() {
        MoveTo(_targetPosition == Position.Start ? Position.End : Position.Start);
    }

    /// <summary>Starts moving the ground towards the given position. Can be called mid movement to reverse.</summary>
    public void MoveTo(Position position) {
        _targetPosition = position;
        _isArriving = false;
        _isMoving = _rigidbody.position != GetWorldPosition(position);
    }

    /// <summary>Starts moving towards the given position and waits until it has arrived, or the ground got disabled.</summary>
    public IEnumerator MoveToAndWait(Position position) {
        MoveTo(position);
        while(_isMoving)
            yield return null;
    }

    /// <summary>Activates the ground object and instantly places it at the given position.</summary>
    public void Spawn(Position position) {
        // Activating runs Awake the first time, which snaps to the initial position, so snap afterwards
        gameObject.SetActive(true);
        SnapTo(position);
    }

    /// <summary>Stops the ground and deactivates its object.</summary>
    public void Despawn() {
        gameObject.SetActive(false);
    }

    /// <summary>Instantly places the ground at the given position, e.g. when respawning mid boss fight.</summary>
    public void SnapTo(Position position) {
        Stop();
        _targetPosition = position;
        _rigidbody.position = GetWorldPosition(position);
        _rigidbody.transform.position = GetWorldPosition(position);
    }

    void FixedUpdate() {
        if(!_isMoving)
            return;

        if(_isArriving) {
            Arrive();
            return;
        }

        Vector2 targetWorldPosition = GetWorldPosition(_targetPosition);
        Vector2 remaining = targetWorldPosition - _rigidbody.position;
        float stepDistance = _speed * Time.fixedDeltaTime;

        if(remaining.magnitude <= stepDistance) {
            // Cover exactly the remaining distance this step, so the player still gets a matching velocity
            _rigidbody.velocity = remaining / Time.fixedDeltaTime;
            _isArriving = true;
        } else {
            _rigidbody.velocity = remaining.normalized * _speed;
        }
    }

    private void Arrive() {
        Stop();
        // Removes any floating point drift from the last step
        _rigidbody.position = GetWorldPosition(_targetPosition);

        if(_targetPosition == Position.Start)
            _reachedStart?.Invoke();
        else
            _reachedEnd?.Invoke();
    }

    private void Stop() {
        _isMoving = false;
        _isArriving = false;
        if(_rigidbody != null)
            _rigidbody.velocity = Vector2.zero;
    }

    private Vector2 GetWorldPosition(Position position) {
        return position == Position.Start ? _startPosition : _endPosition;
    }

    [ContextMenu("Set Start Position From Current")]
    private void SetStartPositionFromCurrent() {
        _startPosition = _rigidbody.transform.position;
    }

    [ContextMenu("Set End Position From Current")]
    private void SetEndPositionFromCurrent() {
        _endPosition = _rigidbody.transform.position;
    }

    void OnDrawGizmosSelected() {
        if(_rigidbody == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_startPosition, GizmoMarkerRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_endPosition, GizmoMarkerRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_startPosition, _endPosition);

        // Outline of the collider at both positions
        Collider2D groundCollider = _rigidbody.GetComponent<Collider2D>();
        if(groundCollider == null)
            return;
        Vector2 currentPosition = _rigidbody.transform.position;
        Bounds bounds = groundCollider.bounds;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center + (Vector3)(_startPosition - currentPosition), bounds.size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center + (Vector3)(_endPosition - currentPosition), bounds.size);
    }
}
