using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave6CapePickedManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _capePicked;
    [SerializeField] private GameObject _cape;
    [SerializeField] private List<GameObject> _blobs;
    [SerializeField] private PowerUpScreen _powerUpScreen;
    [SerializeField] private Transform _finalPlayerPosition;
    [SerializeField] private GameObject _blobsContainer;
    [SerializeField] private GameObject _pickCapeTrigger;
    [SerializeField] private MusicTrack _musicTrack;
    [SerializeField] private EventReference _powerupFanfareStinger;
    [SerializeField] private EventReference _capePickedRoarSfx;
    
    private EventInstance _capePickedRoarInstance;
    private Coroutine _cutsceneCoroutine;

    public void RequestSkip() {
        StopCoroutine(_cutsceneCoroutine);
        _pickCapeTrigger.SetActive(false);

        CameraShakeManager.obj.ShakeCamera(0, 0, 0);

        WhiteFadeManager.obj.Reset();

        _cape.SetActive(false);
        _blobsContainer.SetActive(false);
        FadeOutAndStopAmbience();

        CaveAvatar.obj.SetPosition(_finalPlayerPosition.position);
        CaveAvatar.obj.FollowPlayer();
        CaveAvatar.obj.SetFloatingEnabled(true);
        Player.obj.SetAnimatorLayerAndHasCape(true);
        PlayerPowersManager.obj.EliCanForcePush = true;
        Player.obj.transform.position = _finalPlayerPosition.position;
        Player.obj.ResetAnimator();

        AudioUtils.SafeStop(ref _capePickedRoarInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);

        MusicManager.obj.Play(_musicTrack);

        GameManager.obj.RegisterEvent(_capePicked);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        PlayerMovement.obj.UnFreeze();
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

    private IEnumerator StartCutscene() {
        PauseMenuManager.obj.RegisterSkippable(this);

        PlayerMovement.obj.Freeze();
        FadeOutAndStopAmbience();

        StartSoundEvent(_capePickedRoarSfx, ref _capePickedRoarInstance);

        List<Animator> animators = new List<Animator>();
        foreach(var blob in _blobs) {
            animators.Add(blob.GetComponent<Animator>());
        }
        foreach(Animator animator in animators) {
            animator.speed = 10;
        }

        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 6f);

        yield return new WaitForSeconds(0.2f);

        WhiteFadeManager.obj.StartFadeOut();
        yield return new WaitForSeconds(2.5f);
        Player.obj.SetAnimatorLayerAndHasCape(true);
        PlayerPowersManager.obj.EliCanForcePush = true;
        Player.obj.transform.position = _finalPlayerPosition.position;
        PlayerMovement.obj.SetNewPower();
        _cape.SetActive(false);
        WhiteFadeManager.obj.StartFadeIn();
        
        yield return new WaitForSeconds(3.3f);
        
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

        PlayerMovement.obj.SetNewPowerRecevied();
        yield return new WaitForSeconds(2);
        PlayerMovement.obj.UnFreeze();

        MusicManager.obj.Play(_musicTrack);
        
        GameManager.obj.RegisterEvent(_capePicked);

        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public void FadeOutAndStopAmbience() {
        AmbienceManager.obj.Stop();
    }
}
