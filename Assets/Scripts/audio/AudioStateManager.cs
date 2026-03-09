using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using System.Collections;

public class AudioStateManager : MonoBehaviour
{
    public static AudioStateManager obj;
    [SerializeField] private float fadeDuration = 0.25f;

    private Bus gameplaySfxBus;
    private PARAMETER_ID pauseParamId;
    private Coroutine fadeRoutine;

    void Awake()
    {
        obj = this;
        gameplaySfxBus = RuntimeManager.GetBus("bus:/gameplay_sfx");

        RuntimeManager.StudioSystem.getParameterDescriptionByName(
            "Pause",
            out PARAMETER_DESCRIPTION pauseDesc
        );

        pauseParamId = pauseDesc.id;
    }

    public void SetPaused(bool paused)
    {
        gameplaySfxBus.setPaused(paused);

        float target = paused ? 1f : 0f;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadePause(target));
    }

    // Restore volume and low pass filter
    public void RestoreMusic() {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadePause(0f));
    }

    public void RestoreSfx() {
        gameplaySfxBus.setPaused(false);
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

    void OnDestroy() {
        obj = null;
    }
}
