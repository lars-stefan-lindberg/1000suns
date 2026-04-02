using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Forest1CameraHandler : MonoBehaviour
{
    [SerializeField] private RoomMgr _roomMgr;
    [SerializeField] private GameObject _tentZoomedCamera;
    [SerializeField] private GameEventId _tentCutSceneCompleted;
    
    public void HandleCamera() {
        if(!GameManager.obj.HasEvent(_tentCutSceneCompleted)) {
            _tentZoomedCamera.SetActive(true);
            CinemachineVirtualCamera zoomedCamera = _tentZoomedCamera.GetComponent<CinemachineVirtualCamera>();
            zoomedCamera.enabled = true;
        } else {
            _roomMgr.ActivateMainCamera();
            _tentZoomedCamera.SetActive(false);
            CinemachineVirtualCamera zoomedCamera = _tentZoomedCamera.GetComponent<CinemachineVirtualCamera>();
            zoomedCamera.enabled = false;
        }
    }
}
