using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraInitializer : MonoBehaviour
{
    void Start() {
        CinemachineVirtualCamera cam = GetComponent<CinemachineVirtualCamera>();
        cam.Follow = Player.obj.transform;
    }
}
