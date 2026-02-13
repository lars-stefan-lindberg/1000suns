using System.Collections;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CapeRoomSwitchToFullRoomCam : MonoBehaviour
{
    [SerializeField] private GameEventId _customCameraZoomedOutEventId;
    [SerializeField] private GameObject _mainCamera;
    [SerializeField] private SpawnPoint _spawnPoint;
    [SerializeField] private EventReference _stinger;
    private bool _isTriggered = false;

    void Awake() {
        if(GameManager.obj.HasEvent(_customCameraZoomedOutEventId)) {
            _isTriggered = true;
            Destroy(gameObject, 3);
        }
    }

    private void OnTriggerExit2D(Collider2D collider) {
        if(collider.transform.CompareTag("Player") && !_isTriggered) {
            PlayerMovement.obj.Freeze();
            StartCoroutine(SwitchToFullRoomCamera());
            _isTriggered = true;
        }
    }

    private IEnumerator SwitchToFullRoomCamera() {
        yield return new WaitForSeconds(0.5f);
        RoomCameraController cameraController = _mainCamera.GetComponent<RoomCameraController>();
        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(), _spawnPoint.transform.position);

        SoundFXManager.obj.Play2D(_stinger);

        yield return new WaitForSeconds(4);
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.RegisterEvent(_customCameraZoomedOutEventId);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
