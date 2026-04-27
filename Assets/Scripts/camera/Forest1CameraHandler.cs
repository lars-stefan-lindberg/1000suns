using Cinemachine;
using UnityEngine;

public class Forest1CameraHandler : MonoBehaviour
{
    [SerializeField] private RoomMgr _roomMgr;
    [SerializeField] private GameObject _zoomedOutCamera;
    [SerializeField] private GameEventId _tentCutSceneCompleted;
    
    public void HandleCamera() {
        if(!GameManager.obj.HasEvent(_tentCutSceneCompleted)) {
            //Leave camera handling to FirstForestRoomLoader
            // _zoomedOutCamera.SetActive(true);
            // CinemachineVirtualCamera zoomedCamera = _zoomedOutCamera.GetComponent<CinemachineVirtualCamera>();
            // zoomedCamera.enabled = true;
        } else {
            _roomMgr.ActivateMainCamera();
            _zoomedOutCamera.SetActive(false);
            CinemachineVirtualCamera zoomedCamera = _zoomedOutCamera.GetComponent<CinemachineVirtualCamera>();
            zoomedCamera.enabled = false;
        }
    }
}
