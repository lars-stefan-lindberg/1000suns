using System.Collections;
using Cinemachine;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PowerUpRoomCutScene : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private EventReference _receivePowerupStinger;
    [SerializeField] private EventReference _powerUpFanfareStinger;
    [SerializeField] private EventReference _pickupPowerupSfx;
    [SerializeField] private GameEventId _shadowJumpReceived;

    private bool _isPicked = false;
    private bool _playerEntered = false;
    private bool _isSpawned = false;
    private bool _cutsceneFinished = false;

    void Awake() {
        _animator = GetComponent<Animator>();
        if(GameManager.obj.HasEvent(_shadowJumpReceived)) {
            _cutsceneFinished = true;
            //TODO: handle if shadow jump has been recevied
        }
    }
    
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            if(!_cutsceneFinished)
                StartCoroutine(StartCutscene());
            else {
                _playerEntered = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player"))
        {
            _playerEntered = false;
        }
    }

    private IEnumerator StartCutscene() {
        PlayerMovement.obj.Freeze();

        yield return new WaitForSeconds(1);
        //Zoom in on power up
        _zoomedCamera.SetActive(true);
        CinemachineVirtualCamera zoomedCameraVcam = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedCameraVcam.enabled = true;

        Player.obj.transform.position = new Vector2(1306.25f, Player.obj.transform.position.y);
        PlayerMovement.obj.SetNewPower();
        yield return new WaitForSeconds(1.5f);
        SoundFXManager.obj.Play2D(_receivePowerupStinger);
        _animator.SetTrigger("enableFast");
        Player.obj.FlashFor(5f);

        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 4.9f);

        yield return new WaitForSeconds(5f);
        //Player.obj.SetBlackCape();
        //yield return new WaitForSeconds(2.5f);
        
        _animator.SetTrigger("disableFast");
        SetIsPicked();

        yield return new WaitForSeconds(1);

        //Zoom out
        zoomedCameraVcam.enabled = false;
        
        yield return new WaitForSeconds(2f);

        //Time.timeScale = 0;
        //_tutorialCanvas.SetActive(true);
        // TutorialDialogManager.obj.StartFadeIn();
        // SoundFXManager.obj.Play2D(_powerUpFanfareStinger);
        // while(!TutorialDialogManager.obj.tutorialCompleted) {
        //     yield return null;
        // }
        //_tutorialCanvas.SetActive(false);
        //Time.timeScale = 1;

        PlayerMovement.obj.SetNewPowerRecevied();

        yield return new WaitForSeconds(2f);
        GameManager.obj.RegisterEvent(_shadowJumpReceived);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        PlayerMovement.obj.UnFreeze();

        _cutsceneFinished = true;

        yield return null;
    }

    void FixedUpdate() {
        if(_isSpawned && _playerEntered && !_isPicked && !Player.obj.hasPowerUp) {
            SetIsPicked();
        }
    }

    private void SetIsPicked() {
        SoundFXManager.obj.Play2D(_pickupPowerupSfx);
        _animator.SetBool("isPicked", true);
        _isPicked = true;
    }
}
