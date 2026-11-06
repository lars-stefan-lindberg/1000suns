using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave64RoomManager : MonoBehaviour
{
    [SerializeField] private CaveBoss _bossPrefab;
    [Tooltip("Position the boss is moved to (stopped) when the player respawns in this room. Should match the Spawn trigger's spawn position.")]
    [SerializeField] private Transform _bossRespawnPosition;

    void Start() {
        // Also loaded as an adjacent room, only handle the boss when (re)spawning in this room
        if(gameObject.scene != SceneManager.GetActiveScene())
            return;

        CaveBoss boss = CaveBoss.Spawn(_bossPrefab, _bossRespawnPosition.position);
        boss.StopImmediately();
        boss.SetRumbleEnabled(true);
    }
}
