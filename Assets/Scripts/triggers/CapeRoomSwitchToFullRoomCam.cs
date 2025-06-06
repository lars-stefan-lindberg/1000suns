using System.Collections;
using Cinemachine;
using UnityEngine;

public class CapeRoomSwitchToFullRoomCam : MonoBehaviour
{
    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private GameObject _fullRoomCamera;
    private bool _isTriggered = false;

    void Awake() {
        if(GameEventManager.obj.CapePicked) {
            _isTriggered = true;
            Destroy(gameObject, 3);
        }
    }

    private void OnTriggerExit2D(Collider2D collider) {
        if(collider.transform.CompareTag("Player") && !_isTriggered) {
            PlayerMovement.obj.Freeze();
            GameEventManager.obj.IsPauseAllowed = false;
            StartCoroutine(SwitchToFullRoomCamera());
            _isTriggered = true;
        }
    }

    private IEnumerator SwitchToFullRoomCamera() {
        yield return new WaitForSeconds(0.5f);
        _fullRoomCamera.SetActive(true);
        CinemachineVirtualCamera fullRoomCamera = _fullRoomCamera.GetComponent<CinemachineVirtualCamera>();
        CinemachineVirtualCamera zoomedCamera = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        fullRoomCamera.enabled = true;
        zoomedCamera.enabled = false;
        _zoomedCamera.SetActive(false);

        MusicManager.obj.PlayPowerUpIntroSong();

        yield return new WaitForSeconds(5);
        PlayerMovement.obj.UnFreeze();
        GameEventManager.obj.IsPauseAllowed = true;
    }
}
