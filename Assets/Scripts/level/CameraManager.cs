using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject _mainCamera;

    public void ActivateMainCamera() {
        _mainCamera.SetActive(true);
        CinemachineVirtualCamera cinemachineVirtualCamera = _mainCamera.GetComponent<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.enabled = true;
    }
}
