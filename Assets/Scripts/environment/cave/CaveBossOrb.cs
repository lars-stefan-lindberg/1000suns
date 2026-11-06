using System.Collections.Generic;
using UnityEngine;

// Magical orb used against CaveBossFight.
// Dee grabs it with the shadow pull, which puts it "in play" (the other orbs can't be grabbed meanwhile).
// Released it hangs in the air. While in play, a force push sends it flying horizontally.
// Hits the boss -> consumed. Touches a tentacle or gets too far to the right -> back to its start position.
// Setup: Rigidbody2D (Kinematic, gravity 0, rotation frozen), a non trigger collider and a Pullable (on this object or a child).
public class CaveBossOrb : MonoBehaviour
{
    [Header("Push")]
    [Tooltip("Horizontal distance the orb flies when force pushed.")]
    [SerializeField] private float _flyDistance = 8f;
    [SerializeField] private float _flySpeed = 14f;
    [Tooltip("How fast the orb slows down after being released from the pull.")]
    [SerializeField] private float _deceleration = 20f;

    [Header("Reset")]
    [Tooltip("The orb is reset if it gets this far to the right of its start position.")]
    [SerializeField] private float _maxDistanceRight = 15f;

    // Shared state of all active orbs: only one can be in play, and grabbing can be disabled (e.g. before the fight starts)
    private static readonly List<CaveBossOrb> _orbs = new List<CaveBossOrb>();
    private static bool _grabbingEnabled;
    public static CaveBossOrb InPlay { get; private set; }

    private Rigidbody2D _rigidBody;
    private Pullable _pullable;
    private Vector2 _startPosition;
    private bool _wasPulled;
    private bool _isFlying;
    private float _flyStartX;
    private int _flyDirection;

    private bool IsInPlay => InPlay == this;

    public static void SetGrabbingEnabled(bool enabled) {
        _grabbingEnabled = enabled;
        foreach (var orb in _orbs) orb.UpdateGrabbable();
    }

    private static void SetInPlay(CaveBossOrb orb) {
        InPlay = orb;
        foreach (var other in _orbs) other.UpdateGrabbable();
    }

    private void UpdateGrabbable() {
        if (!IsInPlay) _pullable.SetImmune(!_grabbingEnabled || InPlay != null);
    }

    void Awake() {
        _rigidBody = GetComponent<Rigidbody2D>();
        _pullable = GetComponentInChildren<Pullable>();
        _startPosition = _rigidBody.position;
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
    }

    void FixedUpdate() {
        bool isPulled = _pullable.IsPulled;
        if (isPulled && !_wasPulled) OnGrabbed();
        _wasPulled = isPulled;

        if (_isFlying)
            Fly();
        else if (!isPulled && _rigidBody.bodyType == RigidbodyType2D.Dynamic) {
            _rigidBody.velocity = Vector2.MoveTowards(_rigidBody.velocity, Vector2.zero, _deceleration * Time.fixedDeltaTime);
            if (_rigidBody.velocity == Vector2.zero) Hang();
        }

        if (IsInPlay && _rigidBody.position.x > _startPosition.x + _maxDistanceRight)
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
        } else if (_isFlying) {
            CaveBossFight boss = other.GetComponentInParent<CaveBossFight>();
            if (boss == null || !boss.IsHurtbox(other)) return;
            if (boss.TryTakeHit()) Consume();
            else ResetOrb();
        }
    }

    public void ResetOrb() {
        if (!IsInPlay) return;
        ReleaseFromPull();
        Hang();
        _rigidBody.position = _startPosition;
        transform.position = new Vector3(_startPosition.x, _startPosition.y, transform.position.z);
        SetInPlay(null);
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
