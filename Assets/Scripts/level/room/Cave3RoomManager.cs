using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave3RoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _cutsceneCompleted;
    [SerializeField] private ConversationManager _deeConversationManager;

    private Coroutine _cutsceneCoroutine;

    public void StartCutscene()
    {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Dee) {
            if(GameManager.obj.HasEvent(_cutsceneCompleted)) {
                return;
            }
            _deeConversationManager.OnConversationEnd += OnConversationCompleted;
            _deeConversationManager.enabled = true;
            ShadowTwinMovement.obj.Freeze();
            _cutsceneCoroutine = StartCoroutine(Cutscene());
        }
    }

    private IEnumerator Cutscene() {
        PauseMenuManager.obj.RegisterSkippable(this);

        yield return new WaitForSeconds(1f);
        _deeConversationManager.StartConversation();
        yield return null;
    }

    private void OnConversationCompleted() {
        _deeConversationManager.OnConversationEnd -= OnConversationCompleted;
        _deeConversationManager.CleanUp();
        _deeConversationManager.enabled = false;
        PauseMenuManager.obj.UnregisterSkippable();
        CaveAvatar.obj.IsFollowingPlayer = false;
        GameManager.obj.RegisterEvent(_cutsceneCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        ShadowTwinMovement.obj.UnFreeze();
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null) {
            StopCoroutine(_cutsceneCoroutine);
        }

        _deeConversationManager.OnConversationEnd -= OnConversationCompleted;
        _deeConversationManager.HardStopConversation();
        _deeConversationManager.CleanUp();

        CaveAvatar.obj.IsFollowingPlayer = false;
        GameManager.obj.RegisterEvent(_cutsceneCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        ShadowTwinMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    void OnDestroy() {
        if(_deeConversationManager != null) {
            _deeConversationManager.OnConversationEnd -= OnConversationCompleted;
        }
    }
}
