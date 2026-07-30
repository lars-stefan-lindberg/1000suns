using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave40RoomManager : MonoBehaviour
{
    [SerializeField] private CaveElevator _elevator;
    [SerializeField] private SpriteFlash _elevatorFlash;
    [SerializeField] private SceneField _nextScene;
    [SerializeField] private SceneField _thisScene;
    [SerializeField] private EventReference _elevatorBuzzSfx;
    
    [Header("Sound Settings")]
    [SerializeField] [Range(0.1f, 1f)] private float _soundMaxIntensityAtSpeedPercent = 0.5f;
    
    private const string ELEVATOR_SOUND_NAME = "ElevatorBuzz";

    public void StartElevator() {
        StartCoroutine(StartElevatorCoroutine());
    }

    private IEnumerator StartElevatorCoroutine() {
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(1.5f);
        _elevatorFlash.Flash();
        yield return new WaitForSeconds(2.5f);
        
        SoundFXManager.obj.StartNamedPersistent2DSound(ELEVATOR_SOUND_NAME, _elevatorBuzzSfx);
        _elevator.StartMoving();
        
        StartCoroutine(UpdateElevatorSoundCoroutine());

        yield return new WaitForSeconds(3f);

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
