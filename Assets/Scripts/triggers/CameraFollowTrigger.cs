using UnityEngine;
using Cinemachine;

public class CameraFollowTrigger : MonoBehaviour
{
    public GameObject cameraFollowGameObject;
    public GameObject staticCameraGameObject;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            cameraFollowGameObject.SetActive(false);
            CinemachineVirtualCamera followCamera = cameraFollowGameObject.GetComponent<CinemachineVirtualCamera>();
            followCamera.enabled = false;

            staticCameraGameObject.SetActive(true);
            CinemachineVirtualCamera staticCamera = staticCameraGameObject.GetComponent<CinemachineVirtualCamera>();
            staticCamera.enabled = true;
        }
    }
}
