using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave44RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _bossCompleted;
    [SerializeField] private GameEventId _bossStarted;
    [SerializeField] private GameObject _lockedDoorOrb;
    [SerializeField] private GameObject _lockedDoor;
    [SerializeField] private Prisoner _prisoner1;
    [SerializeField] private Prisoner _prisoner2;
    [SerializeField] private MusicTrack _bossMusic;
    [SerializeField] private AmbienceTrack _caveMainAmbience;
    [SerializeField] private SpawnPoint _bossSpawnPoint;

    void Start()
    {
        if(GameManager.obj.HasEvent(_bossCompleted)) {
            _lockedDoorOrb.SetActive(false);
            return;
        }
    }

    public void StartFight() {
        if(GameManager.obj.HasEvent(_bossCompleted))
            return;
        else if(GameManager.obj.HasEvent(_bossStarted)) {
            _prisoner1.isStatic = false;
            _prisoner2.isStatic = false;
            _prisoner1.gameObject.SetActive(true);
            _prisoner2.gameObject.SetActive(true);
            _lockedDoor.SetActive(true);
        } else
            StartCoroutine(StartFightCoroutine());

    }

    private IEnumerator StartFightCoroutine() {
        yield return new WaitForSeconds(0.3f);
        PlayerMovement.obj.Freeze();
        _prisoner1.isStatic = true;
        _prisoner2.isStatic = true;
        _prisoner1.gameObject.SetActive(true);
        _prisoner2.gameObject.SetActive(true);
        _lockedDoor.SetActive(true);
        yield return null;

        yield return new WaitForSeconds(0.5f);
        MusicManager.obj.Play(_bossMusic);
        yield return new WaitForSeconds(2f);
        PlayerMovement.obj.UnFreeze();
        yield return new WaitForSeconds(1.5f);
        _prisoner1.isStatic = false;
        _prisoner2.isStatic = false;
        GameManager.obj.RegisterEvent(_bossStarted);
        GameManager.obj.SetCurrentSpawnPointId(_bossSpawnPoint.SpawnPointID);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void EndFight() {
        MusicManager.obj.EndCurrentTrack();
        AmbienceManager.obj.Play(_caveMainAmbience);
        GameManager.obj.RegisterEvent(_bossCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
