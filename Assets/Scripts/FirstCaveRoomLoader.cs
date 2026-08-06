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
    [SerializeField] private ConversationManager _conversationManagerEli;
    [SerializeField] private ConversationManager _conversationManagerDee1;
    [SerializeField] private ConversationManager _conversationManagerDee2;
    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private GameObject _cutsceneCameraDee;
    [SerializeField] private SpawnPoint _deeSpawnPoint;
    [SerializeField] private ShockwaveColliderEmitter _shockwaveEmitter;
    [SerializeField] private GameObject _shockwaveEmitterTrigger;
    [SerializeField] private GameObject _deeCloseToEliTrigger;
    private bool _isDee = false;

    private Coroutine _cutsceneCoroutine;
    private Coroutine _deeCloseToEliCoroutine;

    void Start() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(!GameManager.obj.HasEvent(_eliFirstCaveRoomLoaded) && caveTimeline == CaveTimelineId.Id.Eli) {
            StartCoroutine(LoadRoomEli());
            _conversationManagerEli.enabled = true;
            _conversationManagerEli.OnConversationEnd += OnConversationCompletedEli;
        } else if(!GameManager.obj.HasEvent(_deeFirstCaveRoomCutsceneCompleted) && caveTimeline == CaveTimelineId.Id.Dee) {
            _isDee = true;
            _conversationManagerDee1.enabled = true;
            _conversationManagerDee1.OnConversationEnd += OnConversationCompletedDee1;
            StartCoroutine(LoadRoomDee());
        }
    }

    void Update() {
        if(!SceneFadeManager.obj.IsFadingIn) {
            SceneFadeManager.obj.SetFadeInSpeed(5f);
        }
    }

    public void TriggerShockwave() {
        _shockwaveEmitter.TriggerShockwave(0);
    }

    public void DeeCloseToEliTriggerHandler() {
        ShadowTwinMovement.obj.SetMovementInput(Vector2.zero);
        _deeCloseToEliCoroutine = StartCoroutine(DeeCloseToEliTriggerHandlerCoroutine());
    }

    private IEnumerator DeeCloseToEliTriggerHandlerCoroutine() {
        yield return new WaitForSeconds(1f);
        _shockwaveEmitter.TriggerShockwave(0);
        yield return new WaitForSeconds(1f);
        _shockwaveEmitterTrigger.SetActive(true);
        yield return new WaitForSeconds(2f);
        _conversationManagerDee2.StartConversation();
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

        yield return null;

        _cutsceneCameraDee.SetActive(true);

        CaveAvatar.obj.SetPosition(ShadowTwinPlayer.obj.transform.position);
        CaveAvatar.obj.gameObject.SetActive(true);
        CaveAvatar.obj.FollowPlayer();
        CaveAvatar.obj.SetEyeColor(Color.white);

        AmbienceManager.obj.Play(_caveMainAmbience);
        AmbienceManager.obj.Play(_caveMainWaterDripping);
        _cutsceneCoroutine = StartCoroutine(StartSceneDee());

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

        _conversationManagerEli.StartConversation();
        yield return null;
    }

    private IEnumerator StartSceneDee() {
        yield return new WaitForSeconds(1f);
        SceneFadeManager.obj.SetFadedOutState();
        SceneFadeManager.obj.SetFadeInSpeed(0.3f);
        SceneFadeManager.obj.StartFadeIn();
        yield return new WaitForSeconds(4f);

        PauseMenuManager.obj.RegisterSkippable(this);
        GameManager.obj.IsPauseAllowed = true;

        _conversationManagerDee1.StartConversation();
        yield return null;
    }
    
    private void OnConversationCompletedEli() {
        GameManager.obj.RegisterEvent(_eliFirstCaveRoomLoaded);

        PlayerMovement.obj.UnFreeze();
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        _conversationManagerEli.OnConversationEnd -= OnConversationCompletedEli;
        _conversationManagerEli.enabled = false;
        PauseMenuManager.obj.UnregisterSkippable();
    }

    private void OnConversationCompletedDee1() {
        _conversationManagerDee1.OnConversationEnd -= OnConversationCompletedDee1;
        _conversationManagerDee1.enabled = false;
        _conversationManagerDee2.enabled = true;
        _conversationManagerDee2.OnConversationEnd += OnConversationCompletedDee2;
        _cutsceneCoroutine = StartCoroutine(MoveDeeTowardsEli());
    }

    private IEnumerator MoveDeeTowardsEli() {
        yield return new WaitForSeconds(1f);
        ShadowTwinMovement.obj.SetMovementInput(new Vector2(-1, 0));
        yield return new WaitForSeconds(0.4f);
        ShadowTwinMovement.obj.SimulateJumpInput(true, Time.time);
        yield return new WaitForSeconds(0.3f);
        ShadowTwinMovement.obj.SimulateJumpInput(false, Time.time);
        yield return new WaitForSeconds(0.5f);
        ShadowTwinMovement.obj.SimulateJumpInput(true, Time.time);
        yield return new WaitForSeconds(0.3f);
        ShadowTwinMovement.obj.SimulateJumpInput(false, Time.time);
    }

    private void OnConversationCompletedDee2() {
        PauseMenuManager.obj.UnregisterSkippable();
        ShadowTwinMovement.obj.UnFreeze();
        _conversationManagerDee2.OnConversationEnd -= OnConversationCompletedDee2;
        _conversationManagerDee2.enabled = false;
        _cutsceneCameraDee.SetActive(false);

        GameManager.obj.RegisterEvent(_deeFirstCaveRoomCutsceneCompleted);
        GameManager.obj.SetCurrentSpawnPointId(_deeSpawnPoint.SpawnPointID);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null)
            StopCoroutine(_cutsceneCoroutine);
        if(_deeCloseToEliCoroutine != null)
            StopCoroutine(_deeCloseToEliCoroutine);

        if(!_isDee) {
            Player.obj.ResetAnimator();
            Player.obj.StartAnimator();
            _conversationManagerEli.HardStopConversation();
            _conversationManagerEli.CleanUp();
            _conversationManagerEli.OnConversationEnd -= OnConversationCompletedEli;
            _zoomedCamera.SetActive(false);
            GameManager.obj.RegisterEvent(_eliFirstCaveRoomLoaded);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);        
            StartCoroutine(ResumeGameplayEli());
        } else {
            ShadowTwinMovement.obj.SetMovementInput(Vector2.zero);
            ShadowTwinPlayer.obj.SetCaveStartingCoordinates();
            ShadowTwinMovement.obj.SetStartingOnGround();
            ShadowTwinMovement.obj.isGrounded = true;
            ShadowTwinMovement.obj.CancelJumping();
            _conversationManagerDee1.HardStopConversation();
            _conversationManagerDee1.OnConversationEnd -= OnConversationCompletedDee1;
            _conversationManagerDee2.HardStopConversation();
            _conversationManagerDee2.CleanUp();
            _conversationManagerDee2.OnConversationEnd -= OnConversationCompletedDee2;
            _cutsceneCameraDee.SetActive(false);
            _shockwaveEmitterTrigger.SetActive(true);
            _deeCloseToEliTrigger.SetActive(false);
            _shockwaveEmitter.AbortShockwave();
            GameManager.obj.RegisterEvent(_deeFirstCaveRoomCutsceneCompleted);
            GameManager.obj.SetCurrentSpawnPointId(_deeSpawnPoint.SpawnPointID);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
            StartCoroutine(ResumeGameplayDee());
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

    private IEnumerator ResumeGameplayDee() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        ShadowTwinMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    void OnDestroy() {
        _conversationManagerEli.OnConversationEnd -= OnConversationCompletedEli;
        _conversationManagerDee1.OnConversationEnd -= OnConversationCompletedDee1;
        _conversationManagerDee2.OnConversationEnd -= OnConversationCompletedDee2;
    }
}
