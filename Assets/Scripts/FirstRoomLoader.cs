using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstRoomLoader : MonoBehaviour
{
    void Start()
    {
        if(!GameEventManager.obj.CaveLevelStarted) {
            StartCoroutine(FadeInAndPlaySounds());
            StartCoroutine(AmbienceFadeIn());
            StartCoroutine(DelayedEnablePlayerMovement());

            GameEventManager.obj.CaveLevelStarted = true;

            PlayerStatsManager.obj.ResumeTimer();
        }
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            SceneFadeManager.obj.SetFadeInSpeed(5f);
            Destroy(this, 10);
        }
    }

    private IEnumerator FadeInAndPlaySounds() {
        SoundFXManager.obj.PlayPlayerLongFall();
        yield return new WaitForSeconds(2.5f);
        SoundFXManager.obj.PlayPlayerLandHeavy();
        yield return new WaitForSeconds(2f);
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(0.2f);
        SceneFadeManager.obj.StartFadeIn();

        yield return null;
    }

    private IEnumerator AmbienceFadeIn() {
        AmbienceManager.obj.PlayCaveAmbience();
        AmbienceManager.obj.FadeInAmbienceSource1(2f);
        yield return null;
    }

    private IEnumerator DelayedEnablePlayerMovement() {
        yield return new WaitForSeconds(5);
        PlayerMovement.obj.EnablePlayerMovement();
    }
}
