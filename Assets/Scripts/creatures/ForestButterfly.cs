using UnityEngine;
using System.Collections;

public class ForestButterfly : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] private float _hoverTime = 2f;
    [SerializeField] private float _hoverDistance = 0.3f;
    [SerializeField] private float _hoverSpeed = 1f;

    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed = 2f;
    [SerializeField] private float _movementLength = 1.5f;
    [SerializeField] private float _maxRadius = 5f;

    [Header("Player Proximity Settings")]
    [SerializeField] private float _playerDetectionRadius = 3f;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private float _fleeSpeedMultiplier = 1.5f;
    [SerializeField] private float _fleeLengthMultiplier = 1.3f;
    [SerializeField] private float _recoveryHoverTime = 1f;

    [Header("Ground Detection Settings")]
    [SerializeField] private float _groundDetectionDistance = 1f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _returnToStartSpeed = 3f;

    [Header("Initial Settings")]
    [SerializeField] private float _initialCycleDelay = 0f;
    [SerializeField] private Vector2 _initialDirection = Vector2.right;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _startPosition;
    private Vector3 _currentDirection;
    private bool _isHovering = true;
    private float _hoverTimer = 0f;
    private Vector3 _hoverStartPos;
    private Transform _playerTransform;
    private bool _wasPlayerNearbyAtStateStart = false;
    private bool _isRecovering = false;
    private bool _isReturningToStart = false;
    private Coroutine _behaviorCycleCoroutine;

    private enum ButterflyState
    {
        Hovering,
        Moving,
        Recovering,
        ReturningToStart
    }

    private ButterflyState _currentState = ButterflyState.Hovering;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        _startPosition = transform.position;
        _currentDirection = _initialDirection.normalized;

        _animator.SetTrigger("fly");

        _behaviorCycleCoroutine = StartCoroutine(ButterflyBehaviorCycle());
    }

    void Update()
    {
        if (!_isReturningToStart)
        {
            CheckForPlayer();
            CheckForGround();
        }
    }

    private void CheckForPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, _playerDetectionRadius, _playerLayer);
        
        if (playerCollider != null)
        {
            _playerTransform = playerCollider.transform;
        }
        else
        {
            _playerTransform = null;
        }
    }

    private void CheckForGround()
    {
        if (_isReturningToStart)
            return;
            
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, _groundDetectionDistance, _groundLayer);
        
        if (hit.collider != null)
        {
            if (_behaviorCycleCoroutine != null)
            {
                StopCoroutine(_behaviorCycleCoroutine);
            }
            
            _isReturningToStart = true;
            StartCoroutine(ReturnToStartPosition());
        }
    }

    private IEnumerator ButterflyBehaviorCycle()
    {
        if (_initialCycleDelay > 0f)
        {
            yield return new WaitForSeconds(_initialCycleDelay);
        }

        while (true)
        {
            yield return StartCoroutine(HoverState());
            yield return StartCoroutine(MovementState());
        }
    }

    private IEnumerator HoverState()
    {
        _currentState = ButterflyState.Hovering;
        _hoverStartPos = transform.position;
        float elapsedTime = 0f;
        _wasPlayerNearbyAtStateStart = IsPlayerNearby();

        while (elapsedTime < _hoverTime)
        {
            if (_isReturningToStart)
            {
                yield break;
            }

            bool isPlayerNearbyNow = IsPlayerNearby();
            if (isPlayerNearbyNow && !_wasPlayerNearbyAtStateStart)
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float hoverOffset = Mathf.Sin(elapsedTime * _hoverSpeed * Mathf.PI * 2f) * _hoverDistance;
            transform.position = _hoverStartPos + Vector3.up * hoverOffset;
            yield return null;
        }
    }

    private IEnumerator MovementState()
    {
        _currentState = ButterflyState.Moving;

        bool isPlayerNearby = IsPlayerNearby();
        bool isFleeing = isPlayerNearby;
        
        if (isFleeing)
        {
            Vector3 directionAwayFromPlayer = (transform.position - _playerTransform.position).normalized;
            _currentDirection = GetClosestDiagonalDirection(directionAwayFromPlayer);
        }
        else
        {
            Vector3 directionToStart = (_startPosition - transform.position);
            float distanceFromStart = directionToStart.magnitude;

            if (distanceFromStart > _maxRadius)
            {
                _currentDirection = directionToStart.normalized;
            }
            else
            {
                _currentDirection = GetRandomDiagonalDirection();
            }
        }

        UpdateSpriteFlip();

        float actualSpeed = isFleeing ? _movementSpeed * _fleeSpeedMultiplier : _movementSpeed;
        float actualLength = isFleeing ? _movementLength * _fleeLengthMultiplier : _movementLength;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + _currentDirection * actualLength;
        float journeyLength = Vector3.Distance(startPos, targetPos);
        float startTime = Time.time;

        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            if (_isReturningToStart)
            {
                yield break;
            }

            bool isPlayerNearbyNow = IsPlayerNearby();
            if (isPlayerNearbyNow && !isFleeing)
            {
                isFleeing = true;
                
                Vector3 directionAwayFromPlayer = (transform.position - _playerTransform.position).normalized;
                _currentDirection = GetClosestDiagonalDirection(directionAwayFromPlayer);
                
                UpdateSpriteFlip();
                
                actualSpeed = _movementSpeed * _fleeSpeedMultiplier;
                actualLength = _movementLength * _fleeLengthMultiplier;
                
                startPos = transform.position;
                targetPos = startPos + _currentDirection * actualLength;
                journeyLength = Vector3.Distance(startPos, targetPos);
                startTime = Time.time;
            }

            float distanceCovered = (Time.time - startTime) * actualSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);

            float hoverOffset = Mathf.Sin(Time.time * _hoverSpeed * Mathf.PI * 4f) * (_hoverDistance * 0.5f);
            transform.position += Vector3.up * hoverOffset;

            yield return null;
        }

        transform.position = targetPos;
        
        if (isFleeing)
        {
            _isRecovering = true;
            bool recoveryInterrupted = false;
            yield return StartCoroutine(RecoveryHoverState(interrupted => recoveryInterrupted = interrupted));
            _isRecovering = false;
            
            if (recoveryInterrupted)
            {
                yield return StartCoroutine(MovementState());
            }
        }
    }

    private IEnumerator RecoveryHoverState(System.Action<bool> onComplete)
    {
        _currentState = ButterflyState.Recovering;
        _hoverStartPos = transform.position;
        float elapsedTime = 0f;
        bool interrupted = false;

        while (elapsedTime < _recoveryHoverTime)
        {
            if (IsPlayerNearby())
            {
                interrupted = true;
                break;
            }

            elapsedTime += Time.deltaTime;
            float hoverOffset = Mathf.Sin(elapsedTime * _hoverSpeed * Mathf.PI * 2f) * _hoverDistance;
            transform.position = _hoverStartPos + Vector3.up * hoverOffset;
            yield return null;
        }
        
        onComplete?.Invoke(interrupted);
    }

    private IEnumerator ReturnToStartPosition()
    {
        _currentState = ButterflyState.ReturningToStart;
        
        Vector3 startPos = transform.position;
        float journeyLength = Vector3.Distance(startPos, _startPosition);
        float startTime = Time.time;

        while (Vector3.Distance(transform.position, _startPosition) > 0.01f)
        {
            float distanceCovered = (Time.time - startTime) * _returnToStartSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            transform.position = Vector3.Lerp(startPos, _startPosition, fractionOfJourney);

            float hoverOffset = Mathf.Sin(Time.time * _hoverSpeed * Mathf.PI * 4f) * (_hoverDistance * 0.5f);
            transform.position += Vector3.up * hoverOffset;

            Vector3 directionToStart = (_startPosition - transform.position).normalized;
            if (directionToStart.x < 0)
            {
                _spriteRenderer.flipX = true;
            }
            else if (directionToStart.x > 0)
            {
                _spriteRenderer.flipX = false;
            }

            yield return null;
        }

        transform.position = _startPosition;
        
        _isReturningToStart = false;
        _playerTransform = null;
        
        _behaviorCycleCoroutine = StartCoroutine(ButterflyBehaviorCycle());
    }

    private Vector3 GetRandomDiagonalDirection()
    {
        int randomIndex = Random.Range(0, 4);
        
        switch (randomIndex)
        {
            case 0: return new Vector3(1, 1, 0).normalized;
            case 1: return new Vector3(1, -1, 0).normalized;
            case 2: return new Vector3(-1, 1, 0).normalized;
            case 3: return new Vector3(-1, -1, 0).normalized;
            default: return new Vector3(1, 1, 0).normalized;
        }
    }

    private Vector3 GetClosestDiagonalDirection(Vector3 targetDirection)
    {
        Vector3[] diagonalDirections = new Vector3[]
        {
            new Vector3(1, 1, 0).normalized,
            new Vector3(1, -1, 0).normalized,
            new Vector3(-1, 1, 0).normalized,
            new Vector3(-1, -1, 0).normalized
        };

        Vector3 closestDirection = diagonalDirections[0];
        float maxDot = Vector3.Dot(targetDirection, diagonalDirections[0]);

        for (int i = 1; i < diagonalDirections.Length; i++)
        {
            float dot = Vector3.Dot(targetDirection, diagonalDirections[i]);
            if (dot > maxDot)
            {
                maxDot = dot;
                closestDirection = diagonalDirections[i];
            }
        }

        return closestDirection;
    }

    private void UpdateSpriteFlip()
    {
        if (_spriteRenderer != null)
        {
            if (_currentDirection.x < 0)
            {
                _spriteRenderer.flipX = true;
            }
            else if (_currentDirection.x > 0)
            {
                _spriteRenderer.flipX = false;
            }
        }
    }

    private bool IsPlayerNearby()
    {
        return _playerTransform != null;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? _startPosition : transform.position;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, _maxRadius);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _playerDetectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * _groundDetectionDistance);
    }
}
