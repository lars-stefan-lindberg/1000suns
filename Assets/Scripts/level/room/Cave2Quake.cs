using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class Cave2Quake : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _quakeEventId;
    [SerializeField] private List<ParticleSystem> _dustParticles;
    [SerializeField] private EventReference _earthquakeStinger;
    private EventInstance _earthQuakeStingerInstance;
    private Coroutine _cutsceneCoroutine;

    void Awake() {
        foreach(var dustParticle in _dustParticles) {
            dustParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void Activate() {
        if(GameManager.obj.HasEvent(_quakeEventId)) {
            return;
        }
        _cutsceneCoroutine = StartCoroutine(StartQuakeCutscene());
    }

    public void RequestSkip() {
        StopCoroutine(_cutsceneCoroutine);
        CameraShakeManager.obj.ShakeCamera(0, 0, 0);
        AudioUtils.SafeStop(ref _earthQuakeStingerInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        foreach(var dustParticle in _dustParticles) {
            dustParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        GameManager.obj.RegisterEvent(_quakeEventId);
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

    private IEnumerator StartQuakeCutscene() {
        PauseMenuManager.obj.RegisterSkippable(this);
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(0.5f);

        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 4.2f);

        StartEarthquake();

        foreach(var dustParticle in _dustParticles) {
            dustParticle.Play();
        }

        yield return new WaitForSeconds(4.5f);

        foreach(var dustParticle in _dustParticles) {
            dustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        BeginFadeOutEarthquake();

        PauseMenuManager.obj.UnregisterSkippable();
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.RegisterEvent(_quakeEventId);
        yield return null;
    }

    private void StartEarthquake()
    {
        _earthQuakeStingerInstance = SoundFXManager.obj.CreateAttachedInstance(_earthquakeStinger, gameObject, null);
        _earthQuakeStingerInstance.start();
    }

    private void BeginFadeOutEarthquake()
    {
        AudioUtils.SafeStop(ref _earthQuakeStingerInstance);
    }
}
