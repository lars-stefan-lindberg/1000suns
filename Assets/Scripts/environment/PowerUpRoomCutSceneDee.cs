using System.Collections;
using Cinemachine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PowerUpRoomCutSceneDee : MonoBehaviour, ISkippable
{
    private Animator _animator;

    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private GameObject _skipCutsceneCamera;
    [SerializeField] private EventReference _receivePowerupStinger;
    [SerializeField] private EventReference _powerupFanfareStinger;
    [SerializeField] private EventReference _pickupPowerupSfx;
    [SerializeField] private GameEventId _shadowLashReceived;
    [SerializeField] private PowerUpScreen _powerUpScreen;
    [SerializeField] private EventReference _teleportSfx;
    [SerializeField] private SceneField _dreamRoomScene;
    [SerializeField] private SceneField _secondDreamRoomScene;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private AmbienceTrack _caveMain;

    private bool _isPicked = false;
    private bool _playerEntered = false;
    private bool _isSpawned = false;
    private bool _cutsceneFinished = false;
    private EventInstance _receivePowerupStingerInstance;
    private EventInstance _pickupPowerupSfxInstance;
    private Coroutine _cutsceneCoroutine;
    
    void Start() {
        _animator = GetComponent<Animator>();
        if(GameManager.obj.HasEvent(_shadowLashReceived)) {
            Destroy(gameObject);
            return;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            if(!_cutsceneFinished)
                _cutsceneCoroutine = StartCoroutine(StartCutscene());
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
        PauseMenuManager.obj.RegisterSkippable(this);
        ShadowTwinMovement.obj.Freeze();

        yield return new WaitForSeconds(1);
        //Zoom in on power up
        _zoomedCamera.SetActive(true);
        CinemachineVirtualCamera zoomedCameraVcam = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedCameraVcam.enabled = true;

        ShadowTwinPlayer.obj.transform.position = new Vector2(1453.75f, ShadowTwinPlayer.obj.transform.position.y);
        ShadowTwinMovement.obj.SetNewPower();
        yield return new WaitForSeconds(1.5f);

        _receivePowerupStingerInstance = SoundFXManager.obj.CreateAttachedInstance(_receivePowerupStinger, gameObject);
        _receivePowerupStingerInstance.start();
        _receivePowerupStingerInstance.release();

        _animator.SetTrigger("enableFast");
        ShadowTwinPlayer.obj.FlashFor(5f);

        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 4.9f);

        yield return new WaitForSeconds(5f);
        
        _animator.SetTrigger("disableFast");
        SetIsPicked();
        
        yield return new WaitForSeconds(1);

        //Zoom out
        zoomedCameraVcam.enabled = false;
        
        yield return new WaitForSeconds(2.5f);

        PauseMenuManager.obj.UnregisterSkippable();

        StartCoroutine(ShowPowerUpScreen());

        yield return null;
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null) {
            StopCoroutine(_cutsceneCoroutine);
        }

        CameraShakeManager.obj.ShakeCamera(0, 0, 0);

        AudioUtils.SafeStop(ref _receivePowerupStingerInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        AudioUtils.SafeStop(ref _pickupPowerupSfxInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);

        ShadowTwinPlayer.obj.transform.position = new Vector2(1453.75f, ShadowTwinPlayer.obj.transform.position.y);
        ShadowTwinMovement.obj.SetNewPower();
        ShadowTwinPlayer.obj.AbortFlash();

        _isPicked = false;
        _animator.SetBool("isPicked", true);
        _animator.Play("idle_picked", 0, 0);

        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ShowPowerUpScreen() {
        GameManager.obj.IsPauseAllowed = false;
        Time.timeScale = 0;
        _powerUpScreen.Show();
        SoundFXManager.obj.Play2D(_powerupFanfareStinger);
        while(!_powerUpScreen.PowerUpScreenCompleted) {
            yield return null;
        }
        Time.timeScale = 1;

        //PlayerMovement.obj.SetNewPowerReceived();
        PlayerPowersManager.obj.DeeCanShadowLash = true;

        ShadowTwinMovement.obj.SetNewPowerRecevied();
        yield return new WaitForSeconds(2f);
        AmbienceManager.obj.Play(_caveMain);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        ShadowTwinMovement.obj.UnFreeze();

        //yield return new WaitForSeconds(1f);
        //Teleport to dream room
        //SoundFXManager.obj.Play2D(_teleportSfx);
        //StartCoroutine(TeleportToDreamRoomRoutine());

        _cutsceneFinished = true;
    }

    private IEnumerator ResumeGameplay() {
        yield return null;
        _skipCutsceneCamera.SetActive(true);
        _zoomedCamera.SetActive(false);
        CinemachineVirtualCamera zoomedCameraVcam = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedCameraVcam.enabled = false;
        yield return null;
        _skipCutsceneCamera.SetActive(false);
        yield return null;

        yield return new WaitForSeconds(0.2f);
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
        StartCoroutine(ShowPowerUpScreen());
        yield return null;
    }

    private IEnumerator TeleportToDreamRoomRoutine() {
        WhiteSceneFadeManager.obj.StartFadeOut(0.5f);

        while(WhiteSceneFadeManager.obj.IsFadingOut)
            yield return null;

        AmbienceManager.obj.Stop();

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
        _pickupPowerupSfxInstance = SoundFXManager.obj.CreateAttachedInstance(_pickupPowerupSfx, gameObject);
        _pickupPowerupSfxInstance.start();
        _pickupPowerupSfxInstance.release();
        _animator.SetBool("isPicked", true);
        _isPicked = true;
    }
}
