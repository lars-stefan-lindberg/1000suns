using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class FirstPrisonerManager : MonoBehaviour
{
    [SerializeField] private GameEventId _firstPrisonerFightEnded;
    [SerializeField] private GameObject _bossGameObjects;
    [SerializeField] private GameObject _blockingWall;
    [SerializeField] private Tilemap _blockingWallTilemap;

    void Start() {
        if(GameManager.obj.HasEvent(_firstPrisonerFightEnded)) {
            _bossGameObjects.SetActive(false);
        }
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
        GameManager.obj.RegisterEvent(_firstPrisonerFightEnded);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
