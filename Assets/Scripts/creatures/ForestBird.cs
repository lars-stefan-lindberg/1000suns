using System.Collections;
using UnityEngine;
using FMODUnity;

public class ForestBird : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField] private float _detectionRadius = 3f;
    [SerializeField] private LayerMask _playerLayer;
    
    [Header("Flight Settings")]
    [SerializeField] private float _flightSpeed = 5f;
    [SerializeField] private float _flightAngle = 45f;
    [SerializeField] private float _flightDurationBeforeDestroy = 3f;
    
    [Header("Jump Settings")]
    [SerializeField] private float _jumpHeight = 0.5f;
    [SerializeField] private float _jumpDistance = 0.3f;
    [SerializeField] private float _jumpSpeed = 3f;
    
    [Header("Cycle Settings")]
    [SerializeField] private bool _enableMovement = true;
    [SerializeField] private bool _includePicking = true;
    [SerializeField] private float _timeBetweenPicks = 0.3f;
    [SerializeField] private float _waitTimeBeforeNewLoop = 1f;
    [SerializeField] private float _initialOffsetTime = 0f;
    [SerializeField] private int _startingDirection = 1;
    
    [Header("Idle Animation Settings")]
    [SerializeField] private float _minIdleAnimationInterval = 2f;
    [SerializeField] private float _maxIdleAnimationInterval = 5f;
    
    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Sfx")]
    [SerializeField] private EventReference _wingFlap;
    [SerializeField] private EventReference _pick;
    
    private bool _isFlyingAway = false;
    private bool _isInCycle = false;
    private Vector3 _flightDirection;
    private int _currentDirection = 1;
    private Transform _playerTransform;
    private bool _isInitialized = false;
    private Coroutine _cycleCoroutine;
    
    void Start()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
        
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        
        _currentDirection = _startingDirection >= 0 ? 1 : -1;
        _spriteRenderer.flipX = _currentDirection < 0;
        _isInitialized = true;
        
        _cycleCoroutine = StartCoroutine(StartCycleWithDelay());
    }

    void OnEnable()
    {
        if (_isInitialized && !_isFlyingAway && _cycleCoroutine == null)
        {
            _cycleCoroutine = StartCoroutine(StartCycleWithDelay());
        }
    }

    void OnDisable()
    {
        _cycleCoroutine = null;
    }

    void Update()
    {
        if (_isFlyingAway)
        {
            transform.position += _flightDirection * _flightSpeed * Time.deltaTime;
            return;
        }
        
        CheckForPlayer();
    }

    public void PlayPickSfx() {
        SoundFXManager.obj.PlayAtGameObject(_pick, gameObject);
    }
    
    private void CheckForPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, _detectionRadius, _playerLayer);
        
        if (playerCollider != null)
        {
            _playerTransform = playerCollider.transform;
            StartFlyingAway();
        }
    }
    
    private void StartFlyingAway()
    {
        if (_isFlyingAway)
            return;
        
        StopAllCoroutines();
        _isFlyingAway = true;
        _isInCycle = false;
        
        Vector3 directionAwayFromPlayer = (transform.position - _playerTransform.position).normalized;
        
        float angleInRadians = _flightAngle * Mathf.Deg2Rad;
        _flightDirection = new Vector3(
            directionAwayFromPlayer.x * Mathf.Cos(angleInRadians),
            Mathf.Sin(angleInRadians),
            0f
        ).normalized;
        
        _spriteRenderer.flipX = _flightDirection.x < 0;
        
        if (_animator != null)
            _animator.SetTrigger("fly");
        
        SoundFXManager.obj.PlayAtGameObject(_wingFlap, gameObject);
        
        StartCoroutine(DestroyAfterFlightDuration());
    }
    
    private IEnumerator DestroyAfterFlightDuration()
    {
        yield return new WaitForSeconds(_flightDurationBeforeDestroy);
        
        StopAllCoroutines();
        
        if (_animator != null)
            _animator.enabled = false;
        
        Destroy(gameObject);
    }
    
    private IEnumerator StartCycleWithDelay()
    {
        if (_initialOffsetTime > 0f)
        {
            yield return new WaitForSeconds(_initialOffsetTime);
        }
        
        yield return StartCoroutine(BirdCycle());
    }
    
    private IEnumerator BirdCycle()
    {
        while (!_isFlyingAway)
        {
            _isInCycle = true;
            
            if (!_enableMovement)
            {
                yield return StartCoroutine(IdleAnimationCycle());
                continue;
            }
            
            yield return StartCoroutine(PerformJumps(2));
            if (_isFlyingAway) yield break;
            
            float elapsed = 0f;
            while (elapsed < _waitTimeBeforeNewLoop && !_isFlyingAway)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (_isFlyingAway) yield break;
            
            if (_includePicking)
            {
                yield return StartCoroutine(PerformPicks(2));
                if (_isFlyingAway) yield break;
                
                elapsed = 0f;
                while (elapsed < _waitTimeBeforeNewLoop && !_isFlyingAway)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
            
            _currentDirection *= -1;
            _spriteRenderer.flipX = _currentDirection < 0;
            
            _isInCycle = false;
            yield return null;
        }
    }
    
    private IEnumerator PerformJumps(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return StartCoroutine(Jump());
        }
    }
    
    private IEnumerator Jump()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(_jumpDistance * _currentDirection, 0f, 0f);
        
        float elapsed = 0f;
        float duration = _jumpDistance / _jumpSpeed;
        
        while (elapsed < duration && !_isFlyingAway)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float height = Mathf.Sin(t * Mathf.PI) * _jumpHeight;
            
            Vector3 newPos = Vector3.Lerp(startPos, targetPos, t);
            newPos.y += height;
            
            transform.position = newPos;
            
            yield return null;
        }
        
        if (!_isFlyingAway)
            transform.position = targetPos;
    }
    
    private IEnumerator PerformPicks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_isFlyingAway)
                yield break;
            
            if (_animator != null)
                _animator.SetTrigger("pick");
            
            float elapsed = 0f;
            while (elapsed < _timeBetweenPicks && !_isFlyingAway)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
    
    private IEnumerator IdleAnimationCycle()
    {
        float waitTime = Random.Range(_minIdleAnimationInterval, _maxIdleAnimationInterval);
        float elapsed = 0f;
        
        while (elapsed < waitTime && !_isFlyingAway)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (_isFlyingAway)
            yield break;
        
        if (_animator != null)
        {
            bool shouldFlap = Random.value > 0.5f;
            if (shouldFlap)
                _animator.SetTrigger("flap");
            else
                _animator.SetTrigger("turnHead");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
    }
}
