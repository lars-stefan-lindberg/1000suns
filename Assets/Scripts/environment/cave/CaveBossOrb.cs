using System;
using System.Collections.Generic;
using UnityEngine;

// Magical orb used against CaveBossFight.
// Dee grabs it with the shadow pull, which puts it "in play" (the other orbs can't be grabbed meanwhile).
// Released it hangs in the air. While in play, a force push sends it flying horizontally.
// Hits the boss -> consumed. Touches a tentacle or gets too far to the right -> back to its home position.
// The home position is fixed by default, or driven by a CaveBossOrbOrbit (the orb then circles while resting).
// A reserve orb (e.g. the big orb for the final stage) can't be grabbed until all regular orbs are used up and
// UnlockReserveOrbsIfRegularOrbsUsedUp is called.
// Setup: Rigidbody2D (Kinematic, gravity 0, rotation frozen), a non trigger collider and a Pullable (on this object or a child).
public class CaveBossOrb : MonoBehaviour
{
    [Header("Reserve")]
    [Tooltip("Locked (can't be grabbed) until all regular orbs have been used up and the boss unlocks it, e.g. the big orb for the final stage.")]
    [SerializeField] private bool _isReserve;

    [Header("Push")]
    [Tooltip("Horizontal distance the orb flies when force pushed.")]
    [SerializeField] private float _flyDistance = 8f;
    [SerializeField] private float _flySpeed = 14f;
    [Tooltip("How fast the orb slows down after being released from the pull.")]
    [SerializeField] private float _deceleration = 20f;

    [Header("Reset")]
    [Tooltip("The orb is reset if it gets this far to the right of its home position.")]
    [SerializeField] private float _maxDistanceRight = 15f;

    [Header("Return Movement")]
    [Tooltip("Normal speed (units per second) the orb flies back to its home position when reset.")]
    [SerializeField] private float _returnSpeed = 10f;
    [Tooltip("Speed (units per second) right after the orb is touched, easing into the normal return speed.")]
    [SerializeField] private float _returnBurstSpeed = 22f;
    [Tooltip("Distance (units) over which the burst speed eases into the normal return speed.")]
    [SerializeField] private float _returnBurstDistance = 3f;
    [Tooltip("Distance (units) from the home position where the orb starts slowing down.")]
    [SerializeField] private float _returnSlowdownDistance = 1.5f;
    [Tooltip("Speed (units per second) the orb has slowed down to when it reaches the home position.")]
    [SerializeField] private float _returnArrivalSpeed = 4f;

    // Distance at which the returning orb snaps to its home position
    private const float ReturnArrivalThreshold = 0.01f;
    // Lower bound for the return speed so a misconfigured arrival speed can't leave the orb stuck
    private const float MinReturnSpeed = 0.5f;

    // Shared state of all active orbs: only one can be in play, and grabbing can be disabled (e.g. before the fight starts)
    private static readonly List<CaveBossOrb> _orbs = new List<CaveBossOrb>();
    private static bool _grabbingEnabled;
    public static CaveBossOrb InPlay { get; private set; }

    /// <summary>
    /// Raised when the orb in play touches a tentacle hazard and resets. Passes the hazard collider that was touched.
    /// </summary>
    public static event Action<Collider2D> HitByTentacle;

    private Rigidbody2D _rigidBody;
    private Pullable _pullable;
    private Collider2D[] _colliders;
    private Vector2 _homePosition;
    // Speed (units per second) the home position currently moves at, added to the return speed so a moving home can be caught
    private float _homeSpeed;
    private bool _wasPulled;
    private bool _isFlying;
    private float _flyStartX;
    private int _flyDirection;
    private bool _isReturning;
    private Vector2 _returnFromPosition;
    private bool _isReserveUnlocked;

    private bool IsInPlay => InPlay == this;
    private bool IsLocked => _isReserve && !_isReserveUnlocked;

    /// <summary>
    /// True while the orb rests at its home position: not in play, not grabbed, not flying and not returning.
    /// </summary>
    public bool IsResting => !_isReturning && !IsInPlay && !_isFlying && !_pullable.IsPulled
        && _rigidBody.bodyType == RigidbodyType2D.Kinematic;

    public static void SetGrabbingEnabled(bool enabled) {
        _grabbingEnabled = enabled;
        foreach (var orb in _orbs) orb.UpdateGrabbable();
    }

    /// <summary>
    /// Unlocks the reserve orbs if no regular orb is left (consumed orbs are disabled and no longer count).
    /// Returns true if the reserve orbs are unlocked after the call.
    /// </summary>
    public static bool UnlockReserveOrbsIfRegularOrbsUsedUp() {
        foreach (var orb in _orbs)
            if (!orb._isReserve) return false;
        foreach (var orb in _orbs) {
            orb._isReserveUnlocked = true;
            orb.UpdateGrabbable();
        }
        return true;
    }

    private static void SetInPlay(CaveBossOrb orb) {
        InPlay = orb;
        foreach (var other in _orbs) other.UpdateGrabbable();
    }

    private void UpdateGrabbable() {
        if (!IsInPlay) _pullable.SetImmune(_isReturning || IsLocked || !_grabbingEnabled || InPlay != null);
    }

    void Awake() {
        _rigidBody = GetComponent<Rigidbody2D>();
        _pullable = GetComponentInChildren<Pullable>();
        _colliders = GetComponentsInChildren<Collider2D>();
        _homePosition = _rigidBody.position;
        _rigidBody.gravityScale = 0f;
        // Kinematic bodies need this to get trigger callbacks from the boss hurtbox and tentacle colliders
        _rigidBody.useFullKinematicContacts = true;
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
    }

    void OnEnable() {
        _orbs.Add(this);
        UpdateGrabbable();
    }

    void OnDisable() {
        _orbs.Remove(this);
        if (IsInPlay) SetInPlay(null);
        // Complete an interrupted return instantly so the orb isn't left without colliders
        if (_isReturning) {
            TeleportTo(_homePosition);
            FinishReturn();
        }
    }

    /// <summary>
    /// Instantly moves the home position, and the orb itself if it's resting. Use for the initial placement.
    /// </summary>
    public void SnapToHomePosition(Vector2 homePosition) {
        _homePosition = homePosition;
        _homeSpeed = 0f;
        if (IsResting) TeleportTo(homePosition);
    }

    /// <summary>
    /// Moves the home position. A resting orb follows it, a returning orb flies towards its new location.
    /// Call once per FixedUpdate so the home movement speed can be tracked.
    /// </summary>
    public void FollowHomePosition(Vector2 homePosition) {
        _homeSpeed = Vector2.Distance(_homePosition, homePosition) / Time.fixedDeltaTime;
        _homePosition = homePosition;
        if (IsResting) _rigidBody.MovePosition(homePosition);
    }

    void FixedUpdate() {
        if (_isReturning) {
            ReturnToHome();
            return;
        }

        bool isPulled = _pullable.IsPulled;
        if (isPulled && !_wasPulled) OnGrabbed();
        _wasPulled = isPulled;

        if (_isFlying)
            Fly();
        else if (!isPulled && _rigidBody.bodyType == RigidbodyType2D.Dynamic) {
            _rigidBody.velocity = Vector2.MoveTowards(_rigidBody.velocity, Vector2.zero, _deceleration * Time.fixedDeltaTime);
            if (_rigidBody.velocity == Vector2.zero) Hang();
        }

        if (IsInPlay && _rigidBody.position.x > _homePosition.x + _maxDistanceRight)
            ResetOrb();
    }

    private void OnGrabbed() {
        _isFlying = false;
        _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        if (!IsInPlay) SetInPlay(this);
    }

    private void Launch(int direction) {
        _isFlying = true;
        _flyDirection = direction;
        _flyStartX = _rigidBody.position.x;
        _rigidBody.bodyType = RigidbodyType2D.Dynamic;
    }

    private void Fly() {
        if (Mathf.Abs(_rigidBody.position.x - _flyStartX) >= _flyDistance) {
            Hang();
            return;
        }
        _rigidBody.velocity = new Vector2(_flyDirection * _flySpeed, 0f);
    }

    // Stops and stays in the air
    private void Hang() {
        _isFlying = false;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (_isFlying) Hang();
    }

    // The boss hurtbox and tentacle hazard colliders must be on a layer that collides with the orb's layer (e.g. Enemies)
    void OnTriggerEnter2D(Collider2D other) {
        if (!IsInPlay) return;

        if (other.CompareTag("Projectile")) {
            if (!_pullable.IsPulled) Launch(other.GetComponent<Projectile>().horizontalDirection);
        } else if (CaveBossTentacleChain.IsHazard(other)) {
            ResetOrb();
            HitByTentacle?.Invoke(other);
        } else if (_isFlying) {
            CaveBossFight boss = other.GetComponentInParent<CaveBossFight>();
            if (boss == null || !boss.IsHurtbox(other)) return;
            if (boss.TryTakeHit()) Consume();
            else ResetOrb();
        }
    }

    /// <summary>
    /// Takes the orb out of play and makes it fly back to its home position at the return speed.
    /// The orb can't be grabbed until it has arrived.
    /// </summary>
    public void ResetOrb() {
        if (!IsInPlay) return;
        ReleaseFromPull();
        Hang();
        _isReturning = true;
        _returnFromPosition = _rigidBody.position;
        // No collisions on the way back, so the orb doesn't bump into the player or other objects
        SetCollidersEnabled(false);
        // Out of play right away so the boss stops targeting it, but it stays immune while returning
        SetInPlay(null);
    }

    // Moves the kinematic body towards the (possibly moving) home position, finishing the reset on arrival
    private void ReturnToHome() {
        float speed = GetReturnSpeed();
        Vector2 nextPosition = Vector2.MoveTowards(_rigidBody.position, _homePosition, speed * Time.fixedDeltaTime);
        if (Vector2.Distance(nextPosition, _homePosition) <= ReturnArrivalThreshold) {
            TeleportTo(_homePosition);
            FinishReturn();
            return;
        }
        _rigidBody.MovePosition(nextPosition);
    }

    // Burst speed easing into the normal speed after leaving, then easing down to the arrival speed near the home position.
    // The home movement speed is added on top so the orb always closes in on a moving home.
    private float GetReturnSpeed() {
        float travelledDistance = Vector2.Distance(_returnFromPosition, _rigidBody.position);
        float remainingDistance = Vector2.Distance(_rigidBody.position, _homePosition);

        float burstProgress = _returnBurstDistance > 0f ? Mathf.Clamp01(travelledDistance / _returnBurstDistance) : 1f;
        float speed = Mathf.Lerp(_returnBurstSpeed, _returnSpeed, Mathf.SmoothStep(0f, 1f, burstProgress));

        if (remainingDistance < _returnSlowdownDistance) {
            float slowdownProgress = 1f - remainingDistance / _returnSlowdownDistance;
            speed = Mathf.Lerp(speed, _returnArrivalSpeed, Mathf.SmoothStep(0f, 1f, slowdownProgress));
        }

        // Never stall before reaching the home position
        return Mathf.Max(speed, MinReturnSpeed) + _homeSpeed;
    }

    // Restores collisions and grabbing once the orb is back at its home position
    private void FinishReturn() {
        _isReturning = false;
        SetCollidersEnabled(true);
        UpdateGrabbable();
    }

    private void TeleportTo(Vector2 position) {
        _rigidBody.position = position;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    private void SetCollidersEnabled(bool enabled) {
        foreach (var orbCollider in _colliders) orbCollider.enabled = enabled;
    }

    // Disabling also takes it out of play
    private void Consume() {
        ReleaseFromPull();
        Hang();
        gameObject.SetActive(false);
    }

    private void ReleaseFromPull() {
        if (_pullable.IsPulled) ShadowTwinPull.obj.CancelPulling();
        _wasPulled = false;
    }
}
