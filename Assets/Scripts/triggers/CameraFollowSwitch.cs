using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraFollowSwitch : MonoBehaviour
{
    public GameObject cameraToDeactivate;
    public GameObject cameraToActivate;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            cameraToDeactivate.SetActive(false);
            CinemachineVirtualCamera vcamToDisable = cameraToDeactivate.GetComponent<CinemachineVirtualCamera>();
            vcamToDisable.enabled = false;

            cameraToActivate.SetActive(true);
            CinemachineVirtualCamera vcamToEnable = cameraToActivate.GetComponent<CinemachineVirtualCamera>();
            vcamToEnable.enabled = true;
        }
    }
}
