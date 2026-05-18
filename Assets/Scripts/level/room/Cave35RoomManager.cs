using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave35RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _hasShadowJump;
    [SerializeField] private GameEventId _postDreamSequenceCompleted;
    [SerializeField] private SpawnPoint _eliReturnFromDreamRoomPosition;

    void Start()
    {
        //If coming back from dream room, load room state
        if(GameManager.obj.HasEvent(_hasShadowJump) && !GameManager.obj.HasEvent(_postDreamSequenceCompleted)) {
            StartCoroutine(AfterEliDreamRoom());
        }
    }

    private IEnumerator AfterEliDreamRoom() {
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.SetStartingOnGround();
        Player.obj.transform.position = _eliReturnFromDreamRoomPosition.transform.position;
        PlayerMovement.obj.SetNewPower();
        if(PlayerMovement.obj.IsFacingLeft())
            PlayerMovement.obj.FlipPlayer();

        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.HUMAN), _eliReturnFromDreamRoomPosition.transform.position);

        SceneManager.SetActiveScene(gameObject.scene);

        //Give things some time to properly load
        yield return new WaitForSeconds(1f);

        SceneFadeManager.obj.StartFadeIn(0.5f);
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;


        PlayerMovement.obj.SetNewPowerReceived();
        yield return new WaitForSeconds(2);

        GameManager.obj.RegisterEvent(_postDreamSequenceCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        PlayerMovement.obj.UnFreeze();
    }
}
