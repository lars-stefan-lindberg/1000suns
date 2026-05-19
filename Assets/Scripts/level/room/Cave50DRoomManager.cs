using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave50DRoomManager : MonoBehaviour
{
    [SerializeField] private SpawnPoint _blobSpawnPoint;
    [SerializeField] private AmbienceTrack _capeRoomAmbience;
    [SerializeField] private EventReference _introStinger;
    
    void Start()
    {
        SceneManager.SetActiveScene(gameObject.scene);
        PlayerBlobMovement.obj.Freeze();
        PlayerBlobMovement.obj.isGrounded = true;
        PlayerBlobMovement.obj.SetStartingOnGround();
        PlayerBlob.obj.transform.position = _blobSpawnPoint.transform.position - new Vector3(0, 0.5f, 0);
        PlayerBlobMovement.obj.FlipPlayer();
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
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.HUMAN), _blobSpawnPoint.transform.position);
        yield return new WaitForSeconds(1f);
        SceneManager.SetActiveScene(gameObject.scene);

        //All loading should be completed. Start fading in room
        SceneFadeManager.obj.StartFadeIn(0.5f);
        SoundFXManager.obj.Play2D(_introStinger);
        yield return new WaitForSeconds(2f);
        PlayerBlobMovement.obj.UnFreeze();
    }
}
