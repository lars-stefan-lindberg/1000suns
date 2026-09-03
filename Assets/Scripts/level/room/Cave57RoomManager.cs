using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class Cave57RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject _wallBlockBack;
    [SerializeField] private GameEventId _deeCutsceneCompleted;
    [SerializeField] private GameEventId _deeConversationCompleted;
    [SerializeField] private EventReference _sootAttack;
    [SerializeField] private EventReference _floorBreak;

    void Start() {
        var caveTimelineId = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimelineId == CaveTimelineId.Id.Dee && GameManager.obj.HasEvent(_deeCutsceneCompleted)) {
            _wallBlockBack.SetActive(true);
        }
    }

    public void OnAfterDeeCutscene() {
        var caveTimelineId = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimelineId == CaveTimelineId.Id.Dee) {
            if(!GameManager.obj.HasEvent(_deeConversationCompleted) || GameManager.obj.HasEvent(_deeCutsceneCompleted)) {
                return;
            }

            CaveAvatar.obj.gameObject.SetActive(false);
            Player.obj.gameObject.SetActive(false);

            _wallBlockBack.SetActive(true);
            GameManager.obj.RegisterEvent(_deeCutsceneCompleted);

            ShadowTwinMovement.obj.UnFreeze();
            GameManager.obj.IsPauseAllowed = true;
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

            StartCoroutine(PlayDelayedEliTrapSfx());
        }
    }

    private IEnumerator PlayDelayedEliTrapSfx() {
        yield return new WaitForSeconds(5f);
        SoundFXManager.obj.Play2D(_sootAttack);
        yield return new WaitForSeconds(0.8f);
        SoundFXManager.obj.Play2D(_floorBreak);
    }
}
