using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Cave45RoomManager : MonoBehaviour
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
    [SerializeField] private GameObject _blockingWall;
    [SerializeField] private Tilemap _blockingWallTilemap;

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
            StartCoroutine(ContinueFight());
        } else
            StartCoroutine(StartFightCoroutine());
    }

    private IEnumerator ContinueFight() {
        _prisoner1.isStatic = true;
        _prisoner2.isStatic = true;
        _prisoner1.gameObject.SetActive(true);
        _prisoner2.gameObject.SetActive(true);
        _lockedDoor.SetActive(true);
        _blockingWall.SetActive(true);
        _blockingWallTilemap.color = new Color(_blockingWallTilemap.color.r, _blockingWallTilemap.color.g, _blockingWallTilemap.color.b, 1f);

        yield return new WaitForSeconds(2f);

        _prisoner1.isStatic = false;
        _prisoner2.isStatic = false;
    }

    private IEnumerator StartFightCoroutine() {
        PlayerMovement.obj.Freeze();
        _prisoner1.isStatic = true;
        _prisoner2.isStatic = true;
        _prisoner1.gameObject.SetActive(true);
        _prisoner2.gameObject.SetActive(true);
        _lockedDoor.SetActive(true);

        _blockingWall.SetActive(true);
        Color startColor = _blockingWallTilemap.color;
        startColor.a = 0f;
        _blockingWallTilemap.color = startColor;
        Color targetColor = startColor;
        targetColor.a = 1f;
        DOTween.To(() => _blockingWallTilemap.color, x => _blockingWallTilemap.color = x, targetColor, 1f);

        yield return null;

        yield return new WaitForSeconds(0.5f);
        MusicManager.obj.Play(_bossMusic);
        yield return new WaitForSeconds(1.5f);
        PlayerMovement.obj.UnFreeze();
        yield return new WaitForSeconds(1f);
        _prisoner1.isStatic = false;
        _prisoner2.isStatic = false;
        GameManager.obj.RegisterEvent(_bossStarted);
        GameManager.obj.SetCurrentSpawnPointId(_bossSpawnPoint.SpawnPointID);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void EndFight() {
        Color startColor = _blockingWallTilemap.color;
        startColor.a = 1f;
        _blockingWallTilemap.color = startColor;
        Color targetColor = startColor;
        targetColor.a = 0f;
        DOTween.To(() => _blockingWallTilemap.color, x => _blockingWallTilemap.color = x, targetColor, 1f).OnComplete(() => {
            _blockingWall.SetActive(false);
        });
        
        MusicManager.obj.EndCurrentTrack();
        AmbienceManager.obj.Play(_caveMainAmbience);
        GameManager.obj.RegisterEvent(_bossCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
