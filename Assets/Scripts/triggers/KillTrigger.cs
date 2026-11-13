using UnityEngine;
using UnityEngine.Events;

public class KillTrigger : MonoBehaviour
{
    [SerializeField] private bool _onlyKillPlayer = false;
    public UnityEvent _onKill;

    /// <summary>
    /// When false the player is ignored by this trigger (e.g. for debugging). Defaults to true.
    /// </summary>
    public bool CanKillPlayer { get; set; } = true;

    void OnTriggerEnter2D(Collider2D other) {
        if(CanKillPlayer && other.gameObject.CompareTag("Player")) {
            Reaper.obj.KillPlayerGeneric(PlayerManager.obj.GetPlayerTypeFromCollider(other));
            _onKill?.Invoke();
        }
        if(other.gameObject.CompareTag("Enemy") && !_onlyKillPlayer) {
            Reaper.obj.KillPrisoner(other.gameObject.GetComponent<Prisoner>());
            _onKill?.Invoke();
        }
    }
}
