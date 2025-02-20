using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Reaper : MonoBehaviour
{
    public static Reaper obj;
    public bool playerKilled = false;

    void Awake() {
        obj = this;
    }

    public float genericDeathAnimationTime = 0.8f;
    public float shadowDeathAnimationTime = 0.8f;
    public void KillPlayerGeneric() {
        if(!playerKilled){
            playerKilled = true;
            PlayerStatsManager.obj.numberOfDeaths += 1;
            PlayerMovement.obj.Freeze(genericDeathAnimationTime);
            SoundFXManager.obj.PlayPlayerGenericDeath(Player.obj.transform);
            Player.obj.PlayGenericDeathAnimation();
            StartCoroutine(AfterDeathAnimation(genericDeathAnimationTime));
        }
    }

    public void KillPlayerShadow() {
        if(!playerKilled){
            playerKilled = true;
            PlayerStatsManager.obj.numberOfDeaths += 1;
            PlayerMovement.obj.Freeze(shadowDeathAnimationTime);
            SoundFXManager.obj.PlayPlayerShadowDeath(Player.obj.transform);
            Player.obj.PlayShadowDeathAnimation();
            StartCoroutine(AfterDeathAnimation(shadowDeathAnimationTime));
        }
    }

    public void KillPrisoner(Prisoner prisoner) {
        prisoner.InitiateKill();
    }

    private IEnumerator AfterDeathAnimation(float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        Player.obj.gameObject.SetActive(false);
        SceneFadeManager.obj.StartFadeOut();
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }
        LevelManager.obj.ReloadCurrentScene();
    }

    void OnDestroy() {
        obj = null;
    }
}
