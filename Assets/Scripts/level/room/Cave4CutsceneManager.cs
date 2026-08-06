using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class Cave4CutsceneManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _isCutsceneCompletedEli;
    [SerializeField] private GameEventId _isCutsceneCompletedDee;
    [SerializeField] private ConversationManager _conversationManagerEli;
    [SerializeField] private ConversationManager _conversationManagerDee;
    [SerializeField] private Cave4SlabTrigger _slabTrigger;
    [SerializeField] private GameObject _cutsceneCamera;
    [SerializeField] private EventReference _stinger;
    private Coroutine _cutsceneCoroutine;
    private Coroutine _slabCoroutine;
    private EventInstance _stingerInstance;
    private EventInstance _stonesStartInstance;
    private EventInstance _stonesImpactInstance;

    void OnDestroy() {
        _conversationManagerEli.OnConversationEnd -= OnConversationCompletedEli;
        _conversationManagerDee.OnConversationEnd -= OnConversationCompletedDee;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player"))
            return;
        PlayerIdentity player = collision.gameObject.GetComponent<PlayerIdentity>();
        if(player.id == 1) {
            if(GameManager.obj.HasEvent(_isCutsceneCompletedEli)) {
                return;
            }
            _conversationManagerEli.OnConversationEnd += OnConversationCompletedEli;
            _conversationManagerEli.enabled = true;
            _cutsceneCoroutine = StartCoroutine(StartCutsceneEli());
        } else if(player.id == 2) {
            if(GameManager.obj.HasEvent(_isCutsceneCompletedDee)) {
                return;
            }
            _conversationManagerDee.OnConversationEnd += OnConversationCompletedDee;
            _conversationManagerDee.enabled = true;
            _cutsceneCoroutine = StartCoroutine(StartCutsceneDee());
        }
    }

    private IEnumerator StartCutsceneEli() {
        PauseMenuManager.obj.RegisterSkippable(this);
        PlayerMovement.obj.Freeze();
        _cutsceneCamera.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        _stingerInstance = SoundFXManager.obj.CreateAttachedInstance(_stinger, gameObject, null);
        _stingerInstance.start();
        _stingerInstance.release();
        yield return new WaitForSeconds(1.7f);

        _slabCoroutine = StartCoroutine(_slabTrigger.StartVfx());
        yield return _slabCoroutine;
        _stonesStartInstance = _slabTrigger.GetStonesStartInstance();
        _stonesImpactInstance = _slabTrigger.GetStonesImpactInstance();
        yield return new WaitForSeconds(1f);

        _conversationManagerEli.StartConversation();
        
        yield return null;
    }

    private IEnumerator StartCutsceneDee() {
        PauseMenuManager.obj.RegisterSkippable(this);
        ShadowTwinMovement.obj.Freeze();
        _cutsceneCamera.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        _stingerInstance = SoundFXManager.obj.CreateAttachedInstance(_stinger, gameObject, null);
        _stingerInstance.start();
        _stingerInstance.release();
        yield return new WaitForSeconds(1.7f);

        _slabCoroutine = StartCoroutine(_slabTrigger.StartVfx());
        yield return _slabCoroutine;
        _stonesStartInstance = _slabTrigger.GetStonesStartInstance();
        _stonesImpactInstance = _slabTrigger.GetStonesImpactInstance();
        yield return new WaitForSeconds(1f);

        _conversationManagerDee.StartConversation();
        
        yield return null;
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null) {
            StopCoroutine(_cutsceneCoroutine);
        }
        if(_slabCoroutine != null) {
            StopCoroutine(_slabCoroutine);
        }

        _cutsceneCamera.SetActive(false);
        AudioUtils.SafeStop(ref _stingerInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        AudioUtils.SafeStop(ref _stonesStartInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        AudioUtils.SafeStop(ref _stonesImpactInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);

        _slabTrigger.Reset();

        PlayerManager.PlayerType activePlayerType = PlayerManager.obj.GetActivePlayerType();
        if(activePlayerType == PlayerManager.PlayerType.HUMAN) {
            _conversationManagerEli.HardStopConversation();
            _conversationManagerEli.OnConversationEnd -= OnConversationCompletedEli;
            GameManager.obj.RegisterEvent(_isCutsceneCompletedEli);
            StartCoroutine(ResumeGameplayEli());
        } else if(activePlayerType == PlayerManager.PlayerType.SHADOW_TWIN) {
            _conversationManagerDee.HardStopConversation();
            _conversationManagerDee.OnConversationEnd -= OnConversationCompletedDee;
            GameManager.obj.RegisterEvent(_isCutsceneCompletedDee);
            StartCoroutine(ResumeGameplayDee());
        }
    }

    private IEnumerator ResumeGameplayEli() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    private IEnumerator ResumeGameplayDee() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        ShadowTwinMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    private void OnConversationCompletedEli() {
        _conversationManagerEli.CleanUp();
        _cutsceneCamera.SetActive(false);
        PlayerMovement.obj.UnFreeze();
        _conversationManagerEli.OnConversationEnd -= OnConversationCompletedEli;
        GameManager.obj.RegisterEvent(_isCutsceneCompletedEli);
        PauseMenuManager.obj.UnregisterSkippable();
    }

    private void OnConversationCompletedDee() {
        _conversationManagerDee.CleanUp();
        _cutsceneCamera.SetActive(false);
        ShadowTwinMovement.obj.UnFreeze();
        _conversationManagerDee.OnConversationEnd -= OnConversationCompletedDee;
        GameManager.obj.RegisterEvent(_isCutsceneCompletedDee);
        PauseMenuManager.obj.UnregisterSkippable();
    }
}
