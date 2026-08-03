using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class FirstPrisonerManager : MonoBehaviour
{
    [SerializeField] private GameEventId _firstPrisonerFightEndedEli;
    [SerializeField] private GameEventId _firstPrisonerFightEndedDee;
    [SerializeField] private GameObject _bossGameObjects;
    [SerializeField] private GameObject _blockingWall;
    [SerializeField] private Tilemap _blockingWallTilemap;

    void Start() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        
        if(caveTimeline == CaveTimelineId.Id.Eli && GameManager.obj.HasEvent(_firstPrisonerFightEndedEli)) {
            _bossGameObjects.SetActive(false);
        } else if(caveTimeline == CaveTimelineId.Id.Dee && GameManager.obj.HasEvent(_firstPrisonerFightEndedDee)) {
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
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli) {
            GameManager.obj.RegisterEvent(_firstPrisonerFightEndedEli);
        } else if(caveTimeline == CaveTimelineId.Id.Dee) {
            GameManager.obj.RegisterEvent(_firstPrisonerFightEndedDee);
        }
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
