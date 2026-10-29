using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float _slowSpeed = 1.5f;
    [SerializeField] private float _mediumSpeed = 3f;
    [SerializeField] private float _fastSpeed = 5f;

    [Header("Catch Up")]
    [SerializeField] private float _catchUpSpeed = 15f;
    [Tooltip("Horizontal distance the boss stops behind the player when catching up.")]
    [SerializeField] private float _catchUpOffset = 3f;

    [System.Serializable]
    public class TentacleSlot {
        public CaveBossTentacleChain tentacle;
        [Tooltip("Object the tentacle reaches for. Drag it around in the Scene view to test.")]
        public Transform target;
    }

    [Header("Tentacles")]
    [SerializeField] private TentacleSlot[] _tentacles;

    private Rigidbody2D _rigidBody;
    private SpeedLevel _currentSpeed;
    private bool _isCatchingUp;

    public SpeedLevel CurrentSpeed => _currentSpeed;
    public bool IsCatchingUp => _isCatchingUp;

    void Awake() {
        _rigidBody = GetComponent<Rigidbody2D>();
        _currentSpeed = _startingSpeed;
    }

    void Update()
    {
        foreach (var slot in _tentacles) {
            if (slot.tentacle != null && slot.target != null)
                slot.tentacle.SetTarget(slot.target.position);
        }
    }

    void FixedUpdate()
    {
        float velocityX = _isCatchingUp ? GetCatchUpVelocity() : GetSpeed(_currentSpeed);
        //_rigidBody.velocity = new Vector2(velocityX, _rigidBody.velocity.y);
    }

    public void SetSpeed(SpeedLevel speed) {
        _currentSpeed = speed;
    }

    public void CatchUp() {
        _isCatchingUp = true;
    }

    private float GetCatchUpVelocity() {
        Transform player = PlayerManager.obj.GetPlayerTransform(PlayerManager.obj.GetActivePlayerType());
        float distanceToTarget = player.position.x - _catchUpOffset - _rigidBody.position.x;
        if (distanceToTarget <= 0) {
            _isCatchingUp = false;
            return GetSpeed(_currentSpeed);
        }
        // Clamp so the boss doesn't overshoot the offset point in a single physics step
        return Mathf.Min(_catchUpSpeed, distanceToTarget / Time.fixedDeltaTime);
    }

    private float GetSpeed(SpeedLevel speed) {
        switch (speed) {
            case SpeedLevel.Medium: return _mediumSpeed;
            case SpeedLevel.Fast: return _fastSpeed;
            default: return _slowSpeed;
        }
    }
}
