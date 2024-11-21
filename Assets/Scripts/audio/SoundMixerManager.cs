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

    private float _musicVolumeBeforeFade = 0;

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
            currentTime += Time.deltaTime;
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
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(MUSIC_VOLUME_PARAM, Mathf.Log10(newVol) * 20);
            yield return null;
        }
        yield break;
    }

    void Awake() {
        obj = this;
    }

    void OnDestroy() {
        obj = null;
    }
}
