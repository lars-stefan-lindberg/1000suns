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
    public void KillPlayerGeneric(PlayerManager.PlayerType playerType) {
        if(PlayerManager.obj.IsSeparated) {
            PlayerStatsManager.obj.numberOfDeaths += 1;
            
            SceneMetadata metadata = LevelManager.obj.GetActiveSceneMetadata();
            bool shouldReload = metadata != null && metadata.ShouldReloadOnDeath();
            if(shouldReload) {
                PlayerManager.obj.KillPlayerGeneric(PlayerManager.PlayerType.HUMAN, genericDeathAnimationTime);
                PlayerManager.obj.KillPlayerGeneric(PlayerManager.PlayerType.SHADOW_TWIN, genericDeathAnimationTime);
                SoundFXManager.obj.PlayPlayerGenericDeath(PlayerManager.obj.GetPlayerTransform(playerType));
                StartCoroutine(AfterDeathAnimationIsSeparated(genericDeathAnimationTime));
            } else {
                PlayerManager.obj.KillPlayerGeneric(playerType, genericDeathAnimationTime);
                Transform playerTransform = PlayerManager.obj.GetPlayerTransform(playerType);
                SoundFXManager.obj.PlayPlayerGenericDeath(playerTransform);
                StartCoroutine(SpawnAfterDeathAnimation(playerType, genericDeathAnimationTime));
            }
        }
        else if(!playerKilled){
            playerKilled = true;
            PlayerStatsManager.obj.numberOfDeaths += 1;
            PlayerManager.obj.KillPlayerGeneric(playerType, genericDeathAnimationTime);
            SoundFXManager.obj.PlayPlayerGenericDeath(PlayerManager.obj.GetPlayerTransform());
            StartCoroutine(AfterDeathAnimation(genericDeathAnimationTime));
        }
    }

    public void KillAllPlayersGeneric() {
        if(PlayerManager.obj.IsSeparated) {
            playerKilled = true;
            PlayerStatsManager.obj.numberOfDeaths += 1;
            PlayerManager.obj.KillPlayerGeneric(PlayerManager.PlayerType.HUMAN, genericDeathAnimationTime);
            PlayerManager.obj.KillPlayerGeneric(PlayerManager.PlayerType.SHADOW_TWIN, genericDeathAnimationTime);
            SoundFXManager.obj.PlayPlayerGenericDeath(PlayerManager.obj.GetPlayerTransform());
            StartCoroutine(AfterDeathAnimationIsSeparated(genericDeathAnimationTime));
        } else {
            playerKilled = true;
            PlayerStatsManager.obj.numberOfDeaths += 1;
            PlayerManager.obj.KillPlayerGeneric(genericDeathAnimationTime);
            SoundFXManager.obj.PlayPlayerGenericDeath(PlayerManager.obj.GetPlayerTransform());
            StartCoroutine(AfterDeathAnimation(genericDeathAnimationTime));
        }
    }

    public void KillPlayerShadow(PlayerManager.PlayerType playerType) {
        if(PlayerManager.obj.IsSeparated) {
            PlayerStatsManager.obj.numberOfDeaths += 1;
            
            SceneMetadata metadata = LevelManager.obj.GetActiveSceneMetadata();
            bool shouldReload = metadata != null && metadata.ShouldReloadOnDeath();
            if(shouldReload) {
                PlayerManager.obj.KillPlayerShadow(PlayerManager.PlayerType.HUMAN, genericDeathAnimationTime);
                PlayerManager.obj.KillPlayerShadow(PlayerManager.PlayerType.SHADOW_TWIN, genericDeathAnimationTime);
                SoundFXManager.obj.PlayPlayerShadowDeath(PlayerManager.obj.GetPlayerTransform(playerType));
                StartCoroutine(AfterDeathAnimationIsSeparated(genericDeathAnimationTime));
            } else {
                PlayerManager.obj.KillPlayerShadow(playerType, genericDeathAnimationTime);
                Transform playerTransform = PlayerManager.obj.GetPlayerTransform(playerType);
                SoundFXManager.obj.PlayPlayerShadowDeath(playerTransform);
                StartCoroutine(SpawnAfterDeathAnimation(playerType, genericDeathAnimationTime));
            }
        }
        else if(!playerKilled){
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

    private IEnumerator AfterDeathAnimationIsSeparated(float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        PlayerManager.obj.SetPlayerGameObjectsInactive();
        SceneFadeManager.obj.StartFadeOut();
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }
        CollectibleManager.obj.ClearNonFollowingCollectibles();
        LevelManager.obj.ReloadCurrentScene();
    }

    private IEnumerator SpawnAfterDeathAnimation(PlayerManager.PlayerType playerType, float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        PlayerManager.obj.SetPlayerGameObjectInactive(playerType);

        LevelManager.obj.SpawnPlayer(playerType);
    }

    void OnDestroy() {
        obj = null;
    }
}
