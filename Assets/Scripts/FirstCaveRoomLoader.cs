using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstCaveRoomLoader : MonoBehaviour
{
    [SerializeField] private GameEventId _eliFirstCaveRoomLoaded;
    [SerializeField] private AmbienceTrack _ambience;

    void Start() {
        if(!GameManager.obj.HasEvent(_eliFirstCaveRoomLoaded)) {
            StartCoroutine(LoadRoom());
        }
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            SceneFadeManager.obj.SetFadeInSpeed(5f);
        }
    }

    private IEnumerator LoadRoom() {
        Player.obj.SetCaveStartingCoordinates();
        Player.obj.gameObject.SetActive(true);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.CancelJumping();
        PlayerMovement.obj.spriteRenderer.flipX = false;
        Player.obj.SetAnimatorLayerAndHasCape(false);
        PlayerMovement.obj.Freeze();

        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, Player.obj.transform, Player.obj.transform.position);   

        CaveAvatar.obj.gameObject.SetActive(false);

        AmbienceManager.obj.Play(_ambience);
        yield return StartCoroutine(FadeInScreen());

        GameManager.obj.RegisterEvent(_eliFirstCaveRoomLoaded);

        PlayerStatsManager.obj.ResumeTimer();
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        yield return null;
    }

    private IEnumerator FadeInScreen() {
        yield return new WaitForSeconds(2f); //Give title screen time to unload
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(0.2f);
        SceneFadeManager.obj.StartFadeIn();
        yield return null;
    }
}
