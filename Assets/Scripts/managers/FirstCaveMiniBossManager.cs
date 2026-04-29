using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstCaveMiniBossManager : MonoBehaviour
{
    
    [SerializeField] private SpawnPoint _spawnPoint;
    [SerializeField] private MusicTrack _bossTrack;
    [SerializeField] private GameEventId _bossCompleted;
    [SerializeField] private GameObject _bossGameObjects;
    private bool _isBossFightStarted = false;

    void Start() {
        if(!GameManager.obj.HasEvent(_bossCompleted))
            _bossGameObjects.SetActive(true);
    }

    public void EndBossFight() {
        MusicManager.obj.EndCurrentTrack();
        GameManager.obj.RegisterEvent(_bossCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void StartBossFight() {
        if(!GameManager.obj.HasEvent(_bossCompleted)) {
            if(!_isBossFightStarted) {
                GameManager.obj.SetCurrentSpawnPointId(_spawnPoint.SpawnPointID);
                MusicManager.obj.Play(_bossTrack);
                SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
                _isBossFightStarted = true;
            } 
        }
    }
}
