using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMOD.Studio;
using FMODUnity;

public class Cave61RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _bossStarted;
    [SerializeField] private SpawnPoint _bossSpawnPoint;
    [SerializeField] private CaveBoss _bossPrefab;
    [Tooltip("Where the boss is spawned when the intro starts (off screen).")]
    [SerializeField] private Transform _bossIntroStartPosition;
    [Tooltip("Where the boss is placed when the player respawns in this room after the intro.")]
    [SerializeField] private Transform _bossChaseStartPosition;
    [Tooltip("Seconds after respawning before the boss resumes the pursuit.")]
    [SerializeField] private float _resumeChaseDelay = 1f;
    [SerializeField] private float _waitForBossTime;
    [SerializeField] private EventReference _earthquakeStinger;
    private EventInstance _rumbleInstance;

    void Start() {
        // Also loaded as an adjacent room, only handle the boss when (re)spawning in this room
        if(gameObject.scene != SceneManager.GetActiveScene())
            return;

        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(GameManager.obj.HasEvent(_bossStarted) && caveTimeline == CaveTimelineId.Id.Dee) {
            StartCoroutine(ResumeChase());
        } else if(caveTimeline == CaveTimelineId.Id.Both) {
            //Handle both loading, use temporary game event
        } else {
            //Intro not completed, make sure it replays from the start
            CaveBoss.Despawn();
        }
    }

    public void StartBoss() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(GameManager.obj.HasEvent(_bossStarted) && caveTimeline == CaveTimelineId.Id.Dee)
            return;

        var activePlayerType = PlayerManager.obj.GetActivePlayerType();
        PlayerManager.obj.FreezePlayer(activePlayerType);
        StartCoroutine(Cutscene(activePlayerType));
    }

    private IEnumerator ResumeChase() {
        CaveBoss boss = CaveBoss.Spawn(_bossPrefab, _bossChaseStartPosition.position);
        boss.StopImmediately();
        boss.SetSpeed(CaveBoss.SpeedLevel.Medium);
        boss.SetRumbleEnabled(true);
        yield return new WaitForSeconds(_resumeChaseDelay);
        boss.StartMoving();
    }

    private IEnumerator Cutscene(PlayerManager.PlayerType activePlayerType) {
        CaveBoss boss = CaveBoss.Spawn(_bossPrefab, _bossIntroStartPosition.position);
        boss.SetRumbleEnabled(false);
        yield return new WaitForSeconds(1);
        PlayerManager.obj.FlipPlayer(activePlayerType);
        yield return new WaitForSeconds(_waitForBossTime);
        boss.StopMoving();
        yield return new WaitForSeconds(3f);

        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 3);
        _rumbleInstance = SoundFXManager.obj.CreateAttachedInstance(_earthquakeStinger, gameObject, null);
        _rumbleInstance.start();
        _rumbleInstance.release();
        yield return new WaitForSeconds(3);
        AudioUtils.SafeStop(ref _rumbleInstance);

        yield return new WaitForSeconds(1f);
        //Start music
        boss.SetSpeed(CaveBoss.SpeedLevel.Medium);
        boss.SetRumbleEnabled(true);
        boss.StartMoving();

        PlayerManager.obj.UnfreezePlayer(activePlayerType);

        GameManager.obj.SetCurrentSpawnPointId(_bossSpawnPoint.SpawnPointID);
        GameManager.obj.RegisterEvent(_bossStarted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
