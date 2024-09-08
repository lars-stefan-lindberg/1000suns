using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Reaper : MonoBehaviour
{
    public static Reaper obj;

    void Awake() {
        obj = this;
    }

    public float genericDeathAnimationTime = 0.8f;
    public float shadowDeathAnimationTime = 0.8f;
    public void KillPlayerGeneric() {
        PlayerMovement.obj.Freeze(genericDeathAnimationTime);
        CollectibleManager.Instance.ResetTemporaryPickedCollectible();
        Player.obj.PlayGenericDeathAnimation();
        StartCoroutine(afterDeathAnimation(genericDeathAnimationTime));
    }

    public void KillPlayerShadow() {
        PlayerMovement.obj.Freeze(shadowDeathAnimationTime);
        CollectibleManager.Instance.ResetTemporaryPickedCollectible();
        Player.obj.PlayShadowDeathAnimation();
        StartCoroutine(afterDeathAnimation(shadowDeathAnimationTime));
    }

    public void KillPrisoner(Prisoner prisoner) {
        prisoner.InitiateKill();
    }

    private IEnumerator afterDeathAnimation(float waitingTime) {
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
