using System.Collections;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMOD.Studio;

public class Cave23RoomManager : MonoBehaviour
{
    [SerializeField] private EventReference _teleportSfx;
    [SerializeField] private EventReference _voices;
    [SerializeField] private EventReference _invisibleGrabWithBuildUp;
    [SerializeField] private EventReference _invisibleGrabWithDelay;
    [SerializeField] private EventReference _stinger;
    [SerializeField] private Transform _voicesStartPosition;
    [SerializeField] private Transform _voicesEndPosition;
    [SerializeField] private SceneField _dreamRoomScene;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private GameEventId _teleportInitiated;
    [SerializeField] private GameEventId _dreamSequenceCompleted;
    [SerializeField] private GameEventId _postDreamSequenceCompleted;
    [SerializeField] private SpawnPoint _eliReturnFromDreamRoomPosition;
    [SerializeField] private Transform _sootStartPositionAfterDreamRoom;
    [SerializeField] private ConversationManager _conversationManager;
    [SerializeField] private AmbienceTrack _caveMainAmbience;
    [SerializeField] private GameObject _crystalCutsceneCamera;
    [SerializeField] private SpriteFlash _crystalFlash;
    [SerializeField] private LightFlash _lightVfx;
    
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
    private CaveTimelineId.Id _activeCaveTimeline;

    void Start() {
        _activeCaveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        //If coming back from dream room, load room state
        if(_activeCaveTimeline == CaveTimelineId.Id.Eli) {
            if(GameManager.obj.HasEvent(_dreamSequenceCompleted) && !GameManager.obj.HasEvent(_postDreamSequenceCompleted)) {
                _conversationManager.OnConversationEnd += OnConversationCompleted;
                StartCoroutine(AfterEliDreamRoom());
            }
        }
    }
    
    void FixedUpdate()
    {
        if (GameManager.obj.HasEvent(_teleportInitiated) && _activeCaveTimeline == CaveTimelineId.Id.Eli)
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
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
        StopVoices();
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

        SceneFadeManager.obj.StartFadeIn(0.5f);
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;


        PlayerMovement.obj.SetNewPowerReceived();
        yield return new WaitForSeconds(2);

        StartCoroutine(SetupDialogue());
    }
    
    public void TeleportToDreamRoom() {
        if(GameManager.obj.HasEvent(_dreamSequenceCompleted) || _activeCaveTimeline != CaveTimelineId.Id.Eli)
            return;
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        PlayerMovement.obj.Freeze();
        CaveAvatar.obj.IsFollowingPlayer = false;
        AmbienceManager.obj.Stop();
        
        StartCoroutine(TeleportToDreamRoomRoutine());
    }

    private IEnumerator TeleportToDreamRoomRoutine() {
        GameManager.obj.RegisterEvent(_teleportInitiated);

        _stingerInstance = SoundFXManager.obj.CreateAttachedInstance(_stinger, gameObject);
        _stingerInstance.start();
        yield return new WaitForSeconds(1f);
        _crystalCutsceneCamera.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        PlayerMovement.obj.StartWalking();
        PlayerMovement.obj.SetMovementInput(new Vector2(1, 0));
        yield return new WaitForSeconds(0.2f);
        SoundFXManager.obj.PlayAtPosition(_invisibleGrabWithBuildUp, Player.obj.transform.position);
        yield return new WaitForSeconds(2.05f);
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

        SoundFXManager.obj.PlayAtPosition(_invisibleGrabWithDelay, Player.obj.transform.position);
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
        SoundFXManager.obj.Play2D(_teleportSfx);
        SceneFadeManager.obj.StartWhiteFadeOut(0.5f);

        yield return new WaitForSeconds(0.5f);
        AudioUtils.SafeStop(ref _stingerInstance, FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        while(SceneFadeManager.obj.IsFadingOut)
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

    private IEnumerator SetupDialogue() {
        yield return new WaitForSeconds(0.5f);
        _conversationManager.StartConversation();
    }

    private void OnConversationCompleted() {
        _conversationManager.CleanUp();
        PlayerMovement.obj.UnFreeze();
        CaveAvatar.obj.IsFollowingPlayer = true;
        GameManager.obj.RegisterEvent(_postDreamSequenceCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        _conversationManager.OnConversationEnd -= OnConversationCompleted;
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
