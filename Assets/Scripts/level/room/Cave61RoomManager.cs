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
    [SerializeField] private GameObject _boss;
    [SerializeField] private CaveBoss _bossComponent;
    [SerializeField] private float _waitForBossTime;
    [SerializeField] private EventReference _earthquakeStinger;
    private EventInstance _rumbleInstance;

    void Start() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(GameManager.obj.HasEvent(_bossStarted) && caveTimeline == CaveTimelineId.Id.Dee) {
            //Handle boss loading
        } else if(caveTimeline == CaveTimelineId.Id.Both) {
            //Handle both loading, use temporary game event
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

    private IEnumerator Cutscene(PlayerManager.PlayerType activePlayerType) {
        _bossComponent.SetRumbleEnabled(false);
        _boss.SetActive(true);
        yield return new WaitForSeconds(1);
        PlayerManager.obj.FlipPlayer(activePlayerType);
        yield return new WaitForSeconds(_waitForBossTime);
        _bossComponent.StopMoving();
        yield return new WaitForSeconds(3f);

        //Stop boss, play "roar", shake camera
        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 3);
        _rumbleInstance = SoundFXManager.obj.CreateAttachedInstance(_earthquakeStinger, gameObject, null);
        _rumbleInstance.start();
        _rumbleInstance.release();
        yield return new WaitForSeconds(3);
        AudioUtils.SafeStop(ref _rumbleInstance);

        yield return new WaitForSeconds(1f);
        //Start music
        _bossComponent.SetSpeed(CaveBoss.SpeedLevel.Medium);
        _bossComponent.SetRumbleEnabled(true);
        _bossComponent.StartMoving();

        PlayerManager.obj.UnfreezePlayer(activePlayerType);

        GameManager.obj.SetCurrentSpawnPointId(_bossSpawnPoint.SpawnPointID);
        GameManager.obj.RegisterEvent(_bossStarted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
