using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave66RoomManager : MonoBehaviour
{
    [Serializable]
    private class StageGrounds {
        [Tooltip("Moveable grounds that move in when this boss stage starts, and move out and despawn when it ends.")]
        public MoveableGround[] grounds;
    }

    private const int FirstStageIndex = 0;
    // Used for "no stage" when only grounds of one stage should move, like before the first stage
    private const int NoStageIndex = -1;
    private const float DefaultGroundRumbleLingerDuration = 0.75f;

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
    [Tooltip("Moveable grounds per boss stage (element index = stage, which is the number of hits the boss has taken). " +
             "Inactive at room start, moved from start to end position when the stage starts, and back to start position and despawned when it ends.")]
    [SerializeField] private StageGrounds[] _stageGrounds;
    [Tooltip("Seconds the boss rumble keeps going after the moveable grounds have arrived. The fight continues right away.")]
    [SerializeField, Min(0f)] private float _groundRumbleLingerDuration = DefaultGroundRumbleLingerDuration;

    private bool _isStarted;
    private Coroutine _stopGroundRumbleRoutine;

    void Start() {
        DespawnAllStageGrounds();

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
            _boss.StageCleared += OnBossStageCleared;
            _boss.PlaceAtFightPosition();
            // Dying resets the fight to the first stage, which starts right away, so its grounds are already in place
            foreach(var ground in GetStageGrounds(FirstStageIndex))
                ground.Spawn(MoveableGround.Position.End);
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
        _boss.StageCleared += OnBossStageCleared;
        yield return _boss.Enter();
        // The first stage's grounds move in after the boss has entered, and are in place before it attacks
        yield return ChangeStageGrounds(NoStageIndex, FirstStageIndex);
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

    // Holds the boss after its hit reaction until the cleared stage's grounds have moved out and the next stage's grounds have moved in
    private void OnBossStageCleared(int clearedStageIndex) {
        _boss.HoldStageStart();
        StartCoroutine(ChangeStageGroundsThenReleaseBoss(clearedStageIndex, clearedStageIndex + 1));
    }

    private IEnumerator ChangeStageGroundsThenReleaseBoss(int outgoingStageIndex, int incomingStageIndex) {
        yield return ChangeStageGrounds(outgoingStageIndex, incomingStageIndex);
        // The boss destroys itself after the final retreat
        if(_boss != null)
            _boss.ReleaseStageStart();
    }

    // Moves the outgoing stage's grounds to their start position and despawns them, and spawns the incoming stage's grounds
    // at their start position and moves them to their end position. The boss rumbles until all grounds have arrived.
    private IEnumerator ChangeStageGrounds(int outgoingStageIndex, int incomingStageIndex) {
        MoveableGround[] outgoingGrounds = GetStageGrounds(outgoingStageIndex);
        MoveableGround[] incomingGrounds = GetStageGrounds(incomingStageIndex);
        if(outgoingGrounds.Length == 0 && incomingGrounds.Length == 0)
            yield break;

        // A lingering rumble from a previous ground change continues into this one
        if(_stopGroundRumbleRoutine != null) {
            StopCoroutine(_stopGroundRumbleRoutine);
            _stopGroundRumbleRoutine = null;
        }
        _boss.StartContinuousRumble();
        foreach(var ground in outgoingGrounds)
            ground.MoveToStart();
        foreach(var ground in incomingGrounds) {
            ground.Spawn(MoveableGround.Position.Start);
            ground.MoveToEnd();
        }

        foreach(var ground in outgoingGrounds.Concat(incomingGrounds))
            while(ground.IsMoving)
                yield return null;

        foreach(var ground in outgoingGrounds)
            ground.Despawn();
        // Runs on its own, so the fight doesn't wait for the rumble to end
        _stopGroundRumbleRoutine = StartCoroutine(StopGroundRumbleAfterLinger());
    }

    private IEnumerator StopGroundRumbleAfterLinger() {
        yield return new WaitForSeconds(_groundRumbleLingerDuration);
        _stopGroundRumbleRoutine = null;
        // The boss destroys itself after the final retreat
        if(_boss != null)
            _boss.StopContinuousRumble();
    }

    private void DespawnAllStageGrounds() {
        if(_stageGrounds == null)
            return;
        foreach(var stage in _stageGrounds)
            foreach(var ground in stage.grounds)
                if(ground != null)
                    ground.Despawn();
    }

    // Empty for stages without grounds, so stages can be left unassigned
    private MoveableGround[] GetStageGrounds(int stageIndex) {
        if(_stageGrounds == null || stageIndex < 0 || stageIndex >= _stageGrounds.Length || _stageGrounds[stageIndex].grounds == null)
            return Array.Empty<MoveableGround>();
        return _stageGrounds[stageIndex].grounds.Where(ground => ground != null).ToArray();
    }

    private void HideFightObjects() {
        foreach(var fightObject in _fightObjects)
            fightObject.SetActive(false);
    }
}
