using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstRoomLoader : MonoBehaviour
{
    void Start()
    {
        if(!GameEventManager.obj.CaveLevelStarted) {
            SceneFadeManager.obj.SetFadedOutState();
            SceneFadeManager.obj.SetFadeInSpeed(0.5f);
            SceneFadeManager.obj.StartFadeIn();

            StartCoroutine(AmbienceFadeIn());
            StartCoroutine(DelayedEnablePlayerMovement());

            GameEventManager.obj.CaveLevelStarted = true;

            PlayerStatsManager.obj.ResumeTimer();
        }
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            SceneFadeManager.obj.SetFadeInSpeed(5f);
            Destroy(this, 5);
        }
    }

    private IEnumerator AmbienceFadeIn() {
        AmbienceManager.obj.PlayCaveAmbience();
        AmbienceManager.obj.FadeInAmbienceSource1(2f);
        yield return null;
    }

    private IEnumerator DelayedEnablePlayerMovement() {
        yield return new WaitForSeconds(3);
        PlayerMovement.obj.EnablePlayerMovement();
    }
}
