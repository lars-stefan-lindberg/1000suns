using System.Collections;
using Cinemachine;
using UnityEngine;

public class C1CameraZoomTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _zoomedCam;

    private BoxCollider2D _collider;

    private void Awake() {
        _collider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            _collider.enabled = false;
            StartCoroutine(HandleCameras());
        }
    }

    private IEnumerator HandleCameras() {
        PlayerMovement.obj.Freeze();
        GameManager.obj.IsPauseAllowed = false;

        _zoomedCam.SetActive(true);
        CinemachineVirtualCamera zoomedCamera = _zoomedCam.GetComponent<CinemachineVirtualCamera>();
        zoomedCamera.enabled = true;

        yield return new WaitForSeconds(5f);

        _zoomedCam.SetActive(false);
        zoomedCamera.enabled = false;

        yield return new WaitForSeconds(2f);

        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;

        yield return null;
    }
}
