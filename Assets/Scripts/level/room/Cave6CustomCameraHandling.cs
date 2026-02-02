using System.Linq;
using Cinemachine;
using UnityEngine;

public class Cave6CustomCameraHandling : MonoBehaviour
{
    [SerializeField] private GameEventId _customCameraZoomedOutEventId;
    [SerializeField] private GameObject _customCamera;
    [SerializeField] private SpawnPoint _spawnPoint;
    public void HandleCamera() {
        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        if(GameManager.obj.HasEvent(_customCameraZoomedOutEventId)) {
            GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
            RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
            CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(), _spawnPoint.transform.position);
        } else {
            RoomCameraController cameraController = _customCamera.GetComponent<RoomCameraController>();
            CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(), _spawnPoint.transform.position);
        }
    }
}
