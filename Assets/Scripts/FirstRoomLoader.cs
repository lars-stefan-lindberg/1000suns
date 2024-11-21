using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstRoomLoader : MonoBehaviour
{
    void Start()
    {
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(1f);
        SceneFadeManager.obj.StartFadeIn();

        StartCoroutine(AmbienceFadeIn());
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            SceneFadeManager.obj.SetFadeInSpeed(5f);
            Destroy(this);
        }
    }

    private IEnumerator AmbienceFadeIn() {
        float v = SoundMixerManager.obj.GetAmbienceVolume();
        SoundMixerManager.obj.SetAmbienceVolume(0.001f);
        AmbienceManager.obj.PlayAmbience();
        StartCoroutine(SoundMixerManager.obj.StartAmbienceFade(3f, v));

        yield return null;
    }
}
