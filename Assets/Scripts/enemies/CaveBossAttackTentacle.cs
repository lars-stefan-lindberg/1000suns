using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Short lived tentacle shot out by CaveBossFight towards a fixed point:
// poke out (straight) -> telegraph (wiggle) -> straighten -> lunge (straight) -> hold (wiggle) -> retract (wiggle) -> despawn.
// The lunge overshoots the target point by Lunge Overshoot, but stops early when it hits the player or the orb in play.
// Put it on the prefab root together with a CaveBossTentacleChain (Start Collapsed on, Auto Length on)
// and a trigger EdgeCollider2D + KillTrigger for the hazard.
public class CaveBossAttackTentacle : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [SerializeField] private CaveBossTentacleChain _chain;

    [Header("Telegraph")]
    [Tooltip("How far the tip pokes out towards the target before the lunge.")]
    [SerializeField] private float _telegraphDistance = 1f;
    [SerializeField] private float _pokeSpeed = 4f;
    [SerializeField] private float _telegraphDuration = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _telegraphWiggle = 0.4f;
    [Tooltip("Time to fade the wiggle out before the lunge.")]
    [SerializeField, Min(0f)] private float _straightenDuration = 0.1f;

    [Header("Lunge")]
    [SerializeField] private float _lungeSpeed = 25f;
    [Tooltip("How far (world units) the lunge continues horizontally past the target's position at launch. " +
             "The lunge stops earlier if it hits the player or an orb.")]
    [SerializeField, Min(0f)] private float _lungeOvershoot = 2f;

    [Header("Hold & Retract")]
    [SerializeField] private float _holdDuration = 0.3f;
    [SerializeField, Range(0f, 1f)] private float _holdWiggle = 0.6f;
    [SerializeField] private float _retractSpeed = 10f;

    [Tooltip("Time to fade the wiggle in.")]
    [SerializeField, Min(0f)] private float _wiggleFadeDuration = 0.15f;

    private readonly List<Collider2D> _hazardOverlaps = new List<Collider2D>();
    // Accepts every layer, depth and trigger so the player is found regardless of its collider setup
    private readonly ContactFilter2D _unfilteredContacts = new ContactFilter2D().NoFilter();
    private Coroutine _routine;
    private bool _isRetracting;
    private bool _hasHitOrb;

    void OnEnable() {
        CaveBossOrb.HitByTentacle += OnOrbHitByTentacle;
    }

    void OnDisable() {
        CaveBossOrb.HitByTentacle -= OnOrbHitByTentacle;
    }

    /// <summary>
    /// Starts the attack towards the given point. The lunge keeps the target's height and, if overshoot is on,
    /// continues Lunge Overshoot past it horizontally. It stops early if the player or an orb is hit first.
    /// </summary>
    public void Launch(Vector2 target, bool overshoot = true) {
        _chain.SetWiggle(0f, 0f);
        _routine = StartCoroutine(Attack(target, overshoot ? _lungeOvershoot : 0f));
    }

    // Aborts the attack and pulls back from wherever the tip is
    public void Retract() {
        if (_isRetracting) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(RetractAndDespawn());
    }

    private IEnumerator Attack(Vector2 target, float overshootDistance) {
        Vector2 root = transform.position;
        Vector2 toTarget = target - root;
        Vector2 poke = root + Vector2.ClampMagnitude(toTarget, _telegraphDistance);
        float lungeDirection = Mathf.Sign(toTarget.x);
        Vector2 lungeEnd = new Vector2(target.x + lungeDirection * overshootDistance, target.y);

        yield return MoveTip(poke, _pokeSpeed);
        _chain.SetWiggle(_telegraphWiggle, _wiggleFadeDuration);
        yield return new WaitForSeconds(_telegraphDuration);
        _chain.SetWiggle(0f, _straightenDuration);
        yield return new WaitForSeconds(_straightenDuration);

        yield return Lunge(lungeEnd);
        _chain.SetWiggle(_holdWiggle, _wiggleFadeDuration);
        yield return new WaitForSeconds(_holdDuration);

        yield return RetractAndDespawn();
    }

    private IEnumerator RetractAndDespawn() {
        _isRetracting = true;
        _chain.SetWiggle(_holdWiggle, _wiggleFadeDuration);
        yield return MoveTip(transform.position, _retractSpeed);
        Destroy(gameObject);
    }

    // Moves the tip to the lunge end, stopping where it is as soon as the player or an orb is hit
    private IEnumerator Lunge(Vector2 lungeEnd) {
        Vector2 tip = _chain.TipPosition;
        while (tip != lungeEnd && !HasHitTarget()) {
            tip = Vector2.MoveTowards(tip, lungeEnd, _lungeSpeed * Time.deltaTime);
            _chain.SetTip(tip);
            yield return null;
        }
    }

    private IEnumerator MoveTip(Vector2 destination, float speed) {
        Vector2 tip = _chain.TipPosition;
        while (tip != destination) {
            tip = Vector2.MoveTowards(tip, destination, speed * Time.deltaTime);
            _chain.SetTip(tip);
            yield return null;
        }
    }

    // The orb disables its colliders the moment it's hit, so orb hits are reported by CaveBossOrb.HitByTentacle
    // instead of being found through overlaps. The player is found by overlapping the hazard collider.
    private bool HasHitTarget() {
        if (_hasHitOrb) return true;

        EdgeCollider2D hazard = _chain.hazardCollider;
        if (hazard == null || !hazard.enabled) return false;

        hazard.OverlapCollider(_unfilteredContacts, _hazardOverlaps);
        foreach (Collider2D overlap in _hazardOverlaps)
            if (overlap.CompareTag(PlayerTag)) return true;
        return false;
    }

    private void OnOrbHitByTentacle(Collider2D hazard) {
        if (hazard == _chain.hazardCollider) _hasHitOrb = true;
    }
}
