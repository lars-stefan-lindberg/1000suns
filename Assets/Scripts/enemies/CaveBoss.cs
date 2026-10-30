using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class CaveBoss : MonoBehaviour
{
    public enum SpeedLevel {
        Slow,
        Medium,
        Fast
    }

    [Header("Speeds")]
    [SerializeField] private SpeedLevel _startingSpeed = SpeedLevel.Slow;
    [Tooltip("Average speed gained from the bursts over a full crawl cycle. Burst distance = speed * cycle duration.")]
    [SerializeField] private float _slowSpeed = 1.5f;
    [SerializeField] private float _mediumSpeed = 3f;
    [SerializeField] private float _fastSpeed = 5f;
    [Tooltip("Constant forward speed added on top of the bursts. Total average speed = burst speed + constant speed.")]
    [SerializeField] private float _slowConstantSpeed = 0f;
    [SerializeField] private float _mediumConstantSpeed = 0f;
    [SerializeField] private float _fastConstantSpeed = 0f;

    [Header("Crawl Cycle (Burst -> Rest)")]
    [Tooltip("Time the body spends lunging forward.")]
    [SerializeField, Min(0.02f)] private float _burstDuration = 0.25f;
    [Tooltip("Body progress (0 -> 1) over the burst.")]
    [SerializeField] private AnimationCurve _burstCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Idle time after a burst. Heads catch up during this time.")]
    [SerializeField, Min(0.01f)] private float _restDuration = 0.7f;

    [Header("Catch Up")]
    [SerializeField] private float _catchUpSpeed = 15f;
    [Tooltip("Horizontal distance the boss stops behind the player when catching up.")]
    [SerializeField] private float _catchUpOffset = 3f;
    [Tooltip("How fast the heads blend back to their resting offset when the crawl cycle is interrupted.")]
    [SerializeField] private float _blendSpeed = 8f;

    [System.Serializable]
    public class TentacleSlot {
        public CaveBossTentacleChain tentacle;
        [Tooltip("Object the tentacle reaches for. Keeps its offset from the body.")]
        public Transform target;
        [System.NonSerialized] public Vector2 offset;
    }

    [Header("Tentacles")]
    [SerializeField] private TentacleSlot[] _tentacles;

    [System.Serializable]
    public class HeadSlot {
        public Transform head;
        [Tooltip("Multiplies the global Head Lag for this head.")]
        [Range(0f, 2f)] public float lagMultiplier = 1f;
        [Tooltip("Extra seconds after the burst before this head starts catching up.")]
        [Min(0f)] public float catchUpDelay;
        [Tooltip("Trigger the \"move\" animation on the head's \"Body\" child Animator when the head starts moving forward.")]
        public bool useBodyAnimation;
        [System.NonSerialized] public Vector2 offset;
        [System.NonSerialized] public Vector2 blend;
        [System.NonSerialized] public Animator bodyAnimator;
        [System.NonSerialized] public int lastAnimatedBurst = -1;
    }

    private static readonly int MoveTrigger = Animator.StringToHash("move");

    [Header("Heads")]
    [SerializeField] private HeadSlot[] _heads;
    [Tooltip("Fraction of the burst movement the heads don't follow. 0 = no lag, 1 = heads stay in place during the burst.")]
    [SerializeField, Range(0f, 1f)] private float _headLag = 0.5f;
    [Tooltip("Time for the heads to catch up after the burst. Shortened if it wouldn't finish before the next burst.")]
    [SerializeField, Min(0.01f)] private float _headCatchUpDuration = 0.35f;
    [Tooltip("Catch-up progress (0 -> 1) of the heads.")]
    [SerializeField] private AnimationCurve _headCatchUpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("How far the heads rise mid catch-up.")]
    [SerializeField] private float _headArcHeight = 0.3f;

    [Header("Rumble")]
    [Tooltip("Whether rumbles occur. Can also be toggled with SetRumbleEnabled().")]
    [SerializeField] private bool _rumbleEnabled = true;
    [Tooltip("Minimum seconds between the end of one rumble and the start of the next.")]
    [SerializeField, Min(0f)] private float _rumbleMinInterval = 5f;
    [Tooltip("Maximum seconds between the end of one rumble and the start of the next.")]
    [SerializeField, Min(0f)] private float _rumbleMaxInterval = 10f;
    [Tooltip("How long the camera shakes and the earthquake sound plays before fading out.")]
    [SerializeField, Min(0f)] private float _rumbleDuration = 2f;
    [SerializeField] private float _rumbleShakeAmplitude = 1.94f;
    [SerializeField] private float _rumbleShakeFrequency = 1.84f;
    [SerializeField] private EventReference _earthquakeStinger;

    [Header("Debug")]
    [Tooltip("Freeze the boss in place (crawl cycle and catch-up paused) while tentacles keep animating. Can be toggled in Play mode.")]
    [SerializeField] private bool _debugStandStill;

    private Rigidbody2D _rigidBody;
    private SpeedLevel _currentSpeed;
    private bool _isCatchingUp;
    private bool _stopRequested;

    private float _cycleTime;
    private float _burstStartX;
    private float _burstDistance;
    private float _nextBurstDistance;
    private int _burstIndex;

    private Coroutine _rumbleCoroutine;
    private EventInstance _rumbleInstance;

    public SpeedLevel CurrentSpeed => _currentSpeed;
    public bool IsCatchingUp => _isCatchingUp;
    public bool RumbleEnabled => _rumbleEnabled;
    // True once a requested stop has taken effect (the current burst has finished)
    public bool IsStopped => _stopRequested && !_isCatchingUp && _cycleTime >= _burstDuration;

    private float CycleDuration => _burstDuration + _restDuration;

    void Awake() {
        _rigidBody = GetComponent<Rigidbody2D>();
        _currentSpeed = _startingSpeed;

        Vector2 body = transform.position;
        foreach (var slot in _tentacles)
            if (slot.target != null) slot.offset = (Vector2)slot.target.position - body;
        foreach (var slot in _heads) {
            if (slot.head == null) continue;
            slot.offset = (Vector2)slot.head.position - body;
            if (!slot.useBodyAnimation) continue;
            Transform bodyChild = slot.head.Find("Body");
            slot.bodyAnimator = bodyChild != null ? bodyChild.GetComponent<Animator>() : null;
            if (slot.bodyAnimator == null)
                Debug.LogWarning($"CaveBoss: head '{slot.head.name}' has Use Body Animation enabled but no 'Body' child with an Animator.", slot.head);
        }

        ResetCycle();
    }

    void OnEnable() {
        _rumbleCoroutine = StartCoroutine(RumbleLoop());
    }

    void OnDisable() {
        if (_rumbleCoroutine != null) StopCoroutine(_rumbleCoroutine);
        _rumbleCoroutine = null;
        AudioUtils.SafeStop(ref _rumbleInstance);
    }

    void Update()
    {
        Vector2 body = transform.position;
        // Smooth cycle time between physics steps
        float t = _debugStandStill ? _cycleTime : Mathf.Min(_cycleTime + Time.time - Time.fixedTime, CycleDuration);
        float decay = Mathf.Exp(-_blendSpeed * Time.deltaTime);

        foreach (var slot in _tentacles) {
            if (slot.target == null) continue;
            SetPosition(slot.target, body + slot.offset);
            if (slot.tentacle != null) slot.tentacle.SetTarget(slot.target.position);
        }

        foreach (var slot in _heads) {
            if (slot.head == null) continue;
            slot.blend *= decay;
            Vector2 procedural = _isCatchingUp ? Vector2.zero : GetHeadOffset(slot, t);
            SetPosition(slot.head, body + slot.offset + slot.blend + procedural);
            UpdateHeadAnimation(slot, t);
        }
    }

    // Trigger once per burst, when the head starts catching up (moving forward)
    private void UpdateHeadAnimation(HeadSlot slot, float t) {
        if (slot.bodyAnimator == null || slot.lastAnimatedBurst == _burstIndex) return;
        if (_isCatchingUp || _burstDistance <= 0f || _headLag * slot.lagMultiplier <= 0f) return;
        float delay = Mathf.Min(slot.catchUpDelay, CycleDuration - _burstDuration);
        if (t < _burstDuration + delay) return;
        slot.lastAnimatedBurst = _burstIndex;
        slot.bodyAnimator.SetTrigger(MoveTrigger);
    }

    void FixedUpdate()
    {
        float velocityX = _debugStandStill ? 0f : _isCatchingUp ? GetCatchUpVelocity() : GetCrawlVelocity();
        _rigidBody.velocity = new Vector2(velocityX, _rigidBody.velocity.y);
    }

    public void SetSpeed(SpeedLevel speed) {
        _currentSpeed = speed;
    }

    // Comes to a full stop once the current burst has finished (immediately if resting)
    public void StopMoving() {
        _stopRequested = true;
    }

    public void StartMoving() {
        _stopRequested = false;
    }

    // Disabling lets a rumble that's already playing finish, but no new ones start
    public void SetRumbleEnabled(bool enabled) {
        _rumbleEnabled = enabled;
    }

    public void CatchUp() {
        if (_isCatchingUp) return;
        _isCatchingUp = true;

        // Keep current positions as a displacement that blends back to the resting offset
        Vector2 body = transform.position;
        foreach (var slot in _heads)
            if (slot.head != null) slot.blend = (Vector2)slot.head.position - body - slot.offset;
    }

    private float GetCatchUpVelocity() {
        Transform player = PlayerManager.obj.GetPlayerTransform(PlayerManager.obj.GetActivePlayerType());
        float distanceToTarget = player.position.x - _catchUpOffset - _rigidBody.position.x;
        if (distanceToTarget <= 0) {
            _isCatchingUp = false;
            ResetCycle();
            return 0f;
        }
        // Clamp so the boss doesn't overshoot the offset point in a single physics step
        return Mathf.Min(_catchUpSpeed, distanceToTarget / Time.fixedDeltaTime);
    }

    private float GetCrawlVelocity() {
        // When stopped, rest time still runs (so heads finish catching up) but is held at the end of the cycle
        bool stopped = IsStopped;
        float previous = _cycleTime;
        _cycleTime += Time.fixedDeltaTime;
        if (stopped) _cycleTime = Mathf.Min(_cycleTime, CycleDuration);
        // The constant speed moves the burst baseline forward, so bursts are layered on top of it
        else _burstStartX += GetConstantSpeed(_currentSpeed) * Time.fixedDeltaTime;

        // Lock the next burst distance when resting starts, so speed changes never alter a burst in progress
        if (previous < _burstDuration && _cycleTime >= _burstDuration)
            _nextBurstDistance = GetSpeed(_currentSpeed) * CycleDuration;

        if (!stopped && _cycleTime >= CycleDuration) {
            _cycleTime -= CycleDuration;
            _burstStartX = _rigidBody.position.x;
            _burstDistance = _nextBurstDistance;
            _burstIndex++;
        }

        float targetX = _burstStartX + _burstDistance * GetBurstProgress(_cycleTime);
        return (targetX - _rigidBody.position.x) / Time.fixedDeltaTime;
    }

    private float GetBurstProgress(float t) {
        return t >= _burstDuration ? 1f : _burstCurve.Evaluate(t / _burstDuration);
    }

    // Start resting in place, as if a zero-length burst just finished
    private void ResetCycle() {
        _cycleTime = _burstDuration;
        _burstStartX = _rigidBody.position.x;
        _burstDistance = 0f;
        _nextBurstDistance = GetSpeed(_currentSpeed) * CycleDuration;
    }

    // Heads only lag behind the burst part of the movement, not the constant speed
    private Vector2 GetHeadOffset(HeadSlot slot, float t) {
        float moved = _burstDistance * GetBurstProgress(t);
        float lag = moved * _headLag * slot.lagMultiplier;
        if (t < _burstDuration) return new Vector2(-lag, 0f);

        float window = CycleDuration - _burstDuration;
        float delay = Mathf.Min(slot.catchUpDelay, window);
        float duration = Mathf.Min(_headCatchUpDuration, window - delay);
        float q = duration > 0f ? Mathf.Clamp01((t - _burstDuration - delay) / duration) : 1f;
        float arc = lag > 0f ? _headArcHeight * Mathf.Sin(q * Mathf.PI) : 0f;
        return new Vector2(-lag * (1f - _headCatchUpCurve.Evaluate(q)), arc);
    }

    private static void SetPosition(Transform target, Vector2 position) {
        target.position = new Vector3(position.x, position.y, target.position.z);
    }

    private float GetSpeed(SpeedLevel speed) {
        switch (speed) {
            case SpeedLevel.Medium: return _mediumSpeed;
            case SpeedLevel.Fast: return _fastSpeed;
            default: return _slowSpeed;
        }
    }

    private float GetConstantSpeed(SpeedLevel speed) {
        switch (speed) {
            case SpeedLevel.Medium: return _mediumConstantSpeed;
            case SpeedLevel.Fast: return _fastConstantSpeed;
            default: return _slowConstantSpeed;
        }
    }

    private IEnumerator RumbleLoop() {
        while (true) {
            float wait = Random.Range(_rumbleMinInterval, _rumbleMaxInterval);
            // The timer only runs while enabled and restarts when disabled, so enabling always waits a full interval
            for (float t = 0f; t < wait; t = _rumbleEnabled ? t + Time.deltaTime : 0f)
                yield return null;
            yield return Rumble(_rumbleDuration);
        }
    }

    //Shakes the camera and plays earthquake sfx
    private IEnumerator Rumble(float duration) {
        CameraShakeManager.obj.ShakeCamera(_rumbleShakeAmplitude, _rumbleShakeFrequency, duration);
        _rumbleInstance = SoundFXManager.obj.CreateAttachedInstance(_earthquakeStinger, gameObject, null);
        _rumbleInstance.start();
        _rumbleInstance.release();
        yield return new WaitForSeconds(duration);
        AudioUtils.SafeStop(ref _rumbleInstance);
    }
}
