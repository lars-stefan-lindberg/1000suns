using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstCaveMiniBossManager : MonoBehaviour
{
    
    [SerializeField] private SpawnPoint _spawnPoint;
    [SerializeField] private MusicTrack _bossTrack;
    [SerializeField] private AmbienceTrack _caveMain;
    [SerializeField] private GameEventId _bossCompleted;
    [SerializeField] private GameEventId _bossCompletedDee;
    [SerializeField] private GameEventId _bossFightStarted;
    [SerializeField] private GameEventId _bossFightStartedDee;
    [SerializeField] private GameObject _bossGameObjects;
    [SerializeField] private Prisoner[] _prisoners;
    [SerializeField] private Spike[] _fallingSpikes;

    void Start() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli && !GameManager.obj.HasEvent(_bossCompleted))
            _bossGameObjects.SetActive(true);
        else if(caveTimeline == CaveTimelineId.Id.Dee && !GameManager.obj.HasEvent(_bossCompletedDee))
            _bossGameObjects.SetActive(true);
    }

    public void EndBossFight() {
        MusicManager.obj.EndCurrentTrack();
        AmbienceManager.obj.Play(_caveMain);
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli) {
            GameManager.obj.RegisterEvent(_bossCompleted);
        } else if(caveTimeline == CaveTimelineId.Id.Dee) {
            GameManager.obj.RegisterEvent(_bossCompletedDee);
        }
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void StartBossFight() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Dee) {
            if(!GameManager.obj.HasEvent(_bossCompletedDee)) {
                foreach(Spike spike in _fallingSpikes) {
                    spike.EnablePlayerDetection();
                }
                if(!GameManager.obj.HasEvent(_bossFightStartedDee)) {
                    foreach(Prisoner prisoner in _prisoners) {
                        prisoner.isStatic = true;
                        prisoner.gameObject.SetActive(true);
                    }
                    StartCoroutine(DelayedStart());
                } else {
                    foreach(Prisoner prisoner in _prisoners)
                        prisoner.gameObject.SetActive(true);
                }
            }
            return;
        } else if(caveTimeline == CaveTimelineId.Id.Eli) {
            if(!GameManager.obj.HasEvent(_bossCompleted)) {
                foreach(Spike spike in _fallingSpikes) {
                    spike.EnablePlayerDetection();
                }
                if(!GameManager.obj.HasEvent(_bossFightStarted)) {
                    foreach(Prisoner prisoner in _prisoners) {
                        prisoner.isStatic = true;
                        prisoner.gameObject.SetActive(true);
                    }
                    StartCoroutine(DelayedStart());
                } else {
                    foreach(Prisoner prisoner in _prisoners)
                        prisoner.gameObject.SetActive(true);
                }
            }
        }
    }

    private IEnumerator DelayedStart() {
        yield return new WaitForSeconds(1.7f);
        foreach(Prisoner prisoner in _prisoners)
            prisoner.isStatic = false;
        MusicManager.obj.Play(_bossTrack);
        GameManager.obj.SetCurrentSpawnPointId(_spawnPoint.SpawnPointID);
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli) {
            GameManager.obj.RegisterEvent(_bossFightStarted);
        } else if(caveTimeline == CaveTimelineId.Id.Dee) {
            GameManager.obj.RegisterEvent(_bossFightStartedDee);
        }
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
