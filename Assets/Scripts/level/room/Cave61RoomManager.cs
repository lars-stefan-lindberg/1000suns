using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave61RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _bossStarted;
    [SerializeField] private SpawnPoint _bossSpawnPoint;
    [SerializeField] private GameObject _boss;
    [SerializeField] private float _waitForBossTime;

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
        _boss.SetActive(true);
        yield return new WaitForSeconds(1);
        PlayerManager.obj.FlipPlayer(activePlayerType);
        yield return new WaitForSeconds(_waitForBossTime);

        //Start music

        PlayerManager.obj.UnfreezePlayer(activePlayerType);

        GameManager.obj.SetCurrentSpawnPointId(_bossSpawnPoint.SpawnPointID);
        GameManager.obj.RegisterEvent(_bossStarted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
