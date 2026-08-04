using System.Collections;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave41RoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private SpawnPoint _eliStartPosition;
    [SerializeField] private Transform _sootStartPosition;
    [SerializeField] private ConversationManager _conversation1Manager;
    [SerializeField] private ConversationManager _conversation2Manager;
    [SerializeField] private ConversationManager _conversation3Manager;
    [SerializeField] private ConversationManager _conversation4Manager;
    [SerializeField] private CaveElevator _elevator;
    [SerializeField] private Animator _backgroundAnimator;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private SceneField _nextScene;
    [SerializeField] private float _animatorSlowdownDuration = 1f;
    [SerializeField] private float _firstForcePushChargeTime = 0.4f;
    [SerializeField] private float _secondForcePushChargeTime = 0.4f;
    [SerializeField] private float _thirdForcePushChargeTime = 0.8f;
    [SerializeField] private float _waitTimeBetweenForcePushes = 0.4f;
    
    [Header("Elevator Sound Settings")]
    [SerializeField] private float _elevatorSoundFadeDuration = 1f;
    [SerializeField] [Range(0f, 1f)] private float _elevatorSoundFadeValue = 0.7f;

    private const string ELEVATOR_SOUND_NAME = "ElevatorBuzz";
    private float _sootDistanceToElevator;
    private bool _moveSoot = false;
    private Coroutine _cutsceneCoroutine;
    private Coroutine _fadeElevatorSoundCoroutine;
    private Coroutine _performThreeForcePushesCoroutine;

    void Start() {
        PlayerMovement.obj.isOnMoveable = true;
        PlayerMovement.obj.moveableRigidbody = _elevator.GetComponent<Rigidbody2D>();
        PlayerMovement.obj.Freeze();
        SceneFadeManager.obj.SetFadedOutState();
        Player.obj.transform.position = _eliStartPosition.transform.position;
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        if(PlayerMovement.obj.IsFacingLeft())
            PlayerMovement.obj.FlipPlayer();

        CaveAvatar.obj.IsFollowingPlayer = false;
        CaveAvatar.obj.SetPosition(_sootStartPosition.position);
        CaveAvatar.obj.SetFlipX(true);

        SceneManager.SetActiveScene(gameObject.scene);

        _sootDistanceToElevator = _sootStartPosition.position.y - _elevator.transform.position.y;

        _cutsceneCoroutine = StartCoroutine(StartScene());
    }

    void OnDestroy() {
        _conversation1Manager.OnConversationEnd -= OnConversation1Completed;
        _conversation2Manager.OnConversationEnd -= OnConversation2Completed;
        _conversation3Manager.OnConversationEnd -= OnConversation3Completed;
    }

    void Update() {
        if(_moveSoot) {
            CaveAvatar.obj.SetPosition(new Vector2(CaveAvatar.obj.transform.position.x, _elevator.transform.position.y + _sootDistanceToElevator), false);
        }
    }

    private IEnumerator StartScene() {
        PauseMenuManager.obj.RegisterSkippable(this);

        //Give some time to transition from previous scene
        yield return new WaitForSeconds(1.5f);
        
        _fadeElevatorSoundCoroutine = StartCoroutine(FadeElevatorSound());
        
        SceneFadeManager.obj.StartFadeIn(0.8f);
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;

        GameManager.obj.IsPauseAllowed = true;
    
        yield return new WaitForSeconds(1.5f);
        PlayerMovement.obj.FlipPlayer();
        yield return new WaitForSeconds(1f);
        _conversation1Manager.enabled = true;
        _conversation1Manager.OnConversationEnd += OnConversation1Completed;
        _conversation1Manager.StartConversation();
    }

    private void OnConversation1Completed() {
        _conversation1Manager.OnConversationEnd -= OnConversation1Completed;
        _conversation1Manager.enabled = false;

        StartCoroutine(OnConversation1CompletedCoroutine());
    }

    private IEnumerator OnConversation1CompletedCoroutine() {
        _performThreeForcePushesCoroutine = StartCoroutine(PerformThreeForcePushes());
        yield return _performThreeForcePushesCoroutine;
        yield return new WaitForSeconds(0.2f);
        PlayerMovement.obj.TriggerFallToKnees();
        _conversation2Manager.enabled = true;
        _conversation2Manager.OnConversationEnd += OnConversation2Completed;
        yield return new WaitForSeconds(2.5f);
        PlayerMovement.obj.SetBreathingOnKnees(true);
        yield return new WaitForSeconds(2.5f);
        _conversation2Manager.StartConversation();
    }

    private void OnConversation2Completed() {
        _conversation2Manager.OnConversationEnd -= OnConversation2Completed;
        _conversation2Manager.enabled = false;

        StartCoroutine(OnConversation2CompletedCoroutine());
    }

    private IEnumerator OnConversation2CompletedCoroutine() { 
        yield return new WaitForSeconds(2f);
        PlayerMovement.obj.SetBreathingOnKnees(false);
        yield return new WaitForSeconds(2f);
        _conversation3Manager.enabled = true;
        _conversation3Manager.OnConversationEnd += OnConversation3Completed;
        _conversation3Manager.StartConversation();
    }

    private void OnConversation3Completed() {
        _conversation3Manager.OnConversationEnd -= OnConversation3Completed;
        _conversation3Manager.enabled = false;

        StartCoroutine(OnConversation3CompletedCoroutine());
    }

    private IEnumerator OnConversation3CompletedCoroutine() { 
        yield return new WaitForSeconds(1.5f);
        PlayerMovement.obj.FlipPlayer();
        yield return new WaitForSeconds(1f);
        _conversation4Manager.enabled = true;
        _conversation4Manager.OnConversationEnd += OnConversation4Completed;
        _conversation4Manager.StartConversation();
    }

    public IEnumerator PerformThreeForcePushes() {
        // First force push - brief hold
        PlayerPush.obj.SimulateShootHold();
        yield return new WaitForSeconds(_firstForcePushChargeTime);
        PlayerPush.obj.SimulateShootRelease();
        
        yield return new WaitForSeconds(_waitTimeBetweenForcePushes);
        
        // Second force push - brief hold
        PlayerPush.obj.SimulateShootHold();
        yield return new WaitForSeconds(_secondForcePushChargeTime);
        PlayerPush.obj.SimulateShootRelease();
        
        yield return new WaitForSeconds(_waitTimeBetweenForcePushes);
        
        // Third force push - slightly longer hold
        PlayerPush.obj.SimulateShootHold();
        yield return new WaitForSeconds(_thirdForcePushChargeTime);
        PlayerPush.obj.SimulateShootRelease();
    }

    private void OnConversation4Completed() {
        _conversation3Manager.OnConversationEnd -= OnConversation4Completed;
        _conversation3Manager.enabled = false;
        _conversation4Manager.CleanUp();

        StartCoroutine(OnConversation4CompletedCoroutine());
    }

    private IEnumerator SlowDownAndStopAnimator() {
        float elapsed = 0f;
        float startSpeed = _backgroundAnimator.speed;
        
        while(elapsed < _animatorSlowdownDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / _animatorSlowdownDuration;
            _backgroundAnimator.speed = Mathf.Lerp(startSpeed, 0f, t);
            yield return null;
        }
        
        _backgroundAnimator.speed = 0f;
        _backgroundAnimator.enabled = false;
    }

    private IEnumerator OnConversation4CompletedCoroutine() {
        _moveSoot = true;
        _elevator.StartMoving();
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(SlowDownAndStopAnimator());
        yield return new WaitForSeconds(2f);

        PauseMenuManager.obj.UnregisterSkippable();
        GameManager.obj.IsPauseAllowed = false;

        Player.obj.gameObject.SetActive(false);
        _elevator.StopAbruptly();
        SceneFadeManager.obj.StartFadeOut(0.8f);
        
        while(SceneFadeManager.obj.IsFadingOut) {
            yield return null;
        }

        //Switch backgrounds
        yield return StartCoroutine(BackgroundLoaderManager.obj.RemoveBackgroundLayers());
        yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground("CaveBg2"));

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone)
            yield return null;
        InitRoom initRoomData = LevelManager.obj.GetInitRoomData(SceneManager.GetSceneByName(_nextScene));
        LevelManager.obj.LoadAdjacentRooms(initRoomData);

        SceneManager.UnloadSceneAsync(_thisScene);
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null) {
            StopCoroutine(_cutsceneCoroutine);
        }
        if(_fadeElevatorSoundCoroutine != null) {
            StopCoroutine(_fadeElevatorSoundCoroutine);
        }
        if(_performThreeForcePushesCoroutine != null) {
            StopCoroutine(_performThreeForcePushesCoroutine);
        }

        //Stop all conversations
        _conversation1Manager.OnConversationEnd -= OnConversation1Completed;
        if(_conversation1Manager.enabled) {
            _conversation1Manager.HardStopConversation();
            _conversation1Manager.enabled = false;
        }
        _conversation2Manager.OnConversationEnd -= OnConversation2Completed;
        if(_conversation2Manager.enabled) {
            _conversation2Manager.HardStopConversation();
            _conversation2Manager.enabled = false;
        }
        _conversation3Manager.OnConversationEnd -= OnConversation3Completed;
        if(_conversation3Manager.enabled) {
            _conversation3Manager.HardStopConversation();
            _conversation3Manager.enabled = false;
        }
        _conversation4Manager.OnConversationEnd -= OnConversation4Completed;
        if(_conversation4Manager.enabled) {
            _conversation4Manager.HardStopConversation();
            _conversation4Manager.enabled = false;
        }

        _elevator.StopAbruptly();
        _backgroundAnimator.speed = 0f;
        _backgroundAnimator.enabled = false;

        PlayerPush.obj.AbortShoot();
        if(PlayerMovement.obj.IsFacingLeft())
            PlayerMovement.obj.FlipPlayer();
        Player.obj.ResetAnimator();
        CaveAvatar.obj.IsFollowingPlayer = false;
        _moveSoot = false;

        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        //Switch backgrounds
        yield return StartCoroutine(BackgroundLoaderManager.obj.RemoveBackgroundLayers());
        yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground("CaveBg2"));

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone)
            yield return null;
        InitRoom initRoomData = LevelManager.obj.GetInitRoomData(SceneManager.GetSceneByName(_nextScene));
        LevelManager.obj.LoadAdjacentRooms(initRoomData);

        SceneManager.UnloadSceneAsync(_thisScene);
    }
    
    private IEnumerator FadeElevatorSound()
    {
        if (!SoundFXManager.obj.HasNamedSound(ELEVATOR_SOUND_NAME))
            yield break;
            
        float elapsed = 0f;
        
        while (elapsed < _elevatorSoundFadeDuration)
        {
            if (!SoundFXManager.obj.HasNamedSound(ELEVATOR_SOUND_NAME))
                yield break;
                
            elapsed += Time.deltaTime;
            float t = elapsed / _elevatorSoundFadeDuration;
            float fadeValue = Mathf.Lerp(1f, _elevatorSoundFadeValue, t);
            
            EventInstance elevatorSound = SoundFXManager.obj.GetNamedSound(ELEVATOR_SOUND_NAME);
            SoundFXManager.obj.SetSoundParameter(elevatorSound, "fade", fadeValue);
            
            yield return null;
        }
        
        // Ensure final value is set
        if (SoundFXManager.obj.HasNamedSound(ELEVATOR_SOUND_NAME))
        {
            EventInstance elevatorSound = SoundFXManager.obj.GetNamedSound(ELEVATOR_SOUND_NAME);
            SoundFXManager.obj.SetSoundParameter(elevatorSound, "fade", _elevatorSoundFadeValue);
        }
    }
}
