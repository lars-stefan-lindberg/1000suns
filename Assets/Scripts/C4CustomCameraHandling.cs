using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class C4CustomCameraHandling : MonoBehaviour
{
    public void HandleCamera() {
        GameObject[] sceneGameObjects = SceneManager.GetSceneByName("C4").GetRootGameObjects();
        GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
        RoomCameraManager cameraManager = cameras.GetComponent<RoomCameraManager>();
        if(GameManager.obj.CapePicked || GameManager.obj.CapeRoomZoomCompleted) {
            cameraManager.ActivateMainCamera();
        } else {
            cameraManager.ActivateCustomCamera();
        }
    }
}
