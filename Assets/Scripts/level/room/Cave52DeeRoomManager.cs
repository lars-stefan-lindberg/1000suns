using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave52DeeRoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _cutsceneCompleted;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private Transform _sootStartPosition;
    [SerializeField] private Transform _sootFlyTarget1;
    [SerializeField] private Transform _sootFlyTarget2;
    private Coroutine _cutsceneCoroutine;

    public void OnCutsceneTriggered() {
        if(GameManager.obj.HasEvent(_cutsceneCompleted))
            return;
        
        ShadowTwinMovement.obj.Freeze();
        _cutsceneCoroutine = StartCoroutine(CutsceneCoroutine());
    }

    private IEnumerator CutsceneCoroutine() {
        PauseMenuManager.obj.RegisterSkippable(this);
        _conversationManager.OnConversationEnd += OnConversationCompleted;

        //Setup Soot and have him fly in
        CaveAvatar.obj.gameObject.SetActive(true);
        CaveAvatar.obj.SetRedEyes();
        CaveAvatar.obj.SetPosition(_sootStartPosition.position);
        yield return new WaitForSeconds(1f);
        CaveAvatar.obj.SetTarget(_sootFlyTarget1);
        yield return new WaitForSeconds(1.5f);
        CaveAvatar.obj.SetTarget(_sootFlyTarget2);
        yield return new WaitForSeconds(1f);
        if(!ShadowTwinMovement.obj.IsFacingLeft()) {
            ShadowTwinMovement.obj.FlipPlayer();
        }

        yield return new WaitForSeconds(1f);

        _conversationManager.StartConversation();

        yield return null;
    }

    private void OnConversationCompleted() {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        _conversationManager.CleanUp();

        PauseMenuManager.obj.UnregisterSkippable();

        StartCoroutine(FlyBackSoot());

        GameManager.obj.RegisterEvent(_cutsceneCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        ShadowTwinMovement.obj.UnFreeze();
    }

    private IEnumerator FlyBackSoot() {
        yield return new WaitForSeconds(0.5f);

        CaveAvatar.obj.SetTarget(_sootFlyTarget1);
        yield return new WaitForSeconds(1f);
        CaveAvatar.obj.SetTarget(_sootStartPosition);
        yield return new WaitForSeconds(1f);
        CaveAvatar.obj.gameObject.SetActive(false);

        yield return null;
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null)
            StopCoroutine(_cutsceneCoroutine);

        _conversationManager.HardStopConversation();
        _conversationManager.CleanUp();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;

        CaveAvatar.obj.gameObject.SetActive(false);

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
}
