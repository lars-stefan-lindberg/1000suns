using System.Collections;
using UnityEngine;

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
            PlayerManager.obj.KillPlayerGeneric(genericDeathAnimationTime);
            SoundFXManager.obj.PlayPlayerGenericDeath(PlayerManager.obj.GetPlayerTransform());
            StartCoroutine(AfterDeathAnimation(genericDeathAnimationTime));
        }
    }

    public void KillPlayerShadow() {
        if(!playerKilled){
            playerKilled = true;
            PlayerStatsManager.obj.numberOfDeaths += 1;
            PlayerManager.obj.KillPlayerShadow(shadowDeathAnimationTime);
            SoundFXManager.obj.PlayPlayerShadowDeath(PlayerManager.obj.GetPlayerTransform());
            StartCoroutine(AfterDeathAnimation(shadowDeathAnimationTime));
        }
    }

    public void KillPrisoner(Prisoner prisoner) {
        prisoner.InitiateKill();
    }

    private IEnumerator AfterDeathAnimation(float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        PlayerManager.obj.SetPlayerGameObjectInactive();
        SceneFadeManager.obj.StartFadeOut();
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }
        CollectibleManager.obj.ClearNonFollowingCollectibles();
        LevelManager.obj.ReloadCurrentScene();
    }

    void OnDestroy() {
        obj = null;
    }
}
