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
        StartCoroutine(DelayedEnablePlayerMovement());
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            SceneFadeManager.obj.SetFadeInSpeed(5f);
            Destroy(this, 5);
        }
    }

    private IEnumerator AmbienceFadeIn() {
        float v = SoundMixerManager.obj.GetAmbienceVolume();
        SoundMixerManager.obj.SetAmbienceVolume(0.001f);
        AmbienceManager.obj.PlayAmbience();
        StartCoroutine(SoundMixerManager.obj.StartAmbienceFade(3f, v));

        yield return null;
    }

    private IEnumerator DelayedEnablePlayerMovement() {
        yield return new WaitForSeconds(3);
        PlayerMovement.obj.EnablePlayerMovement();
    }
}
