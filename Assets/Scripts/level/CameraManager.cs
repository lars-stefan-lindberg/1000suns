using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject _mainCamera;
    [SerializeField] private GameObject _alternativeCamera;
    [SerializeField] private GameObject _customCamera;

    public GameObject ActivateMainCamera()
    {
        ActivateCamera(_mainCamera);
        if(_alternativeCamera != null)
        DeactivateCamera(_alternativeCamera);
        return _mainCamera;
    }

    public void ActivateCustomCamera()
    {
        ActivateCamera(_customCamera);
    }

    public GameObject ActivateAlternativeCamera() {
        if(_alternativeCamera == null) {
            Debug.Log("No alternative camera found when trying to activate it.");
            ActivateCamera(_mainCamera);
            return _mainCamera;
        }
        ActivateCamera(_alternativeCamera);
        DeactivateCamera(_mainCamera);
        return _alternativeCamera;
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
