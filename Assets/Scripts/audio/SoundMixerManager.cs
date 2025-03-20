using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    public static SoundMixerManager obj;

    private const string MASTER_VOLUME_PARAM = "masterVolume";
    private const string MUSIC_VOLUME_PARAM = "musicVolume";
    private const string SOUNDFX_VOLUME_PARAM = "soundFXVolume";
    private const string AMBIENCE_VOLUME_PARAM = "ambienceVolume";
    private const string MUSIC_LOWPASS_PARAM = "musicLowPassCutoff";

    private float _musicVolumeBeforeFade = 0;
    private const float DEFAULT_CUTOFF_FREQUENCY = 22000f;
    private const float MUFFLED_CUTOFF_FREQUENCY = 1000f;
    private float _musicLowPassBeforeMuffle = DEFAULT_CUTOFF_FREQUENCY; // Default cutoff frequency

    // Reference to active low-pass filter coroutine so we can stop it if needed
    private Coroutine _activeLowPassFadeCoroutine = null;

    [SerializeField] AudioMixer audioMixer;

    //Level between 0.0001 and 1
    public void SetMasterVolume(float level) {
        audioMixer.SetFloat(MASTER_VOLUME_PARAM, Mathf.Log10(level) * 20f);
    }
    public float GetMasterVolume() {
        return GetVolume(MASTER_VOLUME_PARAM);
    }

    public void SetSoundFXVolume(float level) {
        audioMixer.SetFloat(SOUNDFX_VOLUME_PARAM, Mathf.Log10(level) * 20f);
    }
    public float GetSoundFXVolume() {
        return GetVolume(SOUNDFX_VOLUME_PARAM);
    }

    public void SetMusicVolume(float level) {
        audioMixer.SetFloat(MUSIC_VOLUME_PARAM, Mathf.Log10(level) * 20f);
    }
    public float GetMusicVolume() {
        return GetVolume(MUSIC_VOLUME_PARAM);
    }

    public void SetAmbienceVolume(float level) {
        audioMixer.SetFloat(AMBIENCE_VOLUME_PARAM, Mathf.Log10(level) * 20f);
    }
    public float GetAmbienceVolume() {
        return GetVolume(AMBIENCE_VOLUME_PARAM);
    }

    // Set the low-pass filter cutoff frequency for music
    // Lower values (500-2000) create a muffled effect, higher values (10000-22000) sound clearer
    public void SetMusicLowPassFilter(float cutoffFrequency) {
        audioMixer.SetFloat(MUSIC_LOWPASS_PARAM, cutoffFrequency);
    }

    // Get the current low-pass filter cutoff frequency
    public float GetMusicLowPassFilter() {
        float currentCutoff;
        audioMixer.GetFloat(MUSIC_LOWPASS_PARAM, out currentCutoff);
        return currentCutoff;
    }

    // Enable muffled music effect with a specific cutoff frequency
    public void MuffleMusic(float cutoffFrequency = MUFFLED_CUTOFF_FREQUENCY) {
        // Stop any active low pass fade coroutine
        StopLowPassFadeCoroutine();
        
        _musicLowPassBeforeMuffle = GetMusicLowPassFilter();
        SetMusicLowPassFilter(cutoffFrequency);
    }

    // Disable muffled music effect, restoring previous clarity
    public void UnmuffleMusic() {
        // Stop any active low pass fade coroutine
        StopLowPassFadeCoroutine();
        
        // Always set to the default value (22000f) when unmuffling
        SetMusicLowPassFilter(DEFAULT_CUTOFF_FREQUENCY);
    }

    // Helper method to stop any active low pass fade coroutine
    private void StopLowPassFadeCoroutine() {
        if (_activeLowPassFadeCoroutine != null) {
            StopCoroutine(_activeLowPassFadeCoroutine);
            _activeLowPassFadeCoroutine = null;
        }
    }

    // Gradually transition to a muffled music effect
    public IEnumerator StartMusicMuffle(float duration, float targetCutoffFrequency = MUFFLED_CUTOFF_FREQUENCY) {
        // Stop any active low pass fade coroutine
        StopLowPassFadeCoroutine();
        
        _musicLowPassBeforeMuffle = GetMusicLowPassFilter();
        _activeLowPassFadeCoroutine = StartCoroutine(InternalStartLowPassFade(duration, GetMusicLowPassFilter(), targetCutoffFrequency));
        yield return null;
    }

    // Gradually transition back to clear music
    public IEnumerator StartMusicUnmuffle(float duration) {
        // Stop any active low pass fade coroutine
        StopLowPassFadeCoroutine();
        
        // Always return to the default cutoff value (22000f) when unmuffling
        _activeLowPassFadeCoroutine = StartCoroutine(InternalStartLowPassFade(duration, GetMusicLowPassFilter(), DEFAULT_CUTOFF_FREQUENCY));
        yield return null;
    }

    // Internal coroutine to handle the low-pass filter transition
    private IEnumerator StartLowPassFade(float duration, float targetCutoffFrequency)
    {
        // Stop any active low pass fade coroutine
        StopLowPassFadeCoroutine();
        
        float currentCutoff = GetMusicLowPassFilter();
        float targetValue = Mathf.Clamp(targetCutoffFrequency, 10f, DEFAULT_CUTOFF_FREQUENCY);
        
        // Store the coroutine reference so we can stop it later
        _activeLowPassFadeCoroutine = StartCoroutine(InternalStartLowPassFade(duration, currentCutoff, targetValue));
        yield return null;
    }

    private IEnumerator InternalStartLowPassFade(float duration, float startCutoff, float targetValue)
    {
        float currentTime = 0;
        // Convert to logarithmic space for more natural-sounding transitions
        float logStartCutoff = Mathf.Log10(startCutoff);
        float logTargetCutoff = Mathf.Log10(targetValue);
        
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime to work even when timeScale is 0
            float t = currentTime / duration;
            
            // Optional: Apply easing function for even smoother transition
            t = EaseInOutCubic(t);
            
            // Interpolate in logarithmic space
            float logNewCutoff = Mathf.Lerp(logStartCutoff, logTargetCutoff, t);
            // Convert back to linear space
            float newCutoff = Mathf.Pow(10, logNewCutoff);
            
            SetMusicLowPassFilter(newCutoff);
            yield return null;
        }
        
        // Ensure we set the exact target value at the end
        SetMusicLowPassFilter(targetValue);
        _activeLowPassFadeCoroutine = null;
        yield break;
    }
    
    // Cubic easing function for smoother transitions
    private float EaseInOutCubic(float t)
    {
        return t < 0.5 ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
    }

    private float GetVolume(string param) {
        float currentVolume;
        audioMixer.GetFloat(param, out currentVolume);
        currentVolume = Mathf.Pow(10, currentVolume / 20);
        return currentVolume;
    }

    public IEnumerator StartMasterFade(float duration, float targetVolume)
    {
        StartCoroutine(StartVolumeFade(MASTER_VOLUME_PARAM, duration, targetVolume));
        yield return null;
    }

    public IEnumerator StartMusicFade(float duration, float targetVolume) {
        _musicVolumeBeforeFade = GetMusicVolume();
        StartCoroutine(StartVolumeFade(MUSIC_VOLUME_PARAM, duration, targetVolume));
        yield return null;
    }

    public IEnumerator StartAmbienceFade(float duration, float targetVolume) {
        StartCoroutine(StartVolumeFade(AMBIENCE_VOLUME_PARAM, duration, targetVolume));
        yield return null;
    }

    private IEnumerator StartVolumeFade(string mixerParam, float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat(mixerParam, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime to work even when timeScale is 0
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(mixerParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }
        yield break;
    }

    public IEnumerator StartMusicVolumeRaise(float duration)
    {
        if(_musicVolumeBeforeFade == 0) {
            yield break;
        }
        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat(MUSIC_VOLUME_PARAM, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(_musicVolumeBeforeFade, 0.0001f, 1);
        while (currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime to work even when timeScale is 0
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(MUSIC_VOLUME_PARAM, Mathf.Log10(newVol) * 20);
            yield return null;
        }
        yield break;
    }

    void Awake() {
        obj = this;
        
        // Initialize the low-pass filter to the default (clear) value
        SetMusicLowPassFilter(_musicLowPassBeforeMuffle);
    }

    void OnDestroy() {
        obj = null;
    }
}
