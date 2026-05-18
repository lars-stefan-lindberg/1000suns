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
    [SerializeField] private EventReference _powerupFanfareStinger;
    [SerializeField] private EventReference _pickupPowerupSfx;
    [SerializeField] private GameEventId _shadowJumpReceived;
    [SerializeField] private PowerUpScreen _powerUpScreen;
    [SerializeField] private EventReference _teleportSfx;
    [SerializeField] private SceneField _dreamRoomScene;
    [SerializeField] private SceneField _secondDreamRoomScene;
    [SerializeField] private SceneField _thisScene;

    private bool _isPicked = false;
    private bool _playerEntered = false;
    private bool _isSpawned = false;
    private bool _cutsceneFinished = false;

    void Start() {
        _animator = GetComponent<Animator>();
        if(GameManager.obj.HasEvent(_shadowJumpReceived)) {
            Destroy(gameObject);
            return;
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
        GameManager.obj.RegisterEvent(_shadowJumpReceived);

        yield return new WaitForSeconds(1);

        //Zoom out
        zoomedCameraVcam.enabled = false;
        
        yield return new WaitForSeconds(2.5f);

        GameManager.obj.IsPauseAllowed = false;
        Time.timeScale = 0;
        _powerUpScreen.Show();
        SoundFXManager.obj.Play2D(_powerupFanfareStinger);
        while(!_powerUpScreen.PowerUpScreenCompleted) {
            yield return null;
        }
        Time.timeScale = 1;
        GameManager.obj.IsPauseAllowed = true;

        //PlayerMovement.obj.SetNewPowerReceived();
        PlayerPowersManager.obj.EliCanShadowJump = true;

        // yield return new WaitForSeconds(2f);
        // SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        // PlayerMovement.obj.UnFreeze();

        yield return new WaitForSeconds(1f);
        //Teleport to dream room
        SoundFXManager.obj.Play2D(_teleportSfx);
        StartCoroutine(TeleportToDreamRoomRoutine());

        _cutsceneFinished = true;

        yield return null;
    }

    private IEnumerator TeleportToDreamRoomRoutine() {
        SceneFadeManager.obj.StartWhiteFadeOut(0.5f);

        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        //Load dream room
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_dreamRoomScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone) {
            yield return null;
        }
        AsyncOperation asyncOperation2 = SceneManager.LoadSceneAsync(_secondDreamRoomScene, LoadSceneMode.Additive);
        while(!asyncOperation2.isDone) {
            yield return null;
        }

        //Give some time for dream room to load until unloading current room
        yield return new WaitForSeconds(2f);

        //Unload current room
        SceneManager.UnloadSceneAsync(_thisScene.SceneName);
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
