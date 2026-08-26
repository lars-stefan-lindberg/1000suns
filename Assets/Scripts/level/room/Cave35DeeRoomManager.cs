using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave35DeeRoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _hasShadowLash;
    [SerializeField] private GameEventId _postDreamSequenceCompleted;
    [SerializeField] private SpawnPoint _deeReturnFromDreamRoomPosition;
    [SerializeField] private AmbienceTrack _caveMain;
    [SerializeField] private GameObject _bgBlobs;

    void Start()
    {
        //If coming back from dream room, load room state
        if(GameManager.obj.HasEvent(_hasShadowLash) && !GameManager.obj.HasEvent(_postDreamSequenceCompleted)) {
            StartCoroutine(AfterDeeDreamRoom());
        }
        if(GameManager.obj.HasEvent(_hasShadowLash))
            _bgBlobs.SetActive(false);
    }

    private IEnumerator AfterDeeDreamRoom() {
        ShadowTwinMovement.obj.isGrounded = true;
        ShadowTwinMovement.obj.SetStartingOnGround();
        ShadowTwinPlayer.obj.transform.position = _deeReturnFromDreamRoomPosition.transform.position;
        ShadowTwinMovement.obj.SetNewPower();
        if(!ShadowTwinMovement.obj.IsFacingLeft())
            ShadowTwinMovement.obj.FlipPlayer();

        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.SHADOW_TWIN), _deeReturnFromDreamRoomPosition.transform.position);

        SceneManager.SetActiveScene(gameObject.scene);

        //Give things some time to properly load
        yield return new WaitForSeconds(1f);

        WhiteSceneFadeManager.obj.StartFadeIn(0.5f);
        while(WhiteSceneFadeManager.obj.IsFadingIn)
            yield return null;

        ShadowTwinMovement.obj.SetNewPowerReceived();
        AmbienceManager.obj.Play(_caveMain);
        yield return new WaitForSeconds(2);

        GameManager.obj.SetCurrentSpawnPointId(_deeReturnFromDreamRoomPosition.SpawnPointID);
        GameManager.obj.RegisterEvent(_postDreamSequenceCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        GameManager.obj.IsPauseAllowed = true;

        ShadowTwinMovement.obj.UnFreeze();
    }
}
