using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave16RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject _wallBlockBack;
    [SerializeField] private GameEventId _deeCutsceneCompleted;

    void Start() {
        var caveTimelineId = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimelineId == CaveTimelineId.Id.Dee && GameManager.obj.HasEvent(_deeCutsceneCompleted)) {
            _wallBlockBack.SetActive(true);
        }
    }

    public void OnAfterDeeCutscene() {
        var caveTimelineId = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimelineId == CaveTimelineId.Id.Dee) {
            CaveAvatar.obj.IsFollowingPlayer = false;
            CaveAvatar.obj.OverriddenPlayerType = PlayerManager.PlayerType.NONE;
            CaveAvatar.obj.gameObject.SetActive(false);
            Player.obj.gameObject.SetActive(false);

            _wallBlockBack.SetActive(true);
            GameManager.obj.RegisterEvent(_deeCutsceneCompleted);

            ShadowTwinMovement.obj.UnFreeze();
            GameManager.obj.IsPauseAllowed = true;
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        }
    }
}
