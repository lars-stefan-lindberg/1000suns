using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave64RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject _camera;

    public void ActivateCamera() {
        _camera.SetActive(true);
    }

    public void DeactivateCamera() {
        _camera.SetActive(false);
    }
}
