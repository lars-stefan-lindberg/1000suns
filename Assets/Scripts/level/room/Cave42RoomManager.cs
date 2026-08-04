using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FunkyCode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave42RoomManager : MonoBehaviour
{
    [SerializeField] private SpawnPoint _eliStartPosition;
    [SerializeField] private SpawnPoint _afterElevatorSpawnPoint;
    [SerializeField] private Transform _sootStartPosition;
    [SerializeField] private Transform _elevatorStopPosition;
    [SerializeField] private CaveElevator _elevator;
    [SerializeField] private GameEventId _elevatorCompleted;
    [SerializeField] private MusicTrack _caveMain;
    
    [Header("Elevator Sound Settings")]
    [SerializeField] private float _elevatorSoundFadeDuration = 5f;
    [SerializeField] [Range(0.1f, 1f)] private float _soundMaxIntensityAtSpeedPercent = 0.5f;
    
    private const string ELEVATOR_SOUND_NAME = "ElevatorBuzz";

    void Start()
    {
        if(GameManager.obj.HasEvent(_elevatorCompleted)) {
            _elevator.transform.position = new Vector2(_elevator.transform.position.x, _elevatorStopPosition.position.y);
            _elevator.GetComponentInChildren<LightSprite2D>().enabled = true;
            return;
        }
        SceneFadeManager.obj.SetFadedOutState();
        PlayerMovement.obj.isOnMoveable = true;
        PlayerMovement.obj.moveableRigidbody = _elevator.GetComponent<Rigidbody2D>();
        Player.obj.transform.position = _eliStartPosition.transform.position;
        Player.obj.gameObject.SetActive(true);
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.Freeze();

        CaveAvatar.obj.SetPosition(_sootStartPosition.position);
        CaveAvatar.obj.IsFollowingPlayer = true;

        GameObject[] sceneGameObjects = gameObject.scene.GetRootGameObjects();
        GameObject mainCamera = sceneGameObjects.First(gameObject => gameObject.CompareTag("MainCamera"));
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        GameObject room = sceneGameObjects.First(gameObject => gameObject.CompareTag("Room"));
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.PlayerType.HUMAN), _eliStartPosition.transform.position);

        SceneManager.SetActiveScene(gameObject.scene);

        StartCoroutine(StartScene());
    }

    private IEnumerator StartScene() {
        //Give some time to transition from previous scene
        yield return new WaitForSeconds(1f);
        _elevator.SetStopPosition(_elevatorStopPosition.position.y);
        _elevator.StartMoving();
        _elevator.GetComponentInChildren<LightSprite2D>().enabled = true;

        yield return new WaitForSeconds(2f);

        SceneFadeManager.obj.StartFadeIn(0.8f);
        StartCoroutine(FadeElevatorSoundBack());
        StartCoroutine(UpdateElevatorSoundDeceleration());
        
        while(SceneFadeManager.obj.IsFadingIn)
            yield return null;

        MusicManager.obj.Play(_caveMain);

        while(!_elevator.HasReachedStop())
            yield return null;

        yield return new WaitForSeconds(1f);
        GameManager.obj.RegisterEvent(_elevatorCompleted);
        GameManager.obj.SetCurrentSpawnPointId(_afterElevatorSpawnPoint.SpawnPointID);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
    }
    
    private IEnumerator FadeElevatorSoundBack()
    {
        if (!SoundFXManager.obj.HasNamedSound(ELEVATOR_SOUND_NAME))
            yield break;
        
        // Get current fade value
        EventInstance elevatorSound = SoundFXManager.obj.GetNamedSound(ELEVATOR_SOUND_NAME);
        elevatorSound.getParameterByName("fade", out float currentFadeValue);
        
        float elapsed = 0f;
        
        while (elapsed < _elevatorSoundFadeDuration)
        {
            if (!SoundFXManager.obj.HasNamedSound(ELEVATOR_SOUND_NAME))
                yield break;
                
            elapsed += Time.deltaTime;
            float t = elapsed / _elevatorSoundFadeDuration;
            float fadeValue = Mathf.Lerp(currentFadeValue, 1f, t);
            
            elevatorSound = SoundFXManager.obj.GetNamedSound(ELEVATOR_SOUND_NAME);
            SoundFXManager.obj.SetSoundParameter(elevatorSound, "fade", fadeValue);
            
            yield return null;
        }
        
        // Ensure final value is set to 1
        if (SoundFXManager.obj.HasNamedSound(ELEVATOR_SOUND_NAME))
        {
            elevatorSound = SoundFXManager.obj.GetNamedSound(ELEVATOR_SOUND_NAME);
            SoundFXManager.obj.SetSoundParameter(elevatorSound, "fade", 1f);
        }
    }
    
    private IEnumerator UpdateElevatorSoundDeceleration()
    {
        // Wait until elevator starts decelerating (speed starts decreasing)
        float previousSpeed = _elevator.CurrentSpeed;
        
        // Wait for deceleration to begin
        while (SoundFXManager.obj.HasNamedSound(ELEVATOR_SOUND_NAME) && _elevator.CurrentSpeed >= previousSpeed)
        {
            previousSpeed = _elevator.CurrentSpeed;
            yield return null;
        }
        
        // Update accelerate parameter as elevator decelerates
        while (SoundFXManager.obj.HasNamedSound(ELEVATOR_SOUND_NAME) && _elevator.CurrentSpeed > 0.01f)
        {
            EventInstance elevatorSound = SoundFXManager.obj.GetNamedSound(ELEVATOR_SOUND_NAME);
            
            float currentSpeed = _elevator.CurrentSpeed;
            float maxSpeed = _elevator.MaxSpeed;
            float speedThreshold = maxSpeed * _soundMaxIntensityAtSpeedPercent;
            float intensity = Mathf.Clamp01(currentSpeed / speedThreshold);
            
            SoundFXManager.obj.SetSoundParameter(elevatorSound, "accelerate", intensity);
            
            yield return null;
        }
        
        // Elevator has stopped, stop the sound
        SoundFXManager.obj.StopNamedSound(ELEVATOR_SOUND_NAME, allowFadeout: true);
    }
}
