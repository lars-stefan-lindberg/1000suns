using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            followCamera.Follow = Player.obj.transform;

            staticCameraGameObject.SetActive(true);
            CinemachineVirtualCamera staticCamera = staticCameraGameObject.GetComponent<CinemachineVirtualCamera>();
            staticCamera.enabled = true;
        }
    }
}
