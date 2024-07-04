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

    public void KillPlayer() {
        PlayerMovement.obj.Freeze(deathAnimationTime);
        Player.obj.PlayGenericDeathAnimation();
        StartCoroutine(afterDeathAnimation(deathAnimationTime));
    }

    public float deathAnimationTime = 0.8f;

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
