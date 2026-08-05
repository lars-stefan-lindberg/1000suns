using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstCaveRoomLoader : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _eliFirstCaveRoomLoaded;
    [SerializeField] private GameEventId _deeFirstCaveRoomCutsceneCompleted;
    [SerializeField] private AmbienceTrack _caveMainAmbience;
    [SerializeField] private AmbienceTrack _caveMainWaterDripping;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private SpawnPoint _deeSpawnPoint;
    private bool _isDee = false;

    private Coroutine _cutsceneCoroutine;

    void Start() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(!GameManager.obj.HasEvent(_eliFirstCaveRoomLoaded) && caveTimeline == CaveTimelineId.Id.Eli) {
            StartCoroutine(LoadRoomEli());
            _conversationManager.OnConversationEnd += OnConversationCompleted;
        } else if(!GameManager.obj.HasEvent(_deeFirstCaveRoomCutsceneCompleted) && caveTimeline == CaveTimelineId.Id.Dee) {
            _isDee = true;
            StartCoroutine(LoadRoomDee());
        }
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            SceneFadeManager.obj.SetFadeInSpeed(5f);
        }
    }

    private IEnumerator LoadRoomDee() {
        ShadowTwinPlayer.obj.SetCaveStartingCoordinates();
        ShadowTwinPlayer.obj.gameObject.SetActive(true);
        ShadowTwinMovement.obj.SetStartingOnGround();
        ShadowTwinMovement.obj.isGrounded = true;
        ShadowTwinMovement.obj.CancelJumping();
        ShadowTwinMovement.obj.spriteRenderer.flipX = true;
        ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(false);
        ShadowTwinMovement.obj.Freeze();

        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, ShadowTwinPlayer.obj.transform, ShadowTwinPlayer.obj.transform.position);   

        CaveAvatar.obj.SetPosition(ShadowTwinPlayer.obj.transform.position);
        CaveAvatar.obj.gameObject.SetActive(true);
        CaveAvatar.obj.FollowPlayer();
        CaveAvatar.obj.SetEyeColor(Color.white);

        AmbienceManager.obj.Play(_caveMainAmbience);
        AmbienceManager.obj.Play(_caveMainWaterDripping);
        yield return StartCoroutine(StartSceneDee());

        yield return null;
    }

    private IEnumerator LoadRoomEli() {
        Player.obj.SetCaveStartingCoordinates();
        Player.obj.gameObject.SetActive(true);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.CancelJumping();
        PlayerMovement.obj.spriteRenderer.flipX = false;
        Player.obj.SetAnimatorLayerAndHasCape(false);
        PlayerMovement.obj.Freeze();

        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, Player.obj.transform, Player.obj.transform.position);   

        CaveAvatar.obj.gameObject.SetActive(false);

        AmbienceManager.obj.Play(_caveMainAmbience);
        AmbienceManager.obj.Play(_caveMainWaterDripping);
        _cutsceneCoroutine = StartCoroutine(StartSceneEli());

        yield return null;
    }

    private IEnumerator StartSceneEli() {
        Player.obj.gameObject.GetComponent<EliAudio>().PlayLongFall();
        yield return new WaitForSeconds(2.5f);
        Player.obj.gameObject.GetComponent<EliAudio>().PlayHeavyLand();
        yield return new WaitForSeconds(2f); //Give title screen time to unload
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(0.2f);
        SceneFadeManager.obj.StartFadeIn();
        _zoomedCamera.SetActive(true);

        PauseMenuManager.obj.RegisterSkippable(this);
        GameManager.obj.IsPauseAllowed = true;
        
        Player.obj.PlayGetUpAnimation();
        yield return new WaitForSeconds(4);
        Player.obj.StartAnimator();
        yield return new WaitForSeconds(3);
        _zoomedCamera.SetActive(false);
        yield return new WaitForSeconds(3);

        _conversationManager.StartConversation();
        yield return null;
    }

    private IEnumerator StartSceneDee() {
        yield return new WaitForSeconds(1f);
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(0.3f);
        SceneFadeManager.obj.StartFadeIn();
        yield return new WaitForSeconds(3f);
        GameManager.obj.RegisterEvent(_deeFirstCaveRoomCutsceneCompleted);
        GameManager.obj.SetCurrentSpawnPointId(_deeSpawnPoint.SpawnPointID);
        ShadowTwinMovement.obj.UnFreeze();
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }
    
    private void OnConversationCompleted() {
        GameManager.obj.RegisterEvent(_eliFirstCaveRoomLoaded);

        PlayerMovement.obj.UnFreeze();
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        PauseMenuManager.obj.UnregisterSkippable();
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null)
            StopCoroutine(_cutsceneCoroutine);

        if(!_isDee) {
            Player.obj.ResetAnimator();
            Player.obj.StartAnimator();
            _conversationManager.HardStopConversation();
            _conversationManager.OnConversationEnd -= OnConversationCompleted;
            _zoomedCamera.SetActive(false);
            GameManager.obj.RegisterEvent(_eliFirstCaveRoomLoaded);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);        
            StartCoroutine(ResumeGameplayEli());
        }
    }

    private IEnumerator ResumeGameplayEli() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    void OnDestroy() {
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
    }
}
