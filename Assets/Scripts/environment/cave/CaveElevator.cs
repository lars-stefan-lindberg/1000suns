using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveElevator : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidBody;

    [Header("Movement Settings")]
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _acceleration = 2f;
    [SerializeField] private float _deceleration = 3f;

    private float _currentSpeed = 0f;
    private bool _isMoving = false;
    private bool _hasStopPosition = false;
    private float _stopPositionY = 0f;
    private const float STOP_THRESHOLD = 0.01f;

    private void Awake()
    {
        if (_rigidBody == null)
            _rigidBody = GetComponent<Rigidbody2D>();
        
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
    }

    private void FixedUpdate()
    {
        if (_isMoving)
        {
            if (_hasStopPosition)
            {
                float currentY = transform.position.y;
                float distanceToStop = currentY - _stopPositionY;

                if (distanceToStop <= STOP_THRESHOLD)
                {
                    StopAtPosition();
                    return;
                }

                float decelerationDistance = (_currentSpeed * _currentSpeed) / (2f * _deceleration);

                if (distanceToStop <= decelerationDistance)
                {
                    _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, _deceleration * Time.fixedDeltaTime);
                }
                else
                {
                    _currentSpeed = Mathf.MoveTowards(_currentSpeed, _maxSpeed, _acceleration * Time.fixedDeltaTime);
                }

                _rigidBody.velocity = new Vector2(0, -_currentSpeed);
            }
            else
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _maxSpeed, _acceleration * Time.fixedDeltaTime);
                _rigidBody.velocity = new Vector2(0, -_currentSpeed);
            }
        }
    }

    public void StartMoving(bool instantMaxSpeed = false)
    {
        _isMoving = true;
        if (instantMaxSpeed)
            _currentSpeed = _maxSpeed;
    }

    public void StopAbruptly()
    {
        _isMoving = false;
        _currentSpeed = 0f;
        _rigidBody.velocity = Vector2.zero;
        _hasStopPosition = false;
    }

    public void SetStopPosition(float yPosition)
    {
        _stopPositionY = yPosition;
        _hasStopPosition = true;
    }

    public void ClearStopPosition()
    {
        _hasStopPosition = false;
    }

    private void StopAtPosition()
    {
        _isMoving = false;
        _currentSpeed = 0f;
        _rigidBody.velocity = Vector2.zero;
        _hasStopPosition = false;
    }

    public bool IsAtStopPosition()
    {
        return _hasStopPosition && Mathf.Abs(transform.position.y - _stopPositionY) <= STOP_THRESHOLD;
    }

    public bool HasReachedStop()
    {
        return !_isMoving && !_hasStopPosition;
    }
}
