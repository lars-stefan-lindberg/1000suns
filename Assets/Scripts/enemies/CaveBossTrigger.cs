using UnityEngine;

public class CaveBossTrigger : MonoBehaviour
{
    public enum TriggerAction {
        SetSpeed,
        CatchUp,
        Stop
    }

    [Tooltip("Leave empty to use the spawned (persistent) boss.")]
    [SerializeField] private CaveBoss _caveBoss;
    [SerializeField] private TriggerAction _action = TriggerAction.SetSpeed;
    [Tooltip("Only used when Action is SetSpeed.")]
    [SerializeField] private CaveBoss.SpeedLevel _speed = CaveBoss.SpeedLevel.Medium;
    [SerializeField] private bool _triggerOnce = true;
    private bool _hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggerOnce && _hasTriggered) return;
        if (!other.gameObject.CompareTag("Player")) return;

        CaveBoss caveBoss = _caveBoss != null ? _caveBoss : CaveBoss.obj;
        if (caveBoss == null) return;

        if (_action == TriggerAction.SetSpeed) {
            caveBoss.SetSpeed(_speed);
        } else if (_action == TriggerAction.CatchUp) {
            caveBoss.CatchUp();
        } else {
            caveBoss.StopImmediately();
        }
        _hasTriggered = true;
    }
}
