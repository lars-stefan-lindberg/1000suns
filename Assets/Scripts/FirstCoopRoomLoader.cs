using System.Collections;
using System.Linq;
using FunkyCode;
using UnityEngine;

public class FirstCoopRoomLoader : MonoBehaviour
{
    void Start()
    {
        Player.obj.SetCaveStartingCoordinatesCoop();
        Player.obj.gameObject.SetActive(true);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.CancelJumping();
        PlayerMovement.obj.Freeze();
        PlayerMovement.obj.spriteRenderer.flipX = false;

        CaveAvatar.obj.gameObject.SetActive(true);
        CaveAvatar.obj.SetFollowPlayerStartingPosition();
        CaveAvatar.obj.FollowPlayer();

        ShadowTwinPlayer.obj.SetCaveStartingCoordinatesCoop();
        ShadowTwinPlayer.obj.gameObject.SetActive(true);
        ShadowTwinMovement.obj.SetStartingOnGround();
        ShadowTwinMovement.obj.isGrounded = true;
        ShadowTwinMovement.obj.CancelJumping();
        ShadowTwinMovement.obj.Freeze();
        ShadowTwinMovement.obj.spriteRenderer.flipX = true;

        if(PlayerManager.obj.IsCoopActive) {
            LobbyManager.obj.SetPlayerInputs();
        } else {
            PlayerSwitcher.obj.SwitchToEli();
        }

        StartCoroutine(FadeInAndPlaySounds());
        StartCoroutine(AmbienceFadeIn());
        LightingManager2D.Get().profile.DarknessColor = new Color(0.33f, 0.33f, 0.33f, 1f);

        PlayerStatsManager.obj.ResumeTimer();
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            SceneFadeManager.obj.SetFadeInSpeed(5f);
            //Destroy(this, 15);
        }
    }

    private IEnumerator FadeInAndPlaySounds() {
        yield return new WaitForSeconds(2f);
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(0.2f);
        SceneFadeManager.obj.StartFadeIn();
        yield return new WaitForSeconds(3f);
        PlayerMovement.obj.UnFreeze();
        ShadowTwinMovement.obj.UnFreeze();
        GameEventManager.obj.IsPauseAllowed = true;

        yield return null;
    }

    private IEnumerator AmbienceFadeIn() {
        AmbienceManager.obj.PlayCaveAmbience();
        AmbienceManager.obj.FadeInAmbienceSource1(2f);
        yield return null;
    }
}
