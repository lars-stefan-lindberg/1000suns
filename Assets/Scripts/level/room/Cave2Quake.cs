using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave2Quake : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _quakeEventId;
    [SerializeField] private float _earthquakeFadeOutDuration = 0.25f;
    [SerializeField] private List<ParticleSystem> _dustParticles;
    private Coroutine _cutsceneCoroutine;
    private Coroutine _earthquakeFadeRoutine;
    private AudioSource _earthquakeSource;
    private float _earthquakeStartVolume = 1f;

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
        BeginFadeOutEarthquake();
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
        if (SoundFXManager.obj == null)
            return;

        if (_earthquakeFadeRoutine != null)
        {
            StopCoroutine(_earthquakeFadeRoutine);
            _earthquakeFadeRoutine = null;
        }

        if (_earthquakeSource == null || !_earthquakeSource.isPlaying)
        {
            _earthquakeSource = SoundFXManager.obj.PlayEarthquake();
            if (_earthquakeSource != null)
                _earthquakeStartVolume = _earthquakeSource.volume;
        }

        if (_earthquakeSource != null)
            _earthquakeSource.volume = _earthquakeStartVolume;
    }

    private void BeginFadeOutEarthquake()
    {
        if (_earthquakeSource == null)
            return;

        if (_earthquakeFadeRoutine != null)
            StopCoroutine(_earthquakeFadeRoutine);

        _earthquakeFadeRoutine = StartCoroutine(FadeOutAndStop(_earthquakeSource, _earthquakeFadeOutDuration));
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        if (source == null)
            yield break;

        float t = 0f;
        float startVolume = source.volume;
        duration = Mathf.Max(0.01f, duration);

        while (t < duration)
        {
            if (source == null)
                yield break;

            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            source.volume = Mathf.Lerp(startVolume, 0f, a);
            yield return null;
        }

        if (source != null)
        {
            source.volume = 0f;
            source.Stop();
            source.volume = _earthquakeStartVolume;
        }

        if (source == _earthquakeSource)
            _earthquakeFadeRoutine = null;
    }
}
