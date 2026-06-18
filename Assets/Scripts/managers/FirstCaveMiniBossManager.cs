using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstCaveMiniBossManager : MonoBehaviour
{
    
    [SerializeField] private SpawnPoint _spawnPoint;
    [SerializeField] private MusicTrack _bossTrack;
    [SerializeField] private AmbienceTrack _caveMain;
    [SerializeField] private GameEventId _bossCompleted;
    [SerializeField] private GameEventId _bossFightStarted;
    [SerializeField] private GameObject _bossGameObjects;
    [SerializeField] private Prisoner[] _prisoners;
    [SerializeField] private Spike[] _fallingSpikes;

    void Start() {
        if(!GameManager.obj.HasEvent(_bossCompleted))
            _bossGameObjects.SetActive(true);
    }

    public void EndBossFight() {
        MusicManager.obj.EndCurrentTrack();
        AmbienceManager.obj.Play(_caveMain);
        GameManager.obj.RegisterEvent(_bossCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void StartBossFight() {
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

    private IEnumerator DelayedStart() {
        yield return new WaitForSeconds(1.7f);
        foreach(Prisoner prisoner in _prisoners)
            prisoner.isStatic = false;
        MusicManager.obj.Play(_bossTrack);
        GameManager.obj.SetCurrentSpawnPointId(_spawnPoint.SpawnPointID);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        GameManager.obj.RegisterEvent(_bossFightStarted);
    }
}
