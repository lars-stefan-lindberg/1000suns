using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FunkyCode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave42RoomManager : MonoBehaviour
{
    [SerializeField] private SpawnPoint _eliStartPosition;
    [SerializeField] private SpawnPoint _afterElevatorSpawnPoint;
    [SerializeField] private Transform _sootStartPosition;
    [SerializeField] private Transform _elevatorStopPosition;
    [SerializeField] private CaveElevator _elevator;
    [SerializeField] private GameEventId _elevatorCompleted;
    [SerializeField] private MusicTrack _caveMain;

    void Start()
    {
        if(GameManager.obj.HasEvent(_elevatorCompleted)) {
            _elevator.transform.position = new Vector2(_elevator.transform.position.x, _elevatorStopPosition.position.y);
            _elevator.GetComponentInChildren<LightSprite2D>().enabled = true;
            return;
        }
        SceneFadeManager.obj.SetFadedOutState();
        PlayerMovement.obj.isOnMoveable = true;
        PlayerMovement.obj.moveableRigidbody = _elevator.GetComponent<Rigidbody2D>();
        Player.obj.transform.position = _eliStartPosition.transform.position;
        Player.obj.gameObject.SetActive(true);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.Freeze();

        CaveAvatar.obj.SetPosition(_sootStartPosition.position);
        CaveAvatar.obj.IsFollowingPlayer = true;

        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.HUMAN), _eliStartPosition.transform.position);

        SceneManager.SetActiveScene(gameObject.scene);

        StartCoroutine(StartScene());
    }

    private IEnumerator StartScene() {
        //Give some time to transition from previous scene
        yield return new WaitForSeconds(1f);
        _elevator.SetStopPosition(_elevatorStopPosition.position.y);
        _elevator.StartMoving();
        _elevator.GetComponentInChildren<LightSprite2D>().enabled = true;

        yield return new WaitForSeconds(2f);

        SceneFadeManager.obj.StartFadeIn(0.8f);
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;

        MusicManager.obj.Play(_caveMain);

        while(!_elevator.HasReachedStop())
            yield return null;

        yield return new WaitForSeconds(1f);
        GameManager.obj.RegisterEvent(_elevatorCompleted);
        GameManager.obj.SetCurrentSpawnPointId(_afterElevatorSpawnPoint.SpawnPointID);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        PlayerMovement.obj.UnFreeze();
    }
}
