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
    [SerializeField] private Transform _deeBehindElevatorPosition;
    [SerializeField] private Transform _deeCutsceneEliStartPosition;
    [SerializeField] private Transform _deeCutsceneEliEndPosition;
    
    [Header("Sound Settings")]
    [SerializeField] [Range(0.1f, 1f)] private float _soundMaxIntensityAtSpeedPercent = 0.5f;
    
    private const string ELEVATOR_SOUND_NAME = "ElevatorBuzz";
    private EventInstance _elevatorCrystalSfxInstance;

    private Coroutine _cutsceneCoroutine;
    private Coroutine _updateElevatorSoundCoroutine;

    public void StartElevator() {
        _cutsceneCoroutine = StartCoroutine(StartElevatorCoroutine());
    }

    public void StartDeeCutscene() {
        _cutsceneCoroutine = StartCoroutine(DeeCutsceneCoroutine());
    }

    private IEnumerator DeeCutsceneCoroutine() {
        PauseMenuManager.obj.RegisterSkippable(this);
        ShadowTwinMovement.obj.Freeze();
        yield return new WaitForSeconds(1f);

        //Play back Eli sfx

        ShadowTwinMovement.obj.FlipPlayer();

        yield return new WaitForSeconds(1f);

        SpriteRenderer deeRenderer = ShadowTwinMovement.obj.spriteRenderer;
        deeRenderer.sortingLayerName = "Background props";
        deeRenderer.sortingOrder = 0;

        ShadowTwinMovement.obj.SetMovementInput(new Vector2(1, 0));
        while(ShadowTwinMovement.obj.transform.position.x < _deeBehindElevatorPosition.position.x) {
            yield return null;
        }
        ShadowTwinMovement.obj.SetMovementInput(Vector2.zero);
        ShadowTwinPlayer.obj.FadeOutPlayerLight();
        yield return new WaitForSeconds(1f);
        //Make Dee invisible and set position out of Eli's way (collider collision), but still remin in room to not unload room objects
        deeRenderer.enabled = false;
        ShadowTwinMovement.obj.StopMovement();
        ShadowTwinPlayer.obj.DisableGravity();
        ShadowTwinPlayer.obj.transform.position = _deeBehindElevatorPosition.position + new Vector3(0, 9f, 0f);

        yield return new WaitForSeconds(1f);

        //Setup Eli position
        Player.obj.gameObject.SetActive(true);
        Player.obj.SetAnimatorLayerAndHasCape(true);
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.SetStartingOnGround();
        Player.obj.ResetAnimator();
        Player.obj.StartAnimator();
        if(PlayerMovement.obj.IsFacingLeft())
            PlayerMovement.obj.FlipPlayer();
        Player.obj.transform.position = _deeCutsceneEliStartPosition.position;

        //Set cave avatar start position, and follow Eli
        CaveAvatar.obj.gameObject.SetActive(true);
        CaveAvatar.obj.transform.position = _deeCutsceneEliStartPosition.position;
        CaveAvatar.obj.OverriddenPlayerType = PlayerManager.PlayerType.HUMAN;
        CaveAvatar.obj.IsFollowingPlayer = true;

        //Steer Eli to designated position
        PlayerMovement.obj.SetMovementInput(new Vector2(1, 0));
        yield return new WaitForSeconds(0.1f);
        PlayerMovement.obj.SimulateJumpInput(true, Time.time);
        yield return new WaitForSeconds(0.1f);
        PlayerMovement.obj.SimulateJumpInput(false, Time.time);
        yield return new WaitForSeconds(0.8f);
        PlayerMovement.obj.SimulateJumpInput(true, Time.time);
        yield return new WaitForSeconds(0.1f);
        PlayerMovement.obj.SimulateJumpInput(false, Time.time);
        while(PlayerMovement.obj.transform.position.x < _deeCutsceneEliEndPosition.position.x) {
            yield return null;
        }
        PlayerMovement.obj.SetMovementInput(Vector2.zero);
        yield return new WaitForSeconds(1f);

        //Start elevator
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

        Player.obj.gameObject.SetActive(false);
        CaveAvatar.obj.gameObject.SetActive(false);

        //Switch backgrounds
        yield return StartCoroutine(BackgroundLoaderManager.obj.RemoveBackgroundLayers());
        yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground("CaveBg2"));

        //Load next scene
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_skipElevatorScene, LoadSceneMode.Additive);
        while(!asyncOperation.isDone)
            yield return null;
        InitRoom initRoomData = LevelManager.obj.GetInitRoomData(SceneManager.GetSceneByName(_skipElevatorScene));
        LevelManager.obj.LoadAdjacentRooms(initRoomData);

        yield return new WaitForSeconds(2f);
        
        //Unload this scene
        SceneManager.UnloadSceneAsync(_thisScene);
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

        var caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Dee) {
            Player.obj.gameObject.SetActive(false);
            PlayerMovement.obj.SetMovementInput(Vector2.zero);
            PlayerMovement.obj.CancelJumping();
            CaveAvatar.obj.gameObject.SetActive(false);

            SpriteRenderer deeRenderer = ShadowTwinMovement.obj.spriteRenderer;
            deeRenderer.sortingLayerName = "Background props";
            deeRenderer.sortingOrder = 0;

            ShadowTwinPlayer.obj.FadeOutPlayerLight();
            deeRenderer.enabled = false;
            ShadowTwinMovement.obj.StopMovement();
            ShadowTwinPlayer.obj.DisableGravity();
            ShadowTwinMovement.obj.SetMovementInput(Vector2.zero);
            if(ShadowTwinMovement.obj.IsFacingLeft()) {
                ShadowTwinMovement.obj.FlipPlayer();
            }
        }

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
