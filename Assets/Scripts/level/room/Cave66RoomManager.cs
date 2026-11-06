using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave66RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _bossFightStarted;
    [SerializeField] private GameEventId _bossDefeated;
    [SerializeField] private SpawnPoint _bossSpawnPoint;
    [Tooltip("Scene placed boss, positioned off screen to the left. Keep it disabled in the scene, it's enabled when the fight starts.")]
    [SerializeField] private CaveBossFight _boss;
    [Tooltip("Room camera (with RoomCameraController) used when the fight starts, and when respawning mid fight.")]
    [SerializeField] private GameObject _fightCamera;
    [Tooltip("Used for the default main camera handling when the fight isn't active.")]
    [SerializeField] private RoomMgr _roomMgr;
    [Tooltip("Hidden once the boss is defeated.")]
    [SerializeField] private GameObject[] _fightObjects;
    [Tooltip("Seconds before the first attack after the boss has entered.")]
    [SerializeField] private float _firstAttackDelay = 1f;
    [Tooltip("Seconds before the first attack when respawning mid fight.")]
    [SerializeField] private float _resumeDelay = 2f;

    private bool _isStarted;

    void Start() {
        // Also loaded as an adjacent room, only handle the boss when (re)spawning in this room
        if(gameObject.scene != SceneManager.GetActiveScene())
            return;

        // The boss is disabled until the fight starts, so make sure the orbs can't be grabbed before that
        CaveBossOrb.SetGrabbingEnabled(false);

        if(GameManager.obj.HasEvent(_bossDefeated)) {
            _boss.gameObject.SetActive(false);
            HideFightObjects();
        } else if(GameManager.obj.HasEvent(_bossFightStarted)) {
            _isStarted = true;
            _boss.gameObject.SetActive(true);
            _boss.Defeated += EndFight;
            _boss.PlaceAtFightPosition();
            _boss.StartFighting(_resumeDelay);
        }
    }

    // Called from a trigger when the player enters the room the first time
    public void StartFight() {
        if(_isStarted || GameManager.obj.HasEvent(_bossDefeated))
            return;
        _isStarted = true;
        StartCoroutine(Intro());
    }

    private IEnumerator Intro() {
        var activePlayerType = PlayerManager.obj.GetActivePlayerType();
        PlayerManager.obj.FreezePlayer(activePlayerType);
        ActivateFightCamera();
        _boss.gameObject.SetActive(true);
        _boss.Defeated += EndFight;
        yield return _boss.Enter();
        PlayerManager.obj.UnfreezePlayer(activePlayerType);
        _boss.StartFighting(_firstAttackDelay);

        GameManager.obj.SetCurrentSpawnPointId(_bossSpawnPoint.SpawnPointID);
        GameManager.obj.RegisterEvent(_bossFightStarted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    // Hooked up to RoomMgr's Custom Camera Handling event (fired when the player enters/respawns in the room).
    // Uses the fight camera while the fight is active, otherwise RoomMgr's default main camera handling.
    public void HandleCamera() {
        bool isFightActive = GameManager.obj.HasEvent(_bossFightStarted) && !GameManager.obj.HasEvent(_bossDefeated);
        if(isFightActive)
            ActivateFightCamera();
        else
            _roomMgr.ActivateMainCameraIfNeeded();
    }

    private void ActivateFightCamera() {
        Transform player = PlayerManager.obj.GetPlayerTransform(PlayerManager.obj.GetActivePlayerType());
        GameObject room = gameObject.scene.GetRootGameObjects().First(rootObject => rootObject.CompareTag("Room"));
        CameraManager.obj.EnterRoom(_fightCamera.GetComponent<RoomCameraController>(), room.GetComponent<Collider2D>(), player, player.position);
    }

    // The boss despawns itself after the retreat
    private void EndFight() {
        HideFightObjects();
        GameManager.obj.RegisterEvent(_bossDefeated);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    private void HideFightObjects() {
        foreach(var fightObject in _fightObjects)
            fightObject.SetActive(false);
    }
}
