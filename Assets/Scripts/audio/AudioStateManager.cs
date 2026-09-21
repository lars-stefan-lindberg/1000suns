using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using System.Collections;

public enum ReverbZone
{
    Forest = 0,
    Cave = 1
}

public class AudioStateManager : MonoBehaviour
{
    public static AudioStateManager obj;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float fadeDialogueDuration = 1f;

    private Bus gameplaySfxBus;
    private PARAMETER_ID pauseParamId;
    private PARAMETER_ID dialogueParamId;
    private PARAMETER_ID reverbZoneParamId;
    private Coroutine fadeRoutine;
    private Coroutine fadeDialogueRoutine;

    void Awake()
    {
        obj = this;
        gameplaySfxBus = RuntimeManager.GetBus("bus:/gameplay_sfx");

        RuntimeManager.StudioSystem.getParameterDescriptionByName(
            "Pause",
            out PARAMETER_DESCRIPTION pauseDesc
        );

        pauseParamId = pauseDesc.id;

        RuntimeManager.StudioSystem.getParameterDescriptionByName(
            "Dialogue",
            out PARAMETER_DESCRIPTION dialogueDesc
        );

        dialogueParamId = dialogueDesc.id;

        RuntimeManager.StudioSystem.getParameterDescriptionByName(
            "reverb_zone",
            out PARAMETER_DESCRIPTION reverbZoneDesc
        );

        reverbZoneParamId = reverbZoneDesc.id;
    }

    public void SetPaused(bool paused)
    {
        gameplaySfxBus.setPaused(paused);

        float target = paused ? 1f : 0f;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadePause(target));
    }

    public void SetDialogue(bool dialogue) {
        float target = dialogue ? 1f : 0f;

        if (fadeDialogueRoutine != null)
            StopCoroutine(fadeDialogueRoutine);

        fadeDialogueRoutine = StartCoroutine(FadeDialogue(target));
    }

    // Restore volume and low pass filter
    public void RestoreMusic() {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadePause(0f));
    }

    public void PauseSfx() {
        gameplaySfxBus.setPaused(true);
    }

    public void RestoreSfx() {
        gameplaySfxBus.setPaused(false);
    }

    public void SetReverbZone(ReverbZone zone)
    {
        RuntimeManager.StudioSystem.setParameterByID(reverbZoneParamId, (float)zone);
    }

    public void StopSfxEvents() {
        // Kill gameplay audio completely
        gameplaySfxBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    private IEnumerator FadePause(float target)
    {
        RuntimeManager.StudioSystem.getParameterByID(
            pauseParamId,
            out float startValue
        );

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / fadeDuration;

            float value = Mathf.Lerp(startValue, target, t);
            RuntimeManager.StudioSystem.setParameterByID(pauseParamId, value);

            yield return null;
        }

        RuntimeManager.StudioSystem.setParameterByID(pauseParamId, target);
    }

    private IEnumerator FadeDialogue(float target)
    {
        RuntimeManager.StudioSystem.getParameterByID(
            dialogueParamId,
            out float startValue
        );

        float time = 0f;

        while (time < fadeDialogueDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / fadeDialogueDuration;

            float value = Mathf.Lerp(startValue, target, t);
            RuntimeManager.StudioSystem.setParameterByID(dialogueParamId, value);

            yield return null;
        }

        RuntimeManager.StudioSystem.setParameterByID(dialogueParamId, target);
    }

    void OnDestroy() {
        obj = null;
    }
}
