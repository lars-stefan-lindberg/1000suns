using System.Collections;
using System.Linq;
using Cinemachine;
using FunkyCode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToC29 : MonoBehaviour
{
    [SerializeField] private SceneField _sceneToTeleportTo;
    [SerializeField] private GameObject _cameraToDeactivate;
    [SerializeField] private GameObject _tutorialCanvas;
    [SerializeField] private GameObject _shockWaveEmitter;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            GetComponent<BoxCollider2D>().enabled = false;
            StartCoroutine(TeleportToC29Routine());
        }
    }

    private IEnumerator TeleportToC29Routine() {
        PlayerBlobMovement.obj.Freeze();
        MusicManager.obj.ScheduleClipOnNextBar(MusicManager.obj.caveIntense1Outro, 140, false);

        AmbienceManager.obj.SetCurrentAmbienceId(AmbienceManager.AmbienceId.None);
        AmbienceManager.obj.FadeOutAmbienceSource2And3(1f);

        SoundFXManager.obj.PlayCaveSpaceRoomTeleport();
        WhiteFadeManager.obj.StartFadeOut();

        yield return new WaitForSeconds(1f);

        LightingManager2D.Get().profile.DarknessColor = new Color(0.005f, 0.005f, 0.005f);

        PlayerBlobMovement.obj.ToHuman(false);
        Destroy(_shockWaveEmitter);
        yield return new WaitForSeconds(1f);

        Scene scene = SceneManager.GetSceneByName(_sceneToTeleportTo.SceneName);
        SceneManager.SetActiveScene(scene);
        GameObject[] sceneGameObjects = scene.GetRootGameObjects();
        
        _cameraToDeactivate.SetActive(false);
        CinemachineVirtualCamera cinemachineVirtualCamera = _cameraToDeactivate.GetComponent<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.enabled = false;

        //Skip this for now since we will play the main cave song instead
        //AmbienceManager.obj.PlayCaveAmbience();
        //AmbienceManager.obj.FadeInAmbienceSource1(1.5f);

        GameObject playerSpawnPoint = sceneGameObjects.First(gameObject => gameObject.CompareTag("AlternatePlayerSpawnPoint"));
        Collider2D playerSpawningCollider = playerSpawnPoint.GetComponent<Collider2D>();
        
        Player.obj.transform.position = playerSpawningCollider.transform.position;
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.isForcePushJumping = false;
        PlayerMovement.obj.jumpedWhileForcePushJumping = false;
        PlayerMovement.obj.CancelJumping();
        Player.obj.SetAnimatorLayerAndHasCape(true);

        Player.obj.PlayGetUpAnimation();

        yield return new WaitForSeconds(1f);

        WhiteFadeManager.obj.StartFadeIn();

        yield return new WaitForSeconds(2f);

        Player.obj.StartAnimator();

        yield return new WaitForSeconds(4f);

        //Start tutorial dialogue
        Time.timeScale = 0;
        _tutorialCanvas.SetActive(true);
        TutorialDialogManager.obj.StartFadeIn();
        SoundFXManager.obj.PlayPowerUpDialogueStinger();
        while(!TutorialDialogManager.obj.tutorialCompleted) {
            yield return null;
        }
        _tutorialCanvas.SetActive(false);
        Time.timeScale = 1;
        PlayerPowersManager.obj.EliCanTurnFromBlobToHuman = true;

        PlayerMovement.obj.UnFreeze();

        GameManager.obj.IsPauseAllowed = true;

        MusicManager.obj.PlayCaveSong();
        SaveManager.obj.SaveGame(scene.name);
        yield return null;
    }
}
