using System.Collections;
using UnityEngine;

public class FlyingAvatarCreature : MonoBehaviour
{
    [SerializeField] private float _maxDropDistance = 1f;
    [SerializeField] private float _flapForce = 1f;
    [SerializeField] private float _flapDuration = 0.25f;
    [SerializeField] private float _flapLiftDelay = 0.1f;
    [SerializeField] private float _idleGravity = 1f;
    [SerializeField] private float _maxFallSpeed = 1.5f;
    [SerializeField] private float _maxHorizontalDeviation = 1.5f;
    [SerializeField] private float _maxHorizontalNudge = 0.3f;
    [SerializeField] private float _maxRiseDistance = 0.75f;
    [SerializeField, Range(0f,1f)] private float _randomFlapChancePerSecond = 0.3f;
    [SerializeField] private float _minFlapRise = 0.1f;
    [SerializeField] private float _upperOvershootDistance = 0.15f;
    [SerializeField] private float _hitFlyOffHorizontal = 0.8f;
    [SerializeField] private float _hitFlyOffUpward = 0.4f;
    [SerializeField] private float _hitFlyOffSpeed = 3f;
    [SerializeField] private float _hitCooldown = 1.0f;

    private Vector2 _startPos;
    private Animator _animator;
    private bool _isFlapping;
    private float _verticalVelocity;
    private bool _isFlapPending;
    private float _autoFlapEnableTime;
    private Coroutine _runningFlap;
    private int _flapCancelToken;
    private int _blockPositionWritesFrame = -1;
    

    void Awake()
    {
        _startPos = transform.position;
        _animator = GetComponent<Animator>();
        // Removed initial drop offset; randomness handled by flap box chance
        _autoFlapEnableTime = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.CompareTag("Projectile")) {
            bool hitFromTheLeft = PlayerManager.obj.GetPlayerTransform().position.x < transform.position.x;
            TriggerHitFlap(hitFromTheLeft);
        }
    }

    void FixedUpdate()
    {
        // Apply idle gravity when not flapping
        if (!_isFlapping)
        {
            _verticalVelocity -= _idleGravity * Time.fixedDeltaTime;
            _verticalVelocity = Mathf.Max(_verticalVelocity, -_maxFallSpeed);
            transform.position += Vector3.up * (_verticalVelocity * Time.fixedDeltaTime);
        }

        if(!_isFlapping && !_isFlapPending && Time.time >= _autoFlapEnableTime)
        {
            float lowerY = _startPos.y - _maxDropDistance;
            float upperY = _startPos.y + _maxRiseDistance;
            float y = transform.position.y;
            // Mandatory flap at/below max drop distance
            if (y <= lowerY)
            {
                Flap();
            }
            // Only allow random flaps if within the vertical box and falling, with room for a minimum rise
            else if (y < upperY - _minFlapRise && y > lowerY && _verticalVelocity <= 0f)
            {
                float perFrameChance = 1f - Mathf.Pow(1f - _randomFlapChancePerSecond, Time.fixedDeltaTime);
                if (Random.value < perFrameChance)
                {
                    Flap();
                }
            }
            // If above upper bound, do not flap; gravity will bring it back
        }
    }

    private void Flap() {
        // Block any flap attempts during hit cooldown
        if (Time.time < _autoFlapEnableTime) return;
        if (_isFlapping || _isFlapPending) return;
        _isFlapPending = true;
        if (_animator != null)
        {
            _animator.SetTrigger("flap");
        }
        // Invalidate any prior flap targets
        _flapCancelToken++;
        _runningFlap = StartCoroutine(FlapCoroutine());
    }

    private IEnumerator FlapCoroutine() {
        // Move the transform up, going from fast to slow velocity
        int token = _flapCancelToken; // capture before delay
        yield return new WaitForSeconds(_flapLiftDelay);
        if (token != _flapCancelToken) { _isFlapping = false; _isFlapPending = false; _runningFlap = null; _verticalVelocity = 0f; yield break; }
        // Start of actual lift: pause gravity influence during the lift
        _verticalVelocity = 0f;
        _isFlapping = true;
        _isFlapPending = false;
        Vector3 start = transform.position;
        // Determine horizontal nudge, biased toward start position and clamped to max deviation (box)
        float offsetFromStart = start.x - _startPos.x;
        float distanceToLimit = _maxHorizontalDeviation - Mathf.Abs(offsetFromStart);
        distanceToLimit = Mathf.Max(0f, distanceToLimit);
        // Bias direction toward center as we approach/overstep the limit
        int centerDir = offsetFromStart > 0f ? -1 : (offsetFromStart < 0f ? 1 : 0);
        float bias = Mathf.InverseLerp(0f, _maxHorizontalDeviation, Mathf.Abs(offsetFromStart)); // 0 near center, 1 near limit
        float rnd = Random.value;
        int randomDir = rnd < 0.5f ? -1 : 1;
        int chosenDir = centerDir == 0 ? randomDir : (rnd < (0.5f + 0.5f * bias) ? centerDir : randomDir);
        float desiredNudgeMag = Random.Range(0f, _maxHorizontalNudge);
        // Allow nudging outside the horizontal box; bias aims back toward center when outside
        float horizontalNudge = (centerDir == 0 ? chosenDir : centerDir) * desiredNudgeMag;
        float targetX = start.x + horizontalNudge;
        // Ensure a minimum vertical rise so flaps are never tiny or purely horizontal
        float targetY = start.y + Mathf.Max(_flapForce, _minFlapRise);
        // Cap within the vertical box bounds so we don't drift upward indefinitely
        float lowerY = _startPos.y - _maxDropDistance;
        float upperY = _startPos.y + _maxRiseDistance;
        // Allow a small overshoot above the upper bound to keep flap distance meaningful near the top
        float upperCap = upperY + Mathf.Max(0f, _upperOvershootDistance);
        targetY = Mathf.Clamp(targetY, lowerY, upperCap);
        // If clamping removed rise (e.g., near upper bound), try to add at least _minFlapRise within cap
        if (targetY <= start.y)
        {
            targetY = Mathf.Min(upperCap, start.y + _minFlapRise);
        }
        Vector3 target = new Vector3(targetX, targetY, start.z);
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, _flapDuration);
        while (elapsed < duration)
        {
            if (token != _flapCancelToken) { _isFlapping = false; _isFlapPending = false; _runningFlap = null; _verticalVelocity = 0f; yield break; }
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutCubic(t);
            if (Time.frameCount == _blockPositionWritesFrame) { yield return null; continue; }
            if (token != _flapCancelToken) { _isFlapping = false; _isFlapPending = false; _runningFlap = null; _verticalVelocity = 0f; yield break; }
            transform.position = Vector3.LerpUnclamped(start, target, eased);
            yield return null;
        }
        if (token != _flapCancelToken) { _isFlapping = false; _isFlapPending = false; _runningFlap = null; _verticalVelocity = 0f; yield break; }
        transform.position = target;
        _verticalVelocity = 0f;
        _isFlapping = false;
        _runningFlap = null;
    }

    private void TriggerHitFlap(bool hitFromLeft)
    {
        // Disable auto-flaps for cooldown duration
        _autoFlapEnableTime = Time.time + _hitCooldown;
        // Interrupt any pending normal flap
        if (_isFlapPending) _isFlapPending = false;
        // Invalidate any prior flap targets BEFORE stopping running coroutine to avoid race on final assignment
        _flapCancelToken++;
        // Block any position writes for the rest of this frame
        _blockPositionWritesFrame = Time.frameCount;
        // Stop all coroutines on this behaviour to ensure no orphaned flap continues
        StopAllCoroutines();
        _runningFlap = null;
        _isFlapping = false;
        _isFlapPending = false;
        // Play hit anim
        if (_animator != null)
        {
            _animator.SetTrigger("hit");
        }
        _runningFlap = StartCoroutine(FlapCoroutineHit(hitFromLeft ? 1f : -1f));
    }

    private IEnumerator FlapCoroutineHit(float dir)
    {
        // Immediate reactive flap with fixed distance and speed
        _verticalVelocity = 0f;
        _isFlapping = true;
        _isFlapPending = false;
        int token = _flapCancelToken;

        Vector3 start = transform.position;
        Vector3 target = new Vector3(
            start.x + dir * Mathf.Abs(_hitFlyOffHorizontal),
            start.y + Mathf.Max(_hitFlyOffUpward, _minFlapRise),
            start.z
        );

        float totalDist = Vector2.Distance(new Vector2(start.x, start.y), new Vector2(target.x, target.y));
        float duration = Mathf.Max(0.01f, totalDist / Mathf.Max(0.01f, _hitFlyOffSpeed));
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (token != _flapCancelToken) { _isFlapping = false; _isFlapPending = false; _runningFlap = null; _verticalVelocity = 0f; yield break; }
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutCubic(t);
            transform.position = Vector3.LerpUnclamped(start, target, eased);
            yield return null;
        }
        if (token != _flapCancelToken) { _isFlapping = false; _isFlapPending = false; _runningFlap = null; _verticalVelocity = 0f; yield break; }
        transform.position = target;
        _verticalVelocity = 0f;
        _isFlapping = false;
        _runningFlap = null;
    }

    private bool IsAtMinimumDropHeight() {
        return transform.position.y <= _startPos.y - _maxDropDistance;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}
