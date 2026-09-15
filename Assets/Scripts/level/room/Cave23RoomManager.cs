using System.Collections;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMOD.Studio;

public class Cave23RoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private EventReference _teleportSfx;
    [SerializeField] private EventReference _voices;
    [SerializeField] private EventReference _invisibleGrabWithBuildUp;
    [SerializeField] private EventReference _invisibleGrabWithDelay;
    [SerializeField] private EventReference _invisibleGrab;
    [SerializeField] private EventReference _stinger;
    [SerializeField] private Transform _voicesStartPosition;
    [SerializeField] private Transform _voicesEndPosition;
    [SerializeField] private SceneField _dreamRoomScene;
    [SerializeField] private SceneField _dreamRoomDeeScene;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private GameEventId _teleportInitiated;
    [SerializeField] private GameEventId _teleportInitiatedDee;
    [SerializeField] private GameEventId _dreamSequenceCompleted;
    [SerializeField] private GameEventId _postDreamSequenceCompleted;
    [SerializeField] private GameEventId _postDeeDreamSequenceCompleted;
    [SerializeField] private SpawnPoint _eliReturnFromDreamRoomPosition;
    [SerializeField] private GameEventId _deeDreamSequenceCompleted;
    [SerializeField] private Transform _sootStartPositionAfterDreamRoom;
    [SerializeField] private ConversationManager _conversationManagerEli;
    [SerializeField] private ConversationManager _conversationManagerDee;
    [SerializeField] private AmbienceTrack _caveMainAmbience;
    [SerializeField] private GameObject _crystalCutsceneCamera;
    [SerializeField] private SpriteFlash _crystalFlash;
    [SerializeField] private LightFlash _lightVfx;
    [SerializeField] private Transform _eliCutsceneStopPosition;
    
    [Header("Voice Audio Settings")]
    [SerializeField] private float _initialVolumeFadeSpeed = 2f;
    [SerializeField] private float _initialVolumeTarget = 0.5f;
    
    private EventInstance _voicesInstance;
    private bool _voicesPlaying = false;
    private PARAMETER_ID _fadeParamId;
    private bool _fadeParameterInitialized = false;
    private float _currentInitialVolume = 0f;
    private bool _initialFadeComplete = false;
    private EventInstance _stingerInstance;
    private EventInstance _invisibleGrabWithDelayInstance;
    private EventInstance _invisibleGrabWithBuildUpInstance;
    private CaveTimelineId.Id _activeCaveTimeline;
    private Coroutine _cutsceneCoroutine;

    void Start() {
        _activeCaveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        //If coming back from dream room, load room state
        if(_activeCaveTimeline == CaveTimelineId.Id.Eli) {
            if(GameManager.obj.HasEvent(_dreamSequenceCompleted) && !GameManager.obj.HasEvent(_postDreamSequenceCompleted)) {
                _conversationManagerEli.enabled = true;
                _conversationManagerEli.OnConversationEnd += OnConversationCompletedEli;
                StartCoroutine(AfterEliDreamRoom());
            }
        } else if(_activeCaveTimeline == CaveTimelineId.Id.Dee) {
            if(GameManager.obj.HasEvent(_deeDreamSequenceCompleted) && !GameManager.obj.HasEvent(_postDeeDreamSequenceCompleted)) {
                _conversationManagerDee.enabled = true;
                _conversationManagerDee.OnConversationEnd += OnConversationCompletedDee;
                StartCoroutine(AfterDeeDreamRoom());
            }
        }
    }
    
    void FixedUpdate()
    {
        if (GameManager.obj.HasEvent(_teleportInitiated) && _activeCaveTimeline == CaveTimelineId.Id.Eli)
            return;
        if (GameManager.obj.HasEvent(_teleportInitiatedDee) && _activeCaveTimeline == CaveTimelineId.Id.Dee)
            return;
            
        if (_voicesStartPosition == null || _voicesEndPosition == null)
            return;

        Transform playerTransform;
        if(_activeCaveTimeline == CaveTimelineId.Id.Eli) {
            playerTransform = Player.obj.transform;
        } else if(_activeCaveTimeline == CaveTimelineId.Id.Dee) {
            playerTransform = ShadowTwinPlayer.obj.transform;
        } else {
            return;
        }
        
        float playerX = playerTransform.position.x;
        float startX = _voicesStartPosition.position.x;
        float endX = _voicesEndPosition.position.x;
        
        bool playerInRange = (playerX >= Mathf.Min(startX, endX) && playerX <= Mathf.Max(startX, endX));
        
        if (playerInRange && !_voicesPlaying)
        {
            StartVoices();
        }
        else if (!playerInRange && _voicesPlaying)
        {
            StopVoices();
        }
        
        if (_voicesPlaying)
        {
            UpdateVoicesFade(playerX, startX, endX);
        }
    }

    private void OnDestroy()
    {
        _conversationManagerEli.OnConversationEnd -= OnConversationCompletedEli;
        StopVoices();
    }

    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        
        if(_activeCaveTimeline == CaveTimelineId.Id.Eli) {
            PlayerMovement.obj.UnFreeze();
        } else if(_activeCaveTimeline == CaveTimelineId.Id.Dee) {
            ShadowTwinMovement.obj.UnFreeze();
        }
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    private IEnumerator AfterEliDreamRoom() {
        AmbienceManager.obj.Stop();
        AmbienceManager.obj.Play(_caveMainAmbience);
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.SetStartingOnGround();
        Player.obj.transform.position = _eliReturnFromDreamRoomPosition.transform.position;
        PlayerMovement.obj.SetNewPower();

        //Set Soot start position
        CaveAvatar.obj.SetPosition(_sootStartPositionAfterDreamRoom.position, false);
        CaveAvatar.obj.SetFlipX(true);

        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.HUMAN), _eliReturnFromDreamRoomPosition.transform.position);

        SceneManager.SetActiveScene(gameObject.scene);

        //Give things some time to properly load
        yield return new WaitForSeconds(1f);

        WhiteSceneFadeManager.obj.StartFadeIn(0.5f);
        while(WhiteSceneFadeManager.obj.IsFadingIn)
            yield return null;


        PlayerMovement.obj.SetNewPowerReceived();
        yield return new WaitForSeconds(2);

        StartCoroutine(SetupDialogueEli());
    }

    private IEnumerator AfterDeeDreamRoom() {
        AmbienceManager.obj.Stop();
        AmbienceManager.obj.Play(_caveMainAmbience);
        ShadowTwinMovement.obj.isGrounded = true;
        ShadowTwinMovement.obj.SetStartingOnGround();
        ShadowTwinPlayer.obj.transform.position = _eliReturnFromDreamRoomPosition.transform.position;
        ShadowTwinMovement.obj.TriggerGetHitAnimation();

        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.SHADOW_TWIN), _eliReturnFromDreamRoomPosition.transform.position);

        SceneManager.SetActiveScene(gameObject.scene);

        //Give things some time to properly load
        yield return new WaitForSeconds(1f);

        WhiteSceneFadeManager.obj.StartFadeIn(0.5f);
        while(WhiteSceneFadeManager.obj.IsFadingIn)
            yield return null;


        yield return new WaitForSeconds(1f);
        ShadowTwinPlayer.obj.PlayBreathingOnKnees();
        yield return new WaitForSeconds(1f);
        StartCoroutine(SetupDialogueDee());
    }
    
    public void TeleportToDreamRoom() {
        if(GameManager.obj.HasEvent(_dreamSequenceCompleted) || _activeCaveTimeline != CaveTimelineId.Id.Eli)
            return;
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        PlayerMovement.obj.Freeze();
        CaveAvatar.obj.IsFollowingPlayer = false;
        AmbienceManager.obj.Stop();
        
        _cutsceneCoroutine = StartCoroutine(TeleportToDreamRoomRoutine());
    }

    public void TeleportToDreamRoomDee() {
        if(GameManager.obj.HasEvent(_deeDreamSequenceCompleted) || _activeCaveTimeline != CaveTimelineId.Id.Dee)
            return;
        ShadowTwinMovement.obj.Freeze();
        AmbienceManager.obj.Stop();
        
        _cutsceneCoroutine = StartCoroutine(TeleportToDreamRoomDeeRoutine());
    }

    public void RequestSkip() {
        if(!GameManager.obj.HasEvent(_dreamSequenceCompleted)) {
            if(_activeCaveTimeline == CaveTimelineId.Id.Eli) {
                if (_cutsceneCoroutine != null) {
                    StopCoroutine(_cutsceneCoroutine);
                    _cutsceneCoroutine = null;
                }

                AmbienceManager.obj.Stop();
                AmbienceManager.obj.Play(_caveMainAmbience);

                AudioUtils.SafeStop(ref _stingerInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);

                _crystalCutsceneCamera.SetActive(false);

                PlayerMovement.obj.SetMovementInput(Vector2.zero);
                Player.obj.ResetAnimator();
                PlayerMovement.obj.IsControlledProgrammatically = false;
                Player.obj.rigidBody.gravityScale = 1;
                PlayerMovement.obj.isGrounded = true;
                PlayerMovement.obj.SetStartingOnGround();
                Player.obj.transform.position = _eliReturnFromDreamRoomPosition.transform.position;

                CaveAvatar.obj.SetPosition(_sootStartPositionAfterDreamRoom.position, false);
                CaveAvatar.obj.SetFlipX(true);
                CaveAvatar.obj.IsFollowingPlayer = true;

                CameraShakeManager.obj.ShakeCamera(0, 0, 0);

                StopVoices();

                AudioUtils.SafeStop(ref _invisibleGrabWithDelayInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
                AudioUtils.SafeStop(ref _invisibleGrabWithBuildUpInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);

                _crystalFlash.AbortFlash();
                _lightVfx.AbortFlash();

                GameManager.obj.RegisterEvent(_teleportInitiated);
                GameManager.obj.RegisterEvent(_dreamSequenceCompleted);
                GameManager.obj.RegisterEvent(_postDreamSequenceCompleted);
                SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
            } else if(_activeCaveTimeline == CaveTimelineId.Id.Dee) {
                if (_cutsceneCoroutine != null) {
                    StopCoroutine(_cutsceneCoroutine);
                    _cutsceneCoroutine = null;
                }

                AmbienceManager.obj.Stop();
                AmbienceManager.obj.Play(_caveMainAmbience);

                _crystalCutsceneCamera.SetActive(false);

                ShadowTwinPlayer.obj.ResetAnimator();
                ShadowTwinMovement.obj.isGrounded = true;
                ShadowTwinMovement.obj.SetStartingOnGround();
                ShadowTwinPlayer.obj.transform.position = _eliReturnFromDreamRoomPosition.transform.position;

                CameraShakeManager.obj.ShakeCamera(0, 0, 0);

                StopVoices();

                AudioUtils.SafeStop(ref _invisibleGrabWithDelayInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);

                _crystalFlash.AbortFlash();
                _lightVfx.AbortFlash();

                GameManager.obj.RegisterEvent(_teleportInitiatedDee);
                GameManager.obj.RegisterEvent(_deeDreamSequenceCompleted);
                GameManager.obj.RegisterEvent(_postDeeDreamSequenceCompleted);
                SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
            }
        }
        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator TeleportToDreamRoomRoutine() {
        PauseMenuManager.obj.RegisterSkippable(this);

        GameManager.obj.RegisterEvent(_teleportInitiated);

        _stingerInstance = SoundFXManager.obj.CreateAttachedInstance(_stinger, gameObject);
        _stingerInstance.start();
        yield return new WaitForSeconds(1f);
        _crystalCutsceneCamera.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        PlayerMovement.obj.StartWalking();
        PlayerMovement.obj.SetMovementInput(new Vector2(1, 0));
        yield return new WaitForSeconds(0.2f);
        
        _invisibleGrabWithBuildUpInstance = SoundFXManager.obj.CreateAttachedInstance(_invisibleGrabWithBuildUp, Player.obj.gameObject);
        _invisibleGrabWithBuildUpInstance.start();
        _invisibleGrabWithBuildUpInstance.release();

        while (Player.obj.transform.position.x < _eliCutsceneStopPosition.position.x) {
            yield return null;
        }
        _crystalFlash.Flash();
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        PlayerMovement.obj.StopWalking();
        yield return null;
        _lightVfx.Flash();
        PlayerMovement.obj.SetNewPower();
        CameraShakeManager.obj.ForcePushShake();
        yield return new WaitForSeconds(1.5f);

        PlayerMovement.obj.IsControlledProgrammatically = true;
        Player.obj.rigidBody.gravityScale = 0;

        //Move player up in the air
        float startY = Player.obj.transform.position.y;
        float targetY = startY + 3f;
        float maxSpeed = 2f;
        float acceleration = 1f;
        float deceleration = 1f;
        float currentSpeed = 0f;

        _invisibleGrabWithDelayInstance = SoundFXManager.obj.CreateAttachedInstance(_invisibleGrabWithDelay, Player.obj.gameObject);
        _invisibleGrabWithDelayInstance.start();
        _invisibleGrabWithDelayInstance.release();
        _crystalFlash.Flash();
        CameraShakeManager.obj.ForcePushShake();
        while (Player.obj.transform.position.y < targetY) {
            float distanceRemaining = targetY - Player.obj.transform.position.y;
            float stoppingDistance = (currentSpeed * currentSpeed) / (2f * deceleration);
            
            if (distanceRemaining <= stoppingDistance) {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            } else {
                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
            }
            
            Vector3 pos = Player.obj.transform.position;
            pos.y += currentSpeed * Time.deltaTime;
            pos.y = Mathf.Min(pos.y, targetY);
            Player.obj.transform.position = pos;
            yield return null;
        }
        
        StopVoices();

        PauseMenuManager.obj.UnregisterSkippable();
        GameManager.obj.IsPauseAllowed = false;

        SoundFXManager.obj.Play2D(_teleportSfx);
        WhiteSceneFadeManager.obj.StartFadeOut(0.5f);

        yield return new WaitForSeconds(0.5f);
        AudioUtils.SafeStop(ref _stingerInstance, FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        while(WhiteSceneFadeManager.obj.IsFadingOut)
            yield return null;
            

        Player.obj.gameObject.SetActive(false);
        Player.obj.rigidBody.gravityScale = 1;
        PlayerMovement.obj.IsControlledProgrammatically = false;
        _crystalCutsceneCamera.SetActive(false);

        //Load dream room
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_dreamRoomScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone) {
            yield return null;
        }

        //Give some time for dream room to load until unloading current room
        yield return new WaitForSeconds(2f);

        //Unload current room
        SceneManager.UnloadSceneAsync(_thisScene.SceneName);
    }

    private IEnumerator TeleportToDreamRoomDeeRoutine() {
        PauseMenuManager.obj.RegisterSkippable(this);

        GameManager.obj.RegisterEvent(_teleportInitiatedDee);

        _crystalCutsceneCamera.SetActive(true);

        yield return new WaitForSeconds(1f);
        
        SoundFXManager.obj.PlayAtPosition(_invisibleGrab, Player.obj.transform.position);
        
        _crystalFlash.Flash();
        _lightVfx.Flash();
        ShadowTwinMovement.obj.SetNewPower();
        CameraShakeManager.obj.ForcePushShake();
        yield return new WaitForSeconds(2f);

        _invisibleGrabWithDelayInstance = SoundFXManager.obj.CreateAttachedInstance(_invisibleGrabWithDelay, Player.obj.gameObject);
        _invisibleGrabWithDelayInstance.start();
        _invisibleGrabWithDelayInstance.release();
        _crystalFlash.Flash();
        
        StopVoices();

        PauseMenuManager.obj.UnregisterSkippable();
        GameManager.obj.IsPauseAllowed = false;

        SoundFXManager.obj.Play2D(_teleportSfx);
        WhiteSceneFadeManager.obj.StartFadeOut(0.5f);

        while(WhiteSceneFadeManager.obj.IsFadingOut)
            yield return null;
            
        ShadowTwinPlayer.obj.gameObject.SetActive(false);
        _crystalCutsceneCamera.SetActive(false);

        //Load dream room
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_dreamRoomDeeScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone) {
            yield return null;
        }

        //Give some time for dream room to load until unloading current room
        yield return new WaitForSeconds(2f);

        //Unload current room
        SceneManager.UnloadSceneAsync(_thisScene.SceneName);
    }

    private IEnumerator SetupDialogueEli() {
        yield return new WaitForSeconds(0.5f);
        _conversationManagerEli.StartConversation();
        GameManager.obj.IsPauseAllowed = true;
    }

    private IEnumerator SetupDialogueDee() {
        _conversationManagerDee.StartConversation();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    private void OnConversationCompletedEli() {
        _conversationManagerEli.CleanUp();
        PlayerMovement.obj.UnFreeze();
        CaveAvatar.obj.IsFollowingPlayer = true;
        GameManager.obj.RegisterEvent(_postDreamSequenceCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        _conversationManagerEli.OnConversationEnd -= OnConversationCompletedEli;
        _conversationManagerEli.enabled = false;
    }

    private void OnConversationCompletedDee() {
        StartCoroutine(OnConversationCompletedDeeCoroutine());
    }

    private IEnumerator OnConversationCompletedDeeCoroutine() {
        _conversationManagerDee.CleanUp();
        ShadowTwinPlayer.obj.PlayGetUp();
        yield return new WaitForSeconds(1.7f);
        ShadowTwinMovement.obj.UnFreeze();
        GameManager.obj.RegisterEvent(_postDeeDreamSequenceCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        _conversationManagerDee.OnConversationEnd -= OnConversationCompletedDee;
        _conversationManagerDee.enabled = false;
        yield break;
    }
    
    private void StartVoices()
    {
        _voicesInstance = RuntimeManager.CreateInstance(_voices);
        _voicesInstance.start();
        _voicesPlaying = true;
        _currentInitialVolume = 0f;
        _initialFadeComplete = false;
        _fadeParameterInitialized = false;
        InitializeFadeParameter();
    }
    
    private void StopVoices()
    {
        if (_voicesPlaying && _voicesInstance.isValid())
        {
            _voicesInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _voicesInstance.release();
        }
        _voicesPlaying = false;
        _fadeParameterInitialized = false;
        _initialFadeComplete = false;
        _currentInitialVolume = 0f;
    }
    
    private void InitializeFadeParameter()
    {
        if (!_voicesInstance.isValid())
            return;
            
        try
        {
            _voicesInstance.getDescription(out var desc);
            desc.getParameterDescriptionByName("fade", out var fadeParamDesc);
            _fadeParamId = fadeParamDesc.id;
            _fadeParameterInitialized = true;
            _voicesInstance.setParameterByID(_fadeParamId, 0f);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to initialize FMOD fade parameter: {e.Message}");
        }
    }
    
    private void UpdateVoicesFade(float playerX, float startX, float endX)
    {
        if (!_fadeParameterInitialized)
        {
            InitializeFadeParameter();
            return;
        }
        
        if (!_initialFadeComplete)
        {
            _currentInitialVolume += _initialVolumeFadeSpeed * Time.deltaTime;
            if (_currentInitialVolume >= _initialVolumeTarget)
            {
                _currentInitialVolume = _initialVolumeTarget;
                _initialFadeComplete = true;
            }
        }
        
        float distanceFromStart = Mathf.Abs(playerX - startX);
        float totalDistance = Mathf.Abs(endX - startX);
        float normalizedDistance = totalDistance > 0 ? distanceFromStart / totalDistance : 0f;
        
        float fadeValue;
        if (!_initialFadeComplete)
        {
            fadeValue = _currentInitialVolume;
        }
        else
        {
            fadeValue = Mathf.Lerp(_initialVolumeTarget, 1f, normalizedDistance);
        }
        
        SetFadeParameter(fadeValue);
    }
    
    private void SetFadeParameter(float value)
    {
        try
        {
            if (_voicesInstance.isValid())
            {
                _voicesInstance.setParameterByID(_fadeParamId, value);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to set FMOD fade parameter: {e.Message}");
        }
    }
}
