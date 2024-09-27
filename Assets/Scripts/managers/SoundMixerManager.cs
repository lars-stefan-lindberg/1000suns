using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    public static SoundMixerManager obj;

    [SerializeField] AudioMixer audioMixer;

    //Level between 0.0001 and 1
    public void SetMasterVolume(float level) {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);
    }
    public float GetMasterVolume() {
        return GetVolume("masterVolume");
    }

    public void SetSoundFXVolume(float level) {
        audioMixer.SetFloat("soundFXVolume", Mathf.Log10(level) * 20f);
    }
    public float GetSoundFXVolume() {
        return GetVolume("soundFXVolume");
    }

    public void SetMusicVolume(float level) {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);
    }
    public float GetMusicVolume() {
        return GetVolume("musicVolume");
    }

    private float GetVolume(string param) {
        float currentVolume;
        audioMixer.GetFloat(param, out currentVolume);
        currentVolume = Mathf.Pow(10, currentVolume / 20);
        return currentVolume;
    }

    public IEnumerator StartMasterFade(float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat("masterVolume", out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat("masterVolume", Mathf.Log10(newVol) * 20);
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
