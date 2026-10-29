using UnityEngine;

public class CaveBossTrigger : MonoBehaviour
{
    public enum TriggerAction {
        SetSpeed,
        CatchUp
    }

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

        if (_action == TriggerAction.SetSpeed) {
            _caveBoss.SetSpeed(_speed);
        } else {
            _caveBoss.CatchUp();
        }
        _hasTriggered = true;
    }
}
