using System.Collections;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class CaveInSurface : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _firstCaveRoomLoaded;
    [SerializeField] private GameObject _breakableSurface;
    [SerializeField] private ParticleSystem _breakParticles;
    [SerializeField] private ParticleSystem _crackingParticles;
    [SerializeField] private GameObject _ground;
    [SerializeField] private Animator _visibleTilemapAnimator;
    [SerializeField] private int _particleEmitCount = 50;
    [SerializeField] private ThunderLight _thunderLight;
    [SerializeField] private SceneField _firstCaveBackground;
    [SerializeField] private SceneField _firstCaveSurfaces;
    [SerializeField] private AmbienceTrack _caveMainAmbience;
    [SerializeField] private AmbienceTrack _caveMainWaterDripping;
    public EventReference _breakSfx;
    public EventReference _cracklingfx;

    private Animator _breakableSurfaceAnimator;
    private SpriteRenderer _breakableSurfaceRenderer;
    private Coroutine _cutsceneCoroutine;
    private Coroutine _delayedDisableGroundCoroutine;

    void Awake()
    {
        _breakableSurfaceAnimator = _breakableSurface.GetComponent<Animator>();
        _breakableSurfaceRenderer = _breakableSurface.GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if(collider.CompareTag("Player")) {
            GetComponent<BoxCollider2D>().enabled = false;
            Activate();
        }
    }

    [ContextMenu("Activate cave-in")]
    private void Activate() {
        PauseMenuManager.obj.RegisterSkippable(this);
        _cutsceneCoroutine = StartCoroutine(StartBreakSequence());
    }

    private IEnumerator StartBreakSequence() {
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(1f);
        _thunderLight.Flash();
        Player.obj.PlayBalanceBeforeCaveIn();
        yield return new WaitForSeconds(3.2f);
        _breakableSurfaceAnimator.SetTrigger("break");
    }

    public void OnBreakAnimationComplete() {
        SoundFXManager.obj.PlayAtPosition(_breakSfx, transform.position);
        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 0.5f);
        _breakableSurfaceRenderer.enabled = false;
        _visibleTilemapAnimator.SetTrigger("reveal");
        _breakParticles.Emit(_particleEmitCount);
        _delayedDisableGroundCoroutine = StartCoroutine(DelayedDisableGround());
    }

    private IEnumerator DelayedDisableGround() {
        yield return new WaitForSeconds(0.1f);
        _ground.SetActive(false);
        yield return new WaitForSeconds(2f);
        PauseMenuManager.obj.UnregisterSkippable();
        GameManager.obj.IsPauseAllowed = false;
        StartCoroutine(LoadFirstCaveRoom());
    }

    private IEnumerator LoadFirstCaveRoom() {
        MusicManager.obj.Stop();
        AmbienceManager.obj.Stop();
        Player.obj.gameObject.SetActive(false);

        SceneFadeManager.obj.StartFadeOut(0.5f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(BackgroundLoaderManager.obj.RemoveBackgroundLayers());

        yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground(_firstCaveBackground));
        yield return StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(_firstCaveSurfaces));

        AsyncOperation loadFirstCaveRoomOperation = SceneManager.LoadSceneAsync("Cave-1", LoadSceneMode.Additive);
        while(!loadFirstCaveRoomOperation.isDone) {
            yield return null;
        }
        Scene firstScene = SceneManager.GetSceneByName("Cave-1");
        SceneManager.SetActiveScene(firstScene);
        InitRoom initRoomData = LevelManager.obj.GetInitRoomData(firstScene);
        LevelManager.obj.LoadAdjacentRooms(initRoomData);
        
        SceneManager.UnloadSceneAsync("Forest-1");
        SceneManager.UnloadSceneAsync("Forest-2");

        yield return null;
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null) {
            StopCoroutine(_cutsceneCoroutine);
        }
        if(_delayedDisableGroundCoroutine != null) {
            StopCoroutine(_delayedDisableGroundCoroutine);
        }
        Player.obj.gameObject.SetActive(false);
        AmbienceManager.obj.Stop();
        _breakableSurfaceAnimator.StopPlayback();
        _breakableSurfaceAnimator.enabled = false;
        _thunderLight.Stop();
        CameraShakeManager.obj.ShakeCamera(0, 0, 0);
        GameManager.obj.RegisterEvent(_firstCaveRoomLoaded);
        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        MusicManager.obj.Stop();
        AmbienceManager.obj.Stop();

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(BackgroundLoaderManager.obj.RemoveBackgroundLayers());

        yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground(_firstCaveBackground));
        yield return StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(_firstCaveSurfaces));

        AsyncOperation loadFirstCaveRoomOperation = SceneManager.LoadSceneAsync("Cave-1", LoadSceneMode.Additive);
        while(!loadFirstCaveRoomOperation.isDone) {
            yield return null;
        }
        Scene firstScene = SceneManager.GetSceneByName("Cave-1");
        SceneManager.SetActiveScene(firstScene);
        InitRoom initRoomData = LevelManager.obj.GetInitRoomData(firstScene);
        LevelManager.obj.LoadAdjacentRooms(initRoomData);

        GameObject[] sceneGameObjects = firstScene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, Player.obj.transform, Player.obj.transform.position);   

        CaveAvatar.obj.gameObject.SetActive(false);

        AmbienceManager.obj.Play(_caveMainAmbience);
        AmbienceManager.obj.Play(_caveMainWaterDripping);

        Player.obj.SetCaveStartingCoordinates();
        Player.obj.gameObject.SetActive(true);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.CancelJumping();
        PlayerMovement.obj.spriteRenderer.flipX = false;
        Player.obj.SetAnimatorLayerAndHasCape(false);
        Player.obj.ResetAnimator();

        yield return new WaitForSeconds(1f);  //Give things some time to load and change, like the camera

        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        
        SceneManager.UnloadSceneAsync("Forest-1");
        SceneManager.UnloadSceneAsync("Forest-2");

        yield return null;
    }

    public void PlayCracklingSfx() {
        SoundFXManager.obj.PlayAtPosition(_cracklingfx, transform.position);
        _crackingParticles.Emit(10);
    }

    public void EmitCrackingParticles() {
        _crackingParticles.Emit(10);
    }
}
