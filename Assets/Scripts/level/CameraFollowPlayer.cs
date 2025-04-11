using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    private CinemachineVirtualCamera _followCamera;

    void Awake()
    {
        _followCamera = GetComponent<CinemachineVirtualCamera>();
    }

    void FixedUpdate() {
        _followCamera.Follow = PlayerManager.obj.GetPlayerTransform();
    }
}
