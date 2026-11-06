using System.Collections;
using UnityEngine;

// Scene placed tentacle that protects the boss during a fight stage.
// Grows out straight to its target, then idles with a wiggle. Retracts (straight) on the final hit.
// Put it on an object with a CaveBossTentacleChain (Start Collapsed on, Auto Length on)
// and a trigger EdgeCollider2D + KillTrigger for the hazard.
public class CaveBossGuardTentacle : MonoBehaviour
{
    [SerializeField] private CaveBossTentacleChain _chain;
    [SerializeField] private Transform _target;
    [SerializeField] private float _growSpeed = 6f;
    [SerializeField] private float _retractSpeed = 8f;
    [SerializeField, Range(0f, 1f)] private float _idleWiggle = 1f;
    [SerializeField, Min(0f)] private float _wiggleFadeDuration = 0.3f;

    private Coroutine _routine;
    private bool _isActive;

    public void Activate() {
        if (_isActive) return;
        _isActive = true;
        Restart(Grow());
    }

    public void Retract() {
        if (!_isActive) return;
        _isActive = false;
        Restart(Shrink());
    }

    private void Restart(IEnumerator routine) {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(routine);
    }

    private IEnumerator Grow() {
        _chain.SetWiggle(0f, 0f);
        yield return MoveTip(_target.position, _growSpeed);
        _chain.SetWiggle(_idleWiggle, _wiggleFadeDuration);
    }

    private IEnumerator Shrink() {
        _chain.SetWiggle(0f, _wiggleFadeDuration);
        yield return new WaitForSeconds(_wiggleFadeDuration);
        yield return MoveTip(transform.position, _retractSpeed);
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
