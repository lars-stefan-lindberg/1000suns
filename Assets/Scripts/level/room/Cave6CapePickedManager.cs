using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave6CapePickedManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _capePicked;
    [SerializeField] private GameEventId _crownPicked;
    [SerializeField] private GameObject _cape;
    [SerializeField] private GameObject _crown;
    [SerializeField] private List<GameObject> _blobs;
    [SerializeField] private PowerUpScreen _powerUpScreen;
    [SerializeField] private PowerUpScreen _powerUpScreenDee;
    [SerializeField] private Transform _finalPlayerPosition;
    [SerializeField] private GameObject _blobsContainer;
    [SerializeField] private GameObject _pickCapeTrigger;
    [SerializeField] private GameObject _pickCrownTrigger;
    [SerializeField] private MusicTrack _musicTrack;
    [SerializeField] private EventReference _powerupFanfareStinger;
    [SerializeField] private EventReference _capePickedRoarSfx;
    [SerializeField] private float _lightFadeInSpeed = 1.5f;
    [SerializeField] private float _lightFadeOutSpeed = 2f;
    
    private EventInstance _capePickedRoarInstance;
    private Coroutine _cutsceneCoroutine;

    public void RequestSkip() {
        StopCoroutine(_cutsceneCoroutine);
        _pickCapeTrigger.SetActive(false);
        _pickCrownTrigger.SetActive(false);

        CameraShakeManager.obj.ShakeCamera(0, 0, 0);

        SceneFadeManager.obj.Reset();

        _cape.SetActive(false);
        _crown.SetActive(false);
        _blobsContainer.SetActive(false);
        FadeOutAndStopAmbience();

        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli) {    
            CaveAvatar.obj.FollowPlayer();
            CaveAvatar.obj.SetFloatingEnabled(true);
            Player.obj.SetAnimatorLayerAndHasCape(true);
            PlayerPowersManager.obj.EliCanForcePush = true;
            Player.obj.transform.position = _finalPlayerPosition.position;
            PlayerMovement.obj.SetStartingOnGround();
            PlayerMovement.obj.isGrounded = true;
            PlayerMovement.obj.CancelJumping();
            Player.obj.ResetAnimator();
            GameManager.obj.RegisterEvent(_capePicked);
        } else if(caveTimeline == CaveTimelineId.Id.Dee) {
            ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(true);
            PlayerPowersManager.obj.DeeCanForcePull = true;
            ShadowTwinPlayer.obj.transform.position = _finalPlayerPosition.position;
            ShadowTwinMovement.obj.SetStartingOnGround();
            ShadowTwinMovement.obj.isGrounded = true;
            ShadowTwinMovement.obj.CancelJumping();
            ShadowTwinPlayer.obj.ResetAnimator();
            GameManager.obj.RegisterEvent(_crownPicked);
        }

        AudioUtils.SafeStop(ref _capePickedRoarInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        MusicManager.obj.Play(_musicTrack);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        StartCoroutine(ResumeGameplay(caveTimeline));
    }

    private IEnumerator ResumeGameplay(CaveTimelineId.Id caveTimeline) {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        if(caveTimeline == CaveTimelineId.Id.Eli) {
            PlayerMovement.obj.UnFreeze();
        } else if(caveTimeline == CaveTimelineId.Id.Dee) {
            ShadowTwinMovement.obj.UnFreeze();
        }
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    private void StartSoundEvent(EventReference reference, ref EventInstance instance) {
        instance = SoundFXManager.obj.CreateAttachedInstance(reference, gameObject, null);
        instance.start();
        instance.release();
    }

    public void Activate() {
        if(GameManager.obj.HasEvent(_capePicked)) 
            return;
        _cutsceneCoroutine = StartCoroutine(StartCutscene());
    }

    public void ActivateCrownPicked() {
        if(GameManager.obj.HasEvent(_crownPicked)) 
            return;
        _cutsceneCoroutine = StartCoroutine(StartCutsceneCrownPicked());
    }

    private IEnumerator StartCutscene() {
        PauseMenuManager.obj.RegisterSkippable(this);

        PlayerMovement.obj.Freeze();
        Player.obj.SetAnimatorSpeed(0);
        FadeOutAndStopAmbience();

        StartSoundEvent(_capePickedRoarSfx, ref _capePickedRoarInstance);
        SceneFadeManager.obj.SetSortingLayer("Player", 6);
        SceneFadeManager.obj.StartWhiteFadeOut(_lightFadeOutSpeed);

        List<Animator> animators = new List<Animator>();
        foreach(var blob in _blobs) {
            animators.Add(blob.GetComponent<Animator>());
        }
        foreach(Animator animator in animators) {
            animator.speed = 10;
        }

        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 6f);

        yield return new WaitForSeconds(0.2f);

        
        yield return new WaitForSeconds(1.5f);
        Player.obj.StartAnimator();
        yield return new WaitForSeconds(1f);
        Player.obj.SetAnimatorLayerAndHasCape(true);
        PlayerPowersManager.obj.EliCanForcePush = true;
        Player.obj.transform.position = _finalPlayerPosition.position;
        PlayerMovement.obj.SetStartingOnGround();
        PlayerMovement.obj.isGrounded = true;
        PlayerMovement.obj.CancelJumping();
        PlayerMovement.obj.SetNewPower();
        _cape.SetActive(false);
        SceneFadeManager.obj.StartFadeIn(_lightFadeInSpeed);
        
        yield return new WaitForSeconds(3.3f);

        SceneFadeManager.obj.RestoreLayer();
        
        List<SpriteRenderer> blobSprites = new();
        foreach(var blob in _blobs) {
            blobSprites.Add(blob.GetComponent<SpriteRenderer>());
        }
        while(blobSprites[0].color.a > 0) {
            for (int i = 0; i < blobSprites.Count; i++)
            {
                var blobSprite = blobSprites[i];
                blobSprite.color = new Color(blobSprite.color.r, blobSprite.color.b, blobSprite.color.g, Mathf.MoveTowards(blobSprite.color.a, 0, 3.5f * Time.deltaTime));
            }
            yield return null;
        }

        PlayerMovement.obj.Freeze();

        PauseMenuManager.obj.UnregisterSkippable();
        GameManager.obj.IsPauseAllowed = false;
        Time.timeScale = 0;
        _powerUpScreen.Show();
        SoundFXManager.obj.Play2D(_powerupFanfareStinger);
        while(!_powerUpScreen.PowerUpScreenCompleted) {
            yield return null;
        }
        Time.timeScale = 1;
        GameManager.obj.IsPauseAllowed = true;

        PlayerMovement.obj.SetNewPowerReceived();
        yield return new WaitForSeconds(2);
        PlayerMovement.obj.UnFreeze();

        MusicManager.obj.Play(_musicTrack);
        
        GameManager.obj.RegisterEvent(_capePicked);

        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    private IEnumerator StartCutsceneCrownPicked() {
        PauseMenuManager.obj.RegisterSkippable(this);

        ShadowTwinMovement.obj.Freeze();
        ShadowTwinPlayer.obj.SetAnimatorSpeed(0);
        FadeOutAndStopAmbience();

        StartSoundEvent(_capePickedRoarSfx, ref _capePickedRoarInstance);
        SceneFadeManager.obj.SetSortingLayer("Player", 6);
        SceneFadeManager.obj.StartWhiteFadeOut(_lightFadeOutSpeed);

        List<Animator> animators = new List<Animator>();
        foreach(var blob in _blobs) {
            animators.Add(blob.GetComponent<Animator>());
        }
        foreach(Animator animator in animators) {
            animator.speed = 10;
        }

        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 6f);

        yield return new WaitForSeconds(0.2f);

        yield return new WaitForSeconds(1.5f);
        ShadowTwinPlayer.obj.StartAnimator();
        yield return new WaitForSeconds(1f);
        ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(true);
        PlayerPowersManager.obj.DeeCanForcePull = true;
        ShadowTwinPlayer.obj.transform.position = _finalPlayerPosition.position;
        ShadowTwinMovement.obj.SetStartingOnGround();
        ShadowTwinMovement.obj.isGrounded = true;
        ShadowTwinMovement.obj.CancelJumping();
        ShadowTwinMovement.obj.SetNewPower();
        _crown.SetActive(false);
        SceneFadeManager.obj.StartFadeIn(_lightFadeInSpeed);
        
        yield return new WaitForSeconds(3.3f);

        SceneFadeManager.obj.RestoreLayer();
        
        List<SpriteRenderer> blobSprites = new();
        foreach(var blob in _blobs) {
            blobSprites.Add(blob.GetComponent<SpriteRenderer>());
        }
        while(blobSprites[0].color.a > 0) {
            for (int i = 0; i < blobSprites.Count; i++)
            {
                var blobSprite = blobSprites[i];
                blobSprite.color = new Color(blobSprite.color.r, blobSprite.color.b, blobSprite.color.g, Mathf.MoveTowards(blobSprite.color.a, 0, 3.5f * Time.deltaTime));
            }
            yield return null;
        }

        ShadowTwinMovement.obj.Freeze();

        PauseMenuManager.obj.UnregisterSkippable();
        GameManager.obj.IsPauseAllowed = false;
        Time.timeScale = 0;
        _powerUpScreenDee.Show();
        SoundFXManager.obj.Play2D(_powerupFanfareStinger);
        while(!_powerUpScreenDee.PowerUpScreenCompleted) {
            yield return null;
        }
        Time.timeScale = 1;
        GameManager.obj.IsPauseAllowed = true;

        ShadowTwinMovement.obj.SetNewPowerRecevied();
        yield return new WaitForSeconds(2);
        ShadowTwinMovement.obj.UnFreeze();

        MusicManager.obj.Play(_musicTrack);
        
        GameManager.obj.RegisterEvent(_crownPicked);

        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void FadeOutAndStopAmbience() {
        AmbienceManager.obj.Stop();
    }
}
