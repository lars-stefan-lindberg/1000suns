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
        yield return StartCoroutine(StartScene());

        

        yield return null;
    }

    private IEnumerator StartScene() {
        Player.obj.gameObject.GetComponent<EliAudio>().PlayLongFall();
        yield return new WaitForSeconds(2.5f);
        Player.obj.gameObject.GetComponent<EliAudio>().PlayHeavyLand();
        yield return new WaitForSeconds(2f); //Give title screen time to unload
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(0.2f);
        SceneFadeManager.obj.StartFadeIn();

        Player.obj.PlayGetUpAnimation();
        yield return new WaitForSeconds(4);
        Player.obj.StartAnimator();
        yield return new WaitForSeconds(5);

        GameManager.obj.RegisterEvent(_eliFirstCaveRoomLoaded);

        PlayerMovement.obj.UnFreeze();
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        GameManager.obj.IsPauseAllowed = true;

        yield return null;
    }
}
