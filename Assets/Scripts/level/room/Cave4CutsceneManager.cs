using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class Cave4CutsceneManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _isCutsceneCompleted;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private Cave4SlabTrigger _slabTrigger;
    [SerializeField] private GameObject _cutsceneCamera;
    [SerializeField] private EventReference _stinger;
    private Coroutine _cutsceneCoroutine;
    private Coroutine _slabCoroutine;
    private EventInstance _stingerInstance;
    private EventInstance _stonesStartInstance;
    private EventInstance _stonesImpactInstance;

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

        _conversationManager.StartConversation();
        
        yield return null;
    }

    public void RequestSkip() {
        _cutsceneCamera.SetActive(false);
        StopCoroutine(_cutsceneCoroutine);
        StopCoroutine(_slabCoroutine);
        _conversationManager.HardStopConversation();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;

        AudioUtils.SafeStop(ref _stingerInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        AudioUtils.SafeStop(ref _stonesStartInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        AudioUtils.SafeStop(ref _stonesImpactInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);

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
        _conversationManager.CleanUp();
        _cutsceneCamera.SetActive(false);
        PlayerMovement.obj.UnFreeze();
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        GameManager.obj.RegisterEvent(_isCutsceneCompleted);
        PauseMenuManager.obj.UnregisterSkippable();
    }
}
