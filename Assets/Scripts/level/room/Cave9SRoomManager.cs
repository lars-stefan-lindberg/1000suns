using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave9SRoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private GameEventId _conversationCompleted;
    [SerializeField] private Transform _sootFlyOffTarget;

    private CaveTimelineId.Id _caveTimelineId;

    void Start()
    {
        _caveTimelineId = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        _conversationManager.OnConversationEnd += OnConversationCompleted;
    }

    private void OnDestroy()
    {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }

    public void RequestSkip() {
        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    public void StartConversation() {
        if(GameManager.obj.HasEvent(_conversationCompleted) || _caveTimelineId != CaveTimelineId.Id.Eli)
            return;
        //PauseMenuManager.obj.RegisterSkippable(this);
        PlayerMovement.obj.Freeze();
        StartCoroutine(DelayedStartConversation());
    }

    private IEnumerator DelayedStartConversation() {
        if(CaveAvatar.obj.IsFollowingPlayer) {
            CaveAvatar.obj.SetTarget(_sootFlyOffTarget);
            yield return new WaitForSeconds(2f);
        } else {
            yield return new WaitForSeconds(1f);
        }
        _conversationManager.StartConversation();
    }

    private void OnConversationCompleted() {
        CaveAvatar.obj.IsFollowingPlayer = true;
        PlayerMovement.obj.UnFreeze();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        GameManager.obj.RegisterEvent(_conversationCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        //PauseMenuManager.obj.UnregisterSkippable();
    }
}
