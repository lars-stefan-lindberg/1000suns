using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class C4CustomCameraHandling : MonoBehaviour
{
    public void HandleCamera() {
        GameObject[] sceneGameObjects = SceneManager.GetSceneByName("C4").GetRootGameObjects();
        GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
        CameraManager cameraManager = cameras.GetComponent<CameraManager>();
        if(GameEventManager.obj.CapePicked || GameEventManager.obj.CapeRoomZoomCompleted) {
            cameraManager.ActivateMainCamera();
        } else {
            cameraManager.ActivateCustomCamera();
        }
    }
}
