using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave62RoomManager : MonoBehaviour
{
    [SerializeField] private CaveBoss _bossPrefab;
    [Tooltip("Off screen position the boss is moved to when the player respawns in this room. A CatchUp trigger further in brings it back.")]
    [SerializeField] private Transform _bossRespawnPosition;
    [Tooltip("Speed the boss pursues with after catching up, when the player respawned in this room.")]
    [SerializeField] private CaveBoss.SpeedLevel _bossSpeed = CaveBoss.SpeedLevel.Medium;

    void Start() {
        // Also loaded as an adjacent room, only handle the boss when (re)spawning in this room
        if(gameObject.scene != SceneManager.GetActiveScene())
            return;

        CaveBoss boss = CaveBoss.Spawn(_bossPrefab, _bossRespawnPosition.position);
        boss.StopImmediately();
        boss.SetSpeed(_bossSpeed);
        boss.SetRumbleEnabled(true);
    }
}
