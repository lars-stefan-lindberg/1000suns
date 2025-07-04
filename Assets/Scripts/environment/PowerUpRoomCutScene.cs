using System.Collections;
using Cinemachine;
using UnityEngine;

public class PowerUpRoomCutScene : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private CinemachineVirtualCamera _defaultCamera;
    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private GameObject _tutorialCanvas;

    [SerializeField] private float _recoveryTime = 10;
    private float _recoveryTimer = 0;

    private bool _isPicked = false;
    private bool _playerEntered = false;
    private bool _isSpawned = false;
    private bool _cutsceneFinished = false;
    private bool _recoveryTriggered = false;

    void Awake() {
        _animator = GetComponent<Animator>();
        if(GameEventManager.obj.FirstPowerUpPicked) {
            _cutsceneFinished = true;
        }
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
        GameEventManager.obj.IsPauseAllowed = false;
        PlayerMovement.obj.Freeze();

        yield return new WaitForSeconds(1);
        //Zoom in on power up
        _zoomedCamera.SetActive(true);
        CinemachineVirtualCamera zoomedCameraVcam = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedCameraVcam.enabled = true;
        _defaultCamera.enabled = false;

        Player.obj.transform.position = new Vector2(1058.75f, Player.obj.transform.position.y);
        PlayerMovement.obj.SetNewPower();
        MusicManager.obj.PlayPowerUpPickupSong();
        yield return new WaitForSeconds(1f);
        _animator.SetTrigger("enableFast");
        Player.obj.FlashFor(4.5f);

        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 4.9f);

        yield return new WaitForSeconds(2.5f);
        Player.obj.SetBlackCape();
        yield return new WaitForSeconds(2.5f);

        GameEventManager.obj.FirstPowerUpPicked = true;
        
        _animator.SetTrigger("disableFast");
        SetIsPicked();

        yield return new WaitForSeconds(1);

        //Zoom out
        _defaultCamera.enabled = true;
        zoomedCameraVcam.enabled = false;
        
        yield return new WaitForSeconds(2f);

        Time.timeScale = 0;
        _tutorialCanvas.SetActive(true);
        TutorialDialogManager.obj.StartFadeIn();
        SoundFXManager.obj.PlayPowerUpDialogueStinger();
        while(!TutorialDialogManager.obj.tutorialCompleted) {
            yield return null;
        }
        _tutorialCanvas.SetActive(false);
        Time.timeScale = 1;

        PlayerMovement.obj.SetNewPowerRecevied();

        yield return new WaitForSeconds(2f);
        PlayerMovement.obj.UnFreeze();
        GameEventManager.obj.IsPauseAllowed = true;

        _cutsceneFinished = true;

        yield return null;
    }

    void FixedUpdate() {
        if(_isSpawned && _playerEntered && !_isPicked && !Player.obj.hasPowerUp) {
            SetIsPicked();
        }
        if(_isPicked) {
            if(_recoveryTriggered) {
                _recoveryTimer += Time.deltaTime;
                if(_recoveryTimer >= _recoveryTime) {
                    _animator.SetBool("isPicked", false);
                }
            }
        }
    }

    private void SetIsPicked() {
        SoundFXManager.obj.PlayPlayerPickupCavePowerup(transform);
        Player.obj.SetHasPowerUp(true);
        _animator.SetBool("isPicked", true);
        _isPicked = true;
    }

    public void SetFirstPowerUpRecovered() {
        _recoveryTriggered = true;
    }

    public void SetRecovered() {
        _isPicked = false;
        _isSpawned = true;
        _recoveryTimer = 0;
    }
}
