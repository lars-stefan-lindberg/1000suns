using System.Collections;
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

    private const float DEFAULT_CUTOFF_FREQUENCY = 22000f;
    private const float MUFFLED_CUTOFF_FREQUENCY = 1000f;
    
    // The player's preferred music volume (what they set with the slider)
    private float _playerPreferredMusicVolume = 0.8002f;
    // Flag to track if music is currently muffled
    private bool _isMusicMuffled = false;
    // Ratio to apply to the player's volume when muffled (80% of original volume)
    private const float MUFFLED_VOLUME_RATIO = 0.8f;

    // Reference to active low-pass filter coroutine
    private Coroutine _activeLowPassFadeCoroutine = null;
    
    [SerializeField] AudioMixer audioMixer;

    void Awake() {
        obj = this;
        
        // Initialize the low-pass filter to the default (clear) value
        SetMusicLowPassFilter(DEFAULT_CUTOFF_FREQUENCY);
        
        // Initialize the player's preferred music volume to the current music volume
        _playerPreferredMusicVolume = Mathf.Log10(0.8002f) * 20f;
        audioMixer.SetFloat(MUSIC_VOLUME_PARAM, _playerPreferredMusicVolume);
    }

    void OnDestroy() {
        obj = null;
    }

    // Set the player's preferred music volume (the volume they want, regardless of muffling)
    public void SetPlayerPreferredMusicVolume(float volume) {
        // Always store the player's preferred volume
        _playerPreferredMusicVolume = volume;
        
        // Always set the actual volume directly - this ensures immediate feedback
        // When the slider is released, the muffled state will be reapplied if needed
        SetMusicVolume(volume);
    }
    
    // Get the player's preferred music volume (the volume they want, regardless of muffling)
    public float GetPlayerPreferredMusicVolume() {
        return _playerPreferredMusicVolume;
    }

    // Apply a muffled music effect - affects both low-pass filter and volume
    public IEnumerator StartMusicMuffle(float duration, float targetCutoffFrequency = MUFFLED_CUTOFF_FREQUENCY) {
        // Stop any active coroutines
        StopLowPassFadeCoroutine();
        
        // Store current volume as preferred before muffling if not already muffled
        if (!_isMusicMuffled) {
            _playerPreferredMusicVolume = GetMusicVolume();
        }
        
        // Set muffled state first
        _isMusicMuffled = true;
        
        // Calculate muffled volume based on player's preferred volume
        float muffledVolume = _playerPreferredMusicVolume * MUFFLED_VOLUME_RATIO;
        
        // Immediately set to muffled volume
        SetMusicVolume(muffledVolume);
        
        // Start the cutoff frequency fade for the filter effect
        _activeLowPassFadeCoroutine = StartCoroutine(InternalStartLowPassFade(duration, GetMusicLowPassFilter(), targetCutoffFrequency));
        
        yield return null;
    }

    // Restore clear music - affects both low-pass filter and volume
    public IEnumerator StartMusicUnmuffle(float duration) {
        // Stop any active coroutines
        StopLowPassFadeCoroutine();
        
        // Set unmuffled state first
        _isMusicMuffled = false;
        
        // Immediately restore to the player's preferred volume
        SetMusicVolume(_playerPreferredMusicVolume);
        
        // Start the cutoff frequency fade for the filter effect
        _activeLowPassFadeCoroutine = StartCoroutine(InternalStartLowPassFade(duration, GetMusicLowPassFilter(), DEFAULT_CUTOFF_FREQUENCY));
        
        yield return null;
    }
    
    // Temporarily restore normal music settings when adjusting volume slider
    public IEnumerator TemporarilyRestoreMusicForVolumeAdjustment(float duration) {
        // Only do this if the music is currently muffled
        if (_isMusicMuffled) {
            StopLowPassFadeCoroutine();
            
            // Restore full volume so the player can hear the actual volume they're setting
            SetMusicVolume(_playerPreferredMusicVolume);
            
            // Start the cutoff frequency fade for the filter effect
            _activeLowPassFadeCoroutine = StartCoroutine(InternalStartLowPassFade(duration, GetMusicLowPassFilter(), DEFAULT_CUTOFF_FREQUENCY));
        }
        yield return null;
    }
    
    // Re-muffle the music when done adjusting volume
    public IEnumerator ReapplyMusicMuffleAfterVolumeAdjustment(float duration) {
        // Only do this if the music is currently muffled
        if (_isMusicMuffled) {
            StopLowPassFadeCoroutine();
            
            // Calculate muffled volume based on player's preferred volume
            float muffledVolume = _playerPreferredMusicVolume * MUFFLED_VOLUME_RATIO;
            
            // Now we need to reapply the muffled volume
            SetMusicVolume(muffledVolume);
            
            // Start the cutoff frequency fade for the filter effect
            _activeLowPassFadeCoroutine = StartCoroutine(InternalStartLowPassFade(duration, GetMusicLowPassFilter(), MUFFLED_CUTOFF_FREQUENCY));
        }
        yield return null;
    }

    // Helper method to stop any active low pass fade coroutine
    private void StopLowPassFadeCoroutine() {
        if (_activeLowPassFadeCoroutine != null) {
            StopCoroutine(_activeLowPassFadeCoroutine);
            _activeLowPassFadeCoroutine = null;
        }
    }

    // Internal coroutine to handle the low-pass filter transition
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
        currentVolume = DecibelToLinear(currentVolume);
        return currentVolume;
    }

    private float DecibelToLinear(float decibels) {
        return Mathf.Pow(10, decibels / 20);
    }

    private float LinearToDecibel(float linear) {
        return Mathf.Log10(linear) * 20;
    }

    public IEnumerator StartMasterFade(float duration, float targetVolume)
    {
        StartCoroutine(InternalStartVolumeFade(MASTER_VOLUME_PARAM, duration, targetVolume));
        yield return null;
    }

    public IEnumerator StartMusicFade(float duration, float targetVolume) {
        StartCoroutine(InternalStartVolumeFade(MUSIC_VOLUME_PARAM, duration, targetVolume));
        yield return null;
    }

    public IEnumerator StartAmbienceFade(float duration, float targetVolume) {
        StartCoroutine(InternalStartVolumeFade(AMBIENCE_VOLUME_PARAM, duration, targetVolume));
        yield return null;
    }

    public IEnumerator StartMusicVolumeRaise(float duration)
    {
        // If we're muffled, we need to unmute first
        if (_isMusicMuffled) {
            // Set unmuffled state
            _isMusicMuffled = false;
            
            // Start the cutoff frequency fade for the filter effect
            StopLowPassFadeCoroutine();
            _activeLowPassFadeCoroutine = StartCoroutine(InternalStartLowPassFade(duration, GetMusicLowPassFilter(), DEFAULT_CUTOFF_FREQUENCY));
        }
        
        // Raise to the player's preferred volume
        float targetValue = Mathf.Clamp(_playerPreferredMusicVolume, 0.0001f, 1);
        
        // Use the internal fade method for smooth transition
        StartCoroutine(InternalStartVolumeFade(MUSIC_VOLUME_PARAM, duration, targetValue));
        
        yield return null;
    }

    // Internal method to fade a volume parameter
    private IEnumerator InternalStartVolumeFade(string volumeParam, float duration, float targetVolume) {
        float currentVolume;
        audioMixer.GetFloat(volumeParam, out currentVolume);
        currentVolume = DecibelToLinear(currentVolume);
        
        float currentTime = 0;
        
        while (currentTime < duration) {
            currentTime += Time.unscaledDeltaTime;
            float t = currentTime / duration;
            
            // Apply easing function for smoother transition
            t = EaseInOutCubic(t);
            
            float newVolume = Mathf.Lerp(currentVolume, targetVolume, t);
            audioMixer.SetFloat(volumeParam, LinearToDecibel(newVolume));
            yield return null;
        }
        
        // Ensure we set the exact target value at the end
        audioMixer.SetFloat(volumeParam, LinearToDecibel(targetVolume));
        
        yield break;
    }

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
}
