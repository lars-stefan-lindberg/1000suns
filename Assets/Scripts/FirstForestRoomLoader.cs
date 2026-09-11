using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstForestRoomLoader : MonoBehaviour
{
    [SerializeField] private GameEventId _eliFirstForestRoomLoaded;
    [SerializeField] private GameEventId _tentCutsceneCompleted;
    [SerializeField] private GameEventId _introSkipped;
    [SerializeField] private GameEventId _firstForestRoomLoadedAfterIntro;
    [SerializeField] private AmbienceTrack _ambience;
    [SerializeField] private TentCutsceneManager _tentCutsceneManager;
    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private GameObject _zoomedOutCamera;
    [SerializeField] private GameObject _zoomedOutBackgroundObjects;
    [SerializeField] private Forest1LoadObjectsManager _loadObjectsManager;
    [SerializeField] private SceneField _firstForestSurfaces;
    [SerializeField] private SceneField _forestBackgroundScene;
    [SerializeField] private ForestBird _firstBird;
    [SerializeField] private ForestBird _secondBird;
    [SerializeField] private PlayForestWaterfallSfx _waterfallSfx;

    void Start() {
        if(!GameManager.obj.HasEvent(_eliFirstForestRoomLoaded)) {
            StartCoroutine(LoadRoom());
        } else if(GameManager.obj.HasEvent(_tentCutsceneCompleted)) {
            _zoomedOutCamera.SetActive(false);
            _zoomedOutBackgroundObjects.SetActive(false);
        }

        if(GameManager.obj.HasEvent(_introSkipped) && !GameManager.obj.HasEvent(_firstForestRoomLoadedAfterIntro)) {
            StartCoroutine(LoadRoomAfterSkippedIntro());
        }
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            SceneFadeManager.obj.SetFadeInSpeed(5f);
        }
    }

    private IEnumerator LoadRoomAfterSkippedIntro() {
        StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(_firstForestSurfaces));
        yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground(_forestBackgroundScene));
        
        InitRoom initRoomData = LevelManager.obj.GetInitRoomData(gameObject.scene);
        LevelManager.obj.LoadAdjacentRooms(initRoomData);
        SceneManager.SetActiveScene(gameObject.scene);

        _zoomedOutCamera.SetActive(false);
        _zoomedOutBackgroundObjects.SetActive(false);

        AudioStateManager.obj.PauseSfx();  //Avoid any sounds before fading in
        Player.obj.SetForestStartingCoordinates();
        Player.obj.gameObject.SetActive(true);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.CancelJumping();
        PlayerMovement.obj.spriteRenderer.flipX = false;
        Player.obj.SetAnimatorLayerAndHasCape(false);
        PlayerMovement.obj.Freeze();

        CaveAvatar.obj.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);
        AudioStateManager.obj.StopSfxEvents();
        AudioStateManager.obj.RestoreSfx();

        AmbienceManager.obj.Play(_ambience);
        _waterfallSfx.StartAmbienceAfterIntro();
        
        _firstBird.Mute();
        _secondBird.Mute();
        _loadObjectsManager.LoadIntroObjects();

        IntroEvents.InvokeFirstForestRoomReady();

        GameManager.obj.RegisterEvent(_firstForestRoomLoadedAfterIntro);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        yield return new WaitForSeconds(1f);

        _firstBird.UnMute();
        _secondBird.UnMute();

        yield return null;
    }

    private IEnumerator LoadRoom() {
        StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(_firstForestSurfaces));

        InitRoom initRoomData = LevelManager.obj.GetInitRoomData(gameObject.scene);
        LevelManager.obj.LoadAdjacentRooms(initRoomData);
        SceneManager.SetActiveScene(gameObject.scene);
        _loadObjectsManager.LoadIntroObjects();

        _zoomedOutCamera.SetActive(true);
        CinemachineVirtualCamera zoomedOutCamera = _zoomedOutCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedOutCamera.enabled = true;
        _zoomedOutBackgroundObjects.SetActive(true);

        IntroEvents.InvokeFirstForestRoomReady();

        yield return new WaitForSeconds(0.2f);  //Time to load forest 2 and scene objects
        SceneFadeManager.obj.StartFadeIn(1f);
        yield return new WaitForSeconds(0.1f);
        
        _zoomedCamera.SetActive(true);
        CinemachineVirtualCamera zoomedCamera = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedCamera.enabled = true;

        _zoomedOutCamera.SetActive(false);
        zoomedOutCamera = _zoomedOutCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedOutCamera.enabled = false;

        Player.obj.SetForestStartingCoordinates();
        PlayerMovement.obj.spriteRenderer.enabled = false;
        Player.obj.gameObject.SetActive(true);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.CancelJumping();
        PlayerMovement.obj.spriteRenderer.flipX = false;
        Player.obj.SetAnimatorLayerAndHasCape(false);
        PlayerMovement.obj.Freeze();

        CaveAvatar.obj.gameObject.SetActive(false);

        yield return new WaitForSeconds(6f);

        AmbienceManager.obj.Play(_ambience);
        yield return StartCoroutine(FadeInScreenAndStart());

        GameManager.obj.RegisterEvent(_eliFirstForestRoomLoaded);

        yield return null;
    }

    private IEnumerator FadeInScreenAndStart() {
        yield return new WaitForSeconds(2f); //Give title screen time to unload
        _tentCutsceneManager.StartTentSequence();

        yield return null;
    }
}
