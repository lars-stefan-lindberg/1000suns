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
            PlayerMovement.obj.Freeze(genericDeathAnimationTime);
            CollectibleManager.obj.ResetTemporaryPickedCollectible();
            Player.obj.PlayGenericDeathAnimation();
            StartCoroutine(AfterDeathAnimation(genericDeathAnimationTime));
        }
    }

    public void KillPlayerShadow() {
        if(!playerKilled){
            playerKilled = true;
            PlayerMovement.obj.Freeze(shadowDeathAnimationTime);
            CollectibleManager.obj.ResetTemporaryPickedCollectible();
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

    void Destroy() {
        obj = null;
    }
}
