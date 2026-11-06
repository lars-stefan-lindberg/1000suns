using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Random = UnityEngine.Random;

// Stationary cave boss for the boss fight room. Glides in, attacks with tentacles while the player
// hits it with CaveBossOrbs, grows guard tentacles after each hit and retreats to the left after the last hit.
// Hits needed = number of stages; stage index = hits taken so far.
public class CaveBossFight : MonoBehaviour
{
    [Serializable]
    public class Stage {
        [Tooltip("Guard tentacles that grow out when this stage starts.")]
        public CaveBossGuardTentacle[] guardTentacles;
        [Min(0.1f)] public float minAttackInterval = 3f;
        [Min(0.1f)] public float maxAttackInterval = 5f;
    }

    private enum State { Idle, Entering, Fighting, HitReaction, Retreating }

    [Header("Movement")]
    [Tooltip("Where the boss stops after entering. Only X is used.")]
    [SerializeField] private Transform _fightPosition;
    [SerializeField] private float _enterSpeed = 2f;
    [SerializeField] private float _retreatSpeed = 1.5f;
    [Tooltip("Distance the boss retreats to the left before despawning.")]
    [SerializeField] private float _retreatDistance = 15f;

    [Header("Stages (index = hits taken)")]
    [SerializeField] private Stage[] _stages = new Stage[3];

    [Header("Attack")]
    [SerializeField] private CaveBossAttackTentacle _attackTentaclePrefab;
    [Tooltip("Attack tentacles spawn at this X (inside the body), at the target's Y.")]
    [SerializeField] private Transform _attackRoot;

    [Header("Hurtbox")]
    [Tooltip("Trigger collider the orbs have to hit.")]
    [SerializeField] private Collider2D _hurtbox;

    [Header("Hit Reaction")]
    [Tooltip("Shaken on hit. Body and heads should be children of it, the attack root and tentacles shouldn't.")]
    [SerializeField] private Transform _visual;
    [SerializeField] private SpriteRenderer[] _flashRenderers;
    [SerializeField] private Material _flashMaterial;
    [SerializeField, Min(1)] private int _flashCount = 3;
    [SerializeField] private float _flashDuration = 0.08f;
    [Tooltip("Time the boss shakes and doesn't attack after a hit.")]
    [SerializeField] private float _hitReactionDuration = 1.2f;
    [SerializeField] private float _hitShakeStrength = 0.3f;
    [SerializeField] private float _retreatShakeStrength = 0.15f;
    [SerializeField] private int _shakeVibrato = 20;

    [Header("Rumble")]
    [Tooltip("Seconds between rumbles while fighting.")]
    [SerializeField, Min(0f)] private float _rumbleMinInterval = 15f;
    [SerializeField, Min(0f)] private float _rumbleMaxInterval = 25f;
    [SerializeField, Min(0f)] private float _rumbleDuration = 2f;
    [SerializeField] private float _rumbleShakeAmplitude = 1.94f;
    [SerializeField] private float _rumbleShakeFrequency = 1.84f;
    [SerializeField] private EventReference _earthquakeStinger;

    public event Action Defeated;

    private State _state = State.Idle;
    private int _hits;
    private readonly List<CaveBossAttackTentacle> _attackTentacles = new List<CaveBossAttackTentacle>();
    private Material[] _originalMaterials;
    private Vector3 _visualStartPosition;
    private Coroutine _attackRoutine;
    private Coroutine _rumbleRoutine;
    private EventInstance _rumbleInstance;

    private Stage CurrentStage => _stages[Mathf.Min(_hits, _stages.Length - 1)];

    void Awake() {
        _originalMaterials = new Material[_flashRenderers.Length];
        for (int i = 0; i < _flashRenderers.Length; i++)
            _originalMaterials[i] = _flashRenderers[i].sharedMaterial;
        _visualStartPosition = _visual.localPosition;
        CaveBossOrb.SetGrabbingEnabled(false);
    }

    void OnDisable() {
        AudioUtils.SafeStop(ref _rumbleInstance);
    }

    void OnDestroy() {
        _visual.DOKill();
    }

    public bool IsHurtbox(Collider2D other) => other == _hurtbox;

    public void PlaceAtFightPosition() {
        SetX(_fightPosition.position.x);
    }

    public IEnumerator Enter() {
        _state = State.Entering;
        yield return MoveToX(_fightPosition.position.x, _enterSpeed);
        _state = State.Idle;
    }

    public void StartFighting(float firstAttackDelay) {
        _state = State.Fighting;
        ActivateStage(_hits);
        CaveBossOrb.SetGrabbingEnabled(true);
        _attackRoutine = StartCoroutine(AttackLoop(firstAttackDelay));
        if (_rumbleRoutine == null) _rumbleRoutine = StartCoroutine(RumbleLoop());
    }

    // Returns false if the boss can't be hit right now
    public bool TryTakeHit() {
        if (_state != State.Fighting) return false;
        StartCoroutine(HitReaction());
        return true;
    }

    private IEnumerator HitReaction() {
        _state = State.HitReaction;
        _hits++;
        bool defeated = _hits >= _stages.Length;
        if (defeated) CaveBossOrb.SetGrabbingEnabled(false);

        if (_attackRoutine != null) StopCoroutine(_attackRoutine);
        _attackTentacles.RemoveAll(t => t == null);
        foreach (var tentacle in _attackTentacles) tentacle.Retract();

        StartCoroutine(Flash());
        Shake(_hitShakeStrength, _hitReactionDuration, false);
        yield return new WaitForSeconds(_hitReactionDuration);

        if (defeated) {
            yield return Retreat();
            yield break;
        }

        ActivateStage(_hits);
        _state = State.Fighting;
        _attackRoutine = StartCoroutine(AttackLoop(GetAttackInterval()));
    }

    private IEnumerator Retreat() {
        _state = State.Retreating;
        foreach (var stage in _stages)
            foreach (var guard in stage.guardTentacles) guard.Retract();

        Shake(_retreatShakeStrength, 1f, true);
        yield return MoveToX(transform.position.x - _retreatDistance, _retreatSpeed);
        Defeated?.Invoke();
        Destroy(gameObject);
    }

    private IEnumerator AttackLoop(float firstDelay) {
        yield return new WaitForSeconds(firstDelay);
        while (true) {
            SpawnAttack();
            yield return new WaitForSeconds(GetAttackInterval());
        }
    }

    private void SpawnAttack() {
        Vector2 target = GetAttackTarget();
        Vector3 root = new Vector3(_attackRoot.position.x, target.y, _attackRoot.position.z);
        CaveBossAttackTentacle tentacle = Instantiate(_attackTentaclePrefab, root, Quaternion.identity, transform);
        _attackTentacles.RemoveAll(t => t == null);
        _attackTentacles.Add(tentacle);
        tentacle.Launch(target);
    }

    // The orb in play if there is one, otherwise the active player
    private Vector2 GetAttackTarget() {
        if (CaveBossOrb.InPlay != null) return CaveBossOrb.InPlay.transform.position;
        Transform player = PlayerManager.obj.GetPlayerTransform(PlayerManager.obj.GetActivePlayerType());
        Collider2D collider = player.GetComponent<Collider2D>();
        return collider != null ? (Vector2)collider.bounds.center : (Vector2)player.position;
    }

    private float GetAttackInterval() {
        return Random.Range(CurrentStage.minAttackInterval, CurrentStage.maxAttackInterval);
    }

    private void ActivateStage(int index) {
        if (index >= _stages.Length) return;
        foreach (var guard in _stages[index].guardTentacles) guard.Activate();
    }

    private IEnumerator Flash() {
        for (int i = 0; i < _flashCount; i++) {
            SetFlash(true);
            yield return new WaitForSeconds(_flashDuration);
            SetFlash(false);
            yield return new WaitForSeconds(_flashDuration);
        }
    }

    private void SetFlash(bool on) {
        for (int i = 0; i < _flashRenderers.Length; i++)
            _flashRenderers[i].sharedMaterial = on ? _flashMaterial : _originalMaterials[i];
    }

    private void Shake(float strength, float duration, bool loop) {
        _visual.DOKill();
        _visual.localPosition = _visualStartPosition;
        Tween shake = _visual.DOShakePosition(duration, new Vector3(strength, strength * 0.5f, 0f), _shakeVibrato, 90f, false, false)
            .OnComplete(() => _visual.localPosition = _visualStartPosition);
        if (loop) shake.SetLoops(-1);
    }

    private IEnumerator MoveToX(float x, float speed) {
        while (transform.position.x != x) {
            SetX(Mathf.MoveTowards(transform.position.x, x, speed * Time.deltaTime));
            yield return null;
        }
    }

    private void SetX(float x) {
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    // Only counts down while fighting
    private IEnumerator RumbleLoop() {
        while (true) {
            float wait = Random.Range(_rumbleMinInterval, _rumbleMaxInterval);
            for (float t = 0f; t < wait; t += _state == State.Fighting ? Time.deltaTime : 0f)
                yield return null;
            CameraShakeManager.obj.ShakeCamera(_rumbleShakeAmplitude, _rumbleShakeFrequency, _rumbleDuration);
            _rumbleInstance = SoundFXManager.obj.CreateAttachedInstance(_earthquakeStinger, gameObject, null);
            _rumbleInstance.start();
            _rumbleInstance.release();
            yield return new WaitForSeconds(_rumbleDuration);
            AudioUtils.SafeStop(ref _rumbleInstance);
        }
    }
}
