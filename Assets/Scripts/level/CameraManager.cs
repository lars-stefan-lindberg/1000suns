using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject _mainCamera;
    [SerializeField] private GameObject _alternativeCamera;

    public void ActivateMainCamera(PlayerManager.PlayerDirection direction)
    {
        //TODO, change this solution to be more flexible
        //Shitty solution for room 16 where another camera is used if player enters the "exit" of the room
        if (direction == PlayerManager.PlayerDirection.LEFT && _alternativeCamera != null)
            ActivateCamera(_alternativeCamera);
        else {
            ActivateCamera(_mainCamera);
            DeactivateCamera(_alternativeCamera);
        }
    }

    public void ActivateAlternativeCamera() {
        if(_alternativeCamera == null) {
            Debug.Log("No alternative camera found when trying to activate it.");
            ActivateCamera(_mainCamera);
            return;
        }
        ActivateCamera(_alternativeCamera);
        DeactivateCamera(_mainCamera);
    }

    private void ActivateCamera(GameObject camera)
    {
        camera.SetActive(true);
        CinemachineVirtualCamera cinemachineVirtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.enabled = true;
    }

    private void DeactivateCamera(GameObject camera) {
        camera.SetActive(false);
        CinemachineVirtualCamera cinemachineVirtualCamera = camera.GetComponent<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.enabled = false;
    }
}
