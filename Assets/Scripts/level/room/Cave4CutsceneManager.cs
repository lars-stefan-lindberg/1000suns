using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave4CutsceneManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _isCutsceneCompleted;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private Cave4SlabTrigger _slabTrigger;
    private Coroutine _cutsceneCoroutine;

    void Start() {
        _conversationManager.OnConversationEnd += OnConversationCompleted;
    }

    void OnDestroy() {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(GameManager.obj.HasEvent(_isCutsceneCompleted))
            return;
        if(!collision.CompareTag("Player"))
            return;
        _cutsceneCoroutine = StartCoroutine(StartCutscene());
    }

    private IEnumerator StartCutscene() {
        PauseMenuManager.obj.RegisterSkippable(this);
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(_slabTrigger.StartVfx());
        yield return new WaitForSeconds(1f);

        _conversationManager.StartConversation();
        
        yield return null;
    }

    public void RequestSkip() {
        StopCoroutine(_cutsceneCoroutine);
        _conversationManager.HardStopConversation();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;

        //Reset slab and stones
        _slabTrigger.Reset();
        GameManager.obj.RegisterEvent(_isCutsceneCompleted);
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

    private void OnConversationCompleted() {
        PlayerMovement.obj.UnFreeze();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        GameManager.obj.RegisterEvent(_isCutsceneCompleted);
    }
}
