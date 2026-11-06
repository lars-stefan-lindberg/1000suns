using System.Collections;
using UnityEngine;

// Short lived tentacle shot out by CaveBossFight towards a fixed point:
// poke out (straight) -> telegraph (wiggle) -> straighten -> lunge (straight) -> hold (wiggle) -> retract (wiggle) -> despawn.
// Put it on the prefab root together with a CaveBossTentacleChain (Start Collapsed on, Auto Length on)
// and a trigger EdgeCollider2D + KillTrigger for the hazard.
public class CaveBossAttackTentacle : MonoBehaviour
{
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

    [Header("Hold & Retract")]
    [SerializeField] private float _holdDuration = 0.3f;
    [SerializeField, Range(0f, 1f)] private float _holdWiggle = 0.6f;
    [SerializeField] private float _retractSpeed = 10f;

    [Tooltip("Time to fade the wiggle in.")]
    [SerializeField, Min(0f)] private float _wiggleFadeDuration = 0.15f;

    private Coroutine _routine;
    private bool _isRetracting;

    public void Launch(Vector2 target) {
        _chain.SetWiggle(0f, 0f);
        _routine = StartCoroutine(Attack(target));
    }

    // Aborts the attack and pulls back from wherever the tip is
    public void Retract() {
        if (_isRetracting) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(RetractAndDespawn());
    }

    private IEnumerator Attack(Vector2 target) {
        Vector2 root = transform.position;
        Vector2 toTarget = target - root;
        Vector2 poke = root + Vector2.ClampMagnitude(toTarget, _telegraphDistance);

        yield return MoveTip(poke, _pokeSpeed);
        _chain.SetWiggle(_telegraphWiggle, _wiggleFadeDuration);
        yield return new WaitForSeconds(_telegraphDuration);
        _chain.SetWiggle(0f, _straightenDuration);
        yield return new WaitForSeconds(_straightenDuration);

        yield return MoveTip(target, _lungeSpeed);
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

    private IEnumerator MoveTip(Vector2 destination, float speed) {
        Vector2 tip = _chain.TipPosition;
        while (tip != destination) {
            tip = Vector2.MoveTowards(tip, destination, speed * Time.deltaTime);
            _chain.SetTip(tip);
            yield return null;
        }
    }
}
