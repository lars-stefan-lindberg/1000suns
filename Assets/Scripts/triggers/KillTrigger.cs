using UnityEngine;
using UnityEngine.Events;

public class KillTrigger : MonoBehaviour
{
    [SerializeField] private bool _onlyKillPlayer = false;
    public UnityEvent _onKill;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            Reaper.obj.KillPlayerGeneric(PlayerManager.obj.GetPlayerTypeFromCollider(other));
            _onKill?.Invoke();
        }
        if(other.gameObject.CompareTag("Enemy") && !_onlyKillPlayer) {
            Reaper.obj.KillPrisoner(other.gameObject.GetComponent<Prisoner>());
            _onKill?.Invoke();
        }
    }
}
