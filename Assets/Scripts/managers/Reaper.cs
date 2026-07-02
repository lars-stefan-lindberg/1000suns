using System.Collections;
using UnityEngine;

public class Reaper : MonoBehaviour
{
    public static Reaper obj;
    public bool playerKilled = false;
    private SharedCharacterAudio _sharedCharacterAudio;

    void Awake() {
        obj = this;
        _sharedCharacterAudio = GetComponent<SharedCharacterAudio>();
    }

    public float genericDeathAnimationTime = 0.8f;
    public float shadowDeathAnimationTime = 0.8f;
    public void KillPlayerGeneric(PlayerManager.PlayerType playerType) {
        if(PlayerManager.obj.IsSeparated) {
            PlayerStatsManager.obj.numberOfDeaths += 1;
            
            PlayerManager.obj.KillAllPlayersGeneric(genericDeathAnimationTime);
            _sharedCharacterAudio.PlayGenericDeath(PlayerManager.obj.GetPlayerTransform(playerType));
            StartCoroutine(AfterDeathAnimationIsSeparated(genericDeathAnimationTime));
        }
        else if(!playerKilled){
            playerKilled = true;
            PlayerStatsManager.obj.numberOfDeaths += 1;
            PlayerManager.obj.KillPlayerGeneric(playerType, genericDeathAnimationTime);
            _sharedCharacterAudio.PlayGenericDeath(PlayerManager.obj.GetPlayerTransform(playerType));
            StartCoroutine(AfterDeathAnimation(playerType, genericDeathAnimationTime));
        }
    }

    public void KillAllPlayersGeneric() {
        if(PlayerManager.obj.IsSeparated) {
            playerKilled = true;
            PlayerStatsManager.obj.numberOfDeaths += 1;
            PlayerManager.obj.KillAllPlayersGeneric(genericDeathAnimationTime);
            PlayerManager.PlayerType playerType = PlayerManager.obj.GetActivePlayerType();
            _sharedCharacterAudio.PlayGenericDeath(PlayerManager.obj.GetPlayerTransform(playerType));
            StartCoroutine(AfterDeathAnimationIsSeparated(genericDeathAnimationTime));
        } else {
            playerKilled = true;
            PlayerStatsManager.obj.numberOfDeaths += 1;
            PlayerManager.PlayerType playerType = PlayerManager.obj.GetActivePlayerType();
            PlayerManager.obj.KillPlayerGeneric(playerType, genericDeathAnimationTime);
            _sharedCharacterAudio.PlayGenericDeath(PlayerManager.obj.GetPlayerTransform(playerType));
            StartCoroutine(AfterDeathAnimation(playerType, genericDeathAnimationTime));
        }
    }

    public void KillPlayerShadow(PlayerManager.PlayerType playerType) {
        //TODO how to handle if separated?

        if(!playerKilled){
            playerKilled = true;
            PlayerStatsManager.obj.numberOfDeaths += 1;
            PlayerManager.obj.KillPlayerShadow(playerType, shadowDeathAnimationTime);
            _sharedCharacterAudio.PlayShadowDeath(PlayerManager.obj.GetPlayerTransform(playerType));
            StartCoroutine(AfterDeathAnimation(playerType, shadowDeathAnimationTime));
        }
    }

    public void KillPrisoner(Prisoner prisoner) {
        prisoner.InitiateKill();
    }

    private IEnumerator AfterDeathAnimation(PlayerManager.PlayerType playerType, float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        PlayerManager.obj.SetPlayerGameObjectInactive(playerType);
        SceneFadeManager.obj.StartFadeOut();
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }
        LevelManager.obj.ReloadCurrentScene();
    }

    private IEnumerator AfterDeathAnimationIsSeparated(float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        PlayerManager.obj.SetAllPlayerGameObjectsInactive();
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
