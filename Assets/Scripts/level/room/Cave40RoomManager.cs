using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave40RoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private CaveElevator _elevator;
    [SerializeField] private SpriteFlash _elevatorFlash;
    [SerializeField] private SceneField _nextScene;
    [SerializeField] private SceneField _skipElevatorScene;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private EventReference _elevatorBuzzSfx;
    [SerializeField] private EventReference _elevatorCrystalSfx;
    
    [Header("Sound Settings")]
    [SerializeField] [Range(0.1f, 1f)] private float _soundMaxIntensityAtSpeedPercent = 0.5f;
    
    private const string ELEVATOR_SOUND_NAME = "ElevatorBuzz";
    private EventInstance _elevatorCrystalSfxInstance;

    private Coroutine _cutsceneCoroutine;
    private Coroutine _updateElevatorSoundCoroutine;

    public void StartElevator() {
        _cutsceneCoroutine = StartCoroutine(StartElevatorCoroutine());
    }

    private IEnumerator StartElevatorCoroutine() {
        PauseMenuManager.obj.RegisterSkippable(this);
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(0.1f);

        _elevatorCrystalSfxInstance = SoundFXManager.obj.CreateAttachedInstance(_elevatorCrystalSfx, _elevator.gameObject);
        _elevatorCrystalSfxInstance.start();
        _elevatorCrystalSfxInstance.release();

        yield return new WaitForSeconds(1.4f);
        _elevatorFlash.Flash();
        yield return new WaitForSeconds(2.5f);
        
        SoundFXManager.obj.StartNamedPersistent2DSound(ELEVATOR_SOUND_NAME, _elevatorBuzzSfx);
        _elevator.StartMoving();
        
        _updateElevatorSoundCoroutine = StartCoroutine(UpdateElevatorSoundCoroutine());

        yield return new WaitForSeconds(3f);

        PauseMenuManager.obj.UnregisterSkippable();
        GameManager.obj.IsPauseAllowed = false;

        SceneFadeManager.obj.StartFadeOut(0.8f);
        while(SceneFadeManager.obj.IsFadingOut)
            yield return null;

        //Load next scene
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_nextScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone)
            yield return null;
        
        //Unload this scene
        SceneManager.UnloadSceneAsync(_thisScene);
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null) {
            StopCoroutine(_cutsceneCoroutine);
        }
        if(_updateElevatorSoundCoroutine != null) {
            StopCoroutine(_updateElevatorSoundCoroutine);
        } else {
            SoundFXManager.obj.StartNamedPersistent2DSound(ELEVATOR_SOUND_NAME, _elevatorBuzzSfx);
        }

        AudioUtils.SafeStop(ref _elevatorCrystalSfxInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        _elevatorFlash.AbortFlash();
        _elevator.StopAbruptly();
        Destroy(_elevator.gameObject);

        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {

        //Switch backgrounds
        yield return StartCoroutine(BackgroundLoaderManager.obj.RemoveBackgroundLayers());
        yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground("CaveBg2"));

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_skipElevatorScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone)
            yield return null;
        InitRoom initRoomData = LevelManager.obj.GetInitRoomData(SceneManager.GetSceneByName(_skipElevatorScene));
        LevelManager.obj.LoadAdjacentRooms(initRoomData);

        EventInstance elevatorSound = SoundFXManager.obj.GetNamedSound(ELEVATOR_SOUND_NAME);
        SoundFXManager.obj.SetSoundParameter(elevatorSound, "accelerate", 1);
        //Give elevator sound coroutine some time to finish before unloading scene
        yield return new WaitForSeconds(2f);

        SceneManager.UnloadSceneAsync(_thisScene);
    }
    
    private IEnumerator UpdateElevatorSoundCoroutine()
    {
        while (SoundFXManager.obj.HasNamedSound(ELEVATOR_SOUND_NAME))
        {
            EventInstance elevatorSound = SoundFXManager.obj.GetNamedSound(ELEVATOR_SOUND_NAME);
            
            float currentSpeed = _elevator.CurrentSpeed;
            float maxSpeed = _elevator.MaxSpeed;
            float speedThreshold = maxSpeed * _soundMaxIntensityAtSpeedPercent;
            float intensity = Mathf.Clamp01(currentSpeed / speedThreshold);
            
            SoundFXManager.obj.SetSoundParameter(elevatorSound, "accelerate", intensity);
            
            yield return null;
        }
    }
}
