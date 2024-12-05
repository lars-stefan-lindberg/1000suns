using System.Collections;
using Cinemachine;
using UnityEngine;

public class PowerUpRoomCutScene : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private CinemachineVirtualCamera _defaultCamera;
    [SerializeField] private GameObject _zoomedCamera;

    [SerializeField] private float _recoveryTime = 10;
    private float _recoveryTimer = 0;

    private bool _isPicked = false;
    private bool _playerEntered = false;
    private bool _isSpawned = false;
    private bool _cutsceneFinished = false;

    void Awake() {
        _animator = GetComponent<Animator>();
    }
    
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            if(!_cutsceneFinished)
                StartCoroutine(StartCutscene());
            else
                _playerEntered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            _playerEntered = false;
        }
    }

    private IEnumerator StartCutscene() {
        //Zoom in on power up
        _zoomedCamera.SetActive(true);
        CinemachineVirtualCamera zoomedCameraVcam = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedCameraVcam.enabled = true;
        _defaultCamera.enabled = false;

        yield return new WaitForSeconds(3);

        //Zoom out
        _defaultCamera.enabled = true;
        zoomedCameraVcam.enabled = false;

        _cutsceneFinished = true;
        GameEventManager.obj.FirstPowerUpPicked = true;

        yield return null;
    }

    void FixedUpdate() {
        if(_isSpawned && _playerEntered && !_isPicked && !Player.obj.hasPowerUp) {
            Player.obj.SetHasPowerUp(true);
            _animator.SetBool("isPicked", true);
            _isPicked = true;
        }
        if(_isPicked) {
            _recoveryTimer += Time.deltaTime;
            if(_recoveryTimer >= _recoveryTime) {
                _animator.SetBool("isPicked", false);
            }
        }
    }

    public void SetRecovered() {
        _isPicked = false;
        _isSpawned = true;
        _recoveryTimer = 0;
    }
}
