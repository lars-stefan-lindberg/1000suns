using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave35DRoomManager : MonoBehaviour
{
    [SerializeField] private SpawnPoint _eliSpawnPoint;
    [SerializeField] private AmbienceTrack _capeRoomAmbience;

    void Start() {
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.FlipPlayer();
        Player.obj.transform.position = _eliSpawnPoint.transform.position;
        PlayerMovement.obj.SetNewPower();
        DustParticleMgr.obj.Enabled = false;
        AmbienceManager.obj.Play(_capeRoomAmbience);
        StartCoroutine(TransitionIntoRoom());
    }

    private IEnumerator TransitionIntoRoom() {
        //Set camera
        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.HUMAN), _eliSpawnPoint.transform.position);
        yield return new WaitForSeconds(1f);
        SceneManager.SetActiveScene(gameObject.scene);

        //All loading should be completed. Start fading in room
        SceneFadeManager.obj.StartFadeIn(0.5f);
        yield return new WaitForSeconds(3f);
        PlayerMovement.obj.SetNewPowerReceived();
        yield return new WaitForSeconds(2);
        PlayerMovement.obj.UnFreeze();
    }
}
