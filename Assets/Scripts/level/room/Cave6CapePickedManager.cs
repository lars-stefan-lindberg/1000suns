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

        WhiteSceneFadeManager.obj.Reset();

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
        } else if(caveTimeline == CaveTimelineId.Id.Dee) {
            ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(true);
            PlayerPowersManager.obj.DeeCanForcePull = true;
            ShadowTwinPlayer.obj.transform.position = _finalPlayerPosition.position;
            ShadowTwinMovement.obj.SetStartingOnGround();
            ShadowTwinMovement.obj.isGrounded = true;
            ShadowTwinMovement.obj.CancelJumping();
            ShadowTwinPlayer.obj.ResetAnimator();
        }

        AudioUtils.SafeStop(ref _capePickedRoarInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        StartCoroutine(ResumeGameplay(caveTimeline));
    }

    private IEnumerator ResumeGameplay(CaveTimelineId.Id caveTimeline) {
        if(caveTimeline == CaveTimelineId.Id.Eli) {
            yield return null;
            PlayerMovement.obj.SetNewPower();
            yield return null;
            Player.obj.StartAnimator();
            yield return null;
        } else if(caveTimeline == CaveTimelineId.Id.Dee) {
            yield return null;
            ShadowTwinMovement.obj.SetNewPower();
            yield return null;
            ShadowTwinPlayer.obj.StartAnimator();
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);

        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
        if(caveTimeline == CaveTimelineId.Id.Eli) {
            StartCoroutine(EliPowerUpScreen());
        } else if(caveTimeline == CaveTimelineId.Id.Dee) {
            StartCoroutine(DeePowerUpScreen());
        }
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
        WhiteSceneFadeManager.obj.SetSortingLayer("Player", 6);
        WhiteSceneFadeManager.obj.StartFadeOut(_lightFadeOutSpeed);

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
        WhiteSceneFadeManager.obj.StartFadeIn(_lightFadeInSpeed);
        
        yield return new WaitForSeconds(3.3f);

        WhiteSceneFadeManager.obj.RestoreLayer();
        
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

        PauseMenuManager.obj.UnregisterSkippable();
        StartCoroutine(EliPowerUpScreen());
    }

    public IEnumerator EliPowerUpScreen() {
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

        yield return null;
    }

    private IEnumerator StartCutsceneCrownPicked() {
        PauseMenuManager.obj.RegisterSkippable(this);

        ShadowTwinMovement.obj.Freeze();
        ShadowTwinPlayer.obj.SetAnimatorSpeed(0);
        FadeOutAndStopAmbience();

        StartSoundEvent(_capePickedRoarSfx, ref _capePickedRoarInstance);
        WhiteSceneFadeManager.obj.SetSortingLayer("Player", 6);
        WhiteSceneFadeManager.obj.StartFadeOut(_lightFadeOutSpeed);

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
        WhiteSceneFadeManager.obj.StartFadeIn(_lightFadeInSpeed);
        
        yield return new WaitForSeconds(3.3f);

        WhiteSceneFadeManager.obj.RestoreLayer();
        
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

        PauseMenuManager.obj.UnregisterSkippable();
        StartCoroutine(DeePowerUpScreen());
    }

    public IEnumerator DeePowerUpScreen() {
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

        //Clean up Soot, it's safe from this point since you can't backtrack
        CaveAvatar.obj.gameObject.SetActive(false);

        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        yield return null;
    }

    public void FadeOutAndStopAmbience() {
        AmbienceManager.obj.Stop();
    }
}
