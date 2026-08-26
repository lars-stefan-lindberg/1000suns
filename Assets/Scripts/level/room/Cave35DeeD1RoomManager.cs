using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave35DeeD1RoomManager : MonoBehaviour
{
    [SerializeField] private SpawnPoint _deeSpawnPoint;
    [SerializeField] private AmbienceTrack _capeRoomAmbience;

    void Start() {
        ShadowTwinMovement.obj.isGrounded = true;
        ShadowTwinMovement.obj.SetStartingOnGround();
        if(ShadowTwinMovement.obj.IsFacingLeft())
            ShadowTwinMovement.obj.FlipPlayer();
        ShadowTwinPlayer.obj.transform.position = _deeSpawnPoint.transform.position;
        ShadowTwinMovement.obj.SetNewPower();
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
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.SHADOW_TWIN), _deeSpawnPoint.transform.position);
        yield return new WaitForSeconds(1f);
        SceneManager.SetActiveScene(gameObject.scene);

        //All loading should be completed. Start fading in room
        WhiteSceneFadeManager.obj.StartFadeIn(0.5f);
        yield return new WaitForSeconds(3f);
        ShadowTwinMovement.obj.SetNewPowerReceived();
        yield return new WaitForSeconds(2);
        ShadowTwinMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
    }
}
