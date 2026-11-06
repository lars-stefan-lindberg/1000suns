using UnityEngine;

public class CaveBossTrigger : MonoBehaviour
{
    public enum TriggerAction {
        SetSpeed,
        CatchUp,
        Stop,
        Despawn,
        Spawn
    }

    [Tooltip("Leave empty to use the spawned (persistent) boss.")]
    [SerializeField] private CaveBoss _caveBoss;
    [SerializeField] private TriggerAction _action = TriggerAction.SetSpeed;
    [Tooltip("Only used when Action is SetSpeed.")]
    [SerializeField] private CaveBoss.SpeedLevel _speed = CaveBoss.SpeedLevel.Medium;
    [Tooltip("Only used when Action is Spawn.")]
    [SerializeField] private CaveBoss _caveBossPrefab;
    [Tooltip("Only used when Action is Spawn. If the boss already exists, it's teleported here.")]
    [SerializeField] private Transform _spawnPosition;
    [SerializeField] private bool _triggerOnce = true;
    private bool _hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggerOnce && _hasTriggered) return;
        if (!other.gameObject.CompareTag("Player")) return;

        if (_action == TriggerAction.Spawn) {
            CaveBoss.Spawn(_caveBossPrefab, _spawnPosition.position).StopImmediately();
            _hasTriggered = true;
            return;
        }

        CaveBoss caveBoss = _caveBoss != null ? _caveBoss : CaveBoss.obj;
        if (caveBoss == null) return;

        if (_action == TriggerAction.SetSpeed) {
            caveBoss.SetSpeed(_speed);
        } else if (_action == TriggerAction.CatchUp) {
            caveBoss.CatchUp();
        } else if (_action == TriggerAction.Stop) {
            caveBoss.StopImmediately();
        } else if (caveBoss == CaveBoss.obj) {
            CaveBoss.Despawn();
        } else {
            Destroy(caveBoss.gameObject);
        }
        _hasTriggered = true;
    }
}
