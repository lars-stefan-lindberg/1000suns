using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Tilemaps;

public class Cave15RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject _eliBreakableWall;
    [SerializeField] private GameObject _eliBlockingWall;
    [SerializeField] private GameObject _deeBlockingTiles;
    [SerializeField] private GameEventId _eliBrokeWall;

    private CaveTimelineId.Id _caveTimelineId;

    void Start()
    {
        _caveTimelineId = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(_caveTimelineId == CaveTimelineId.Id.Eli) {
            if(GameManager.obj.HasEvent(_eliBrokeWall)) {
                _eliBreakableWall.SetActive(false);
                _eliBlockingWall.SetActive(false);
            }
        } else if(_caveTimelineId == CaveTimelineId.Id.Dee) {
            _deeBlockingTiles.SetActive(true);
        } else if(_caveTimelineId == CaveTimelineId.Id.Both) {
            _eliBreakableWall.SetActive(false);
            _eliBlockingWall.SetActive(false);
        }
    }

    public void OnEliBrokeWall() {
        if(_caveTimelineId == CaveTimelineId.Id.Eli) {
            _eliBlockingWall.SetActive(false);
            GameManager.obj.RegisterEvent(_eliBrokeWall);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        } else if(_caveTimelineId == CaveTimelineId.Id.Dee) {
            var tilemap = _deeBlockingTiles.GetComponentInChildren<Tilemap>();
            DOTween.To(() => tilemap.color.a, x => tilemap.color = new Color(tilemap.color.r, tilemap.color.g, tilemap.color.b, x), 0, 1);
        }
    }
}
