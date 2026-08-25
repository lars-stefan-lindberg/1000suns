using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMOD.Studio;
using FMODUnity;
using System.Linq;

public class Cave47DeeRoomManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _cutsceneCompleted;
    [SerializeField] private GameObject _zoomedOutCamera;
    [SerializeField] private GameObject _skipCutsceneCamera;
    [SerializeField] private GameObject _backgroundBlobs;
    [SerializeField] private Transform _sootFlyOffTarget;
    [SerializeField] private ParticleSystem _particleEffect;
    [SerializeField] private EventReference _rumblingSfx;
    [SerializeField] private EventReference _blobTransformSfx;
    [SerializeField] private Transform _eliCutscenePosition;
    [SerializeField] private Transform _sootCutscenePosition;
    [SerializeField] private float _timeBeforeTransformSfx = 0.8f;
    [SerializeField] private float _timeBeforeTransformVfx = 1f;
    private EventInstance _rumblingInstance;
    private EventInstance _blobTransformSfxInstance;

    private Coroutine _cutsceneCoroutine;
    private Coroutine _increaseParticleSpeedCoroutine;
    private Coroutine _startSoundEventsCoroutine;

    public void StartCutscene() {
        if(GameManager.obj.HasEvent(_cutsceneCompleted))
            return;

        ShadowTwinMovement.obj.Freeze();
        _cutsceneCoroutine = StartCoroutine(CutsceneCoroutine());
    }

    private IEnumerator CutsceneCoroutine() {
        PauseMenuManager.obj.RegisterSkippable(this);

        Player.obj.gameObject.SetActive(true);
        Player.obj.transform.position = _eliCutscenePosition.position;
        if(PlayerMovement.obj.IsFacingLeft())
            PlayerMovement.obj.FlipPlayer();
        PlayerMovement.obj.SetStartingOnGround();
        Player.obj.SetAnimatorLayerAndHasCape(true);
        Player.obj.ResetAnimator();
        Player.obj.StartAnimator();
        PlayerMovement.obj.isGrounded = true;

        CaveAvatar.obj.SetPosition(_sootCutscenePosition.position);
        CaveAvatar.obj.SetTarget(_sootCutscenePosition);
        CaveAvatar.obj.gameObject.SetActive(true);
        CaveAvatar.obj.SetRedEyes();
        if(!CaveAvatar.obj.IsFacingLeft())
            CaveAvatar.obj.SetFlipX(true);

        //"Hack" to avoid player switcher to switch to Eli in blob form
        PlayerSwitcher.obj.DisableSwitching();

        _zoomedOutCamera.SetActive(true);

        yield return new WaitForSeconds(3.5f);

        //Loop through all children, get animators, and increase speed of animation
        Animator[] animators = _backgroundBlobs.GetComponentsInChildren<Animator>();
        foreach(Animator animator in animators) {
            animator.speed = 10;
        }

        //Start particle effect
        _particleEffect.Play();
        _increaseParticleSpeedCoroutine = StartCoroutine(GraduallyIncreaseParticleSpeed(_particleEffect, -2, -5, 2.3f, 1f, 7f));

        _startSoundEventsCoroutine = StartCoroutine(StartSoundEvents());

        yield return new WaitForSeconds(_timeBeforeTransformVfx);

        //Turn player into blob, slowly
        //Shake screen
        CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 6.5f);
        Player.obj.FlashFor(6.5f);
        Player.obj.SetAnimatorSpeed(0.035f);
        Player.obj.PlayToBlobAnimation();

        yield return new WaitForSeconds(10f);

        Player.obj.StartAnimator();

        //Soot flies off
        CaveAvatar.obj.SetTarget(_sootFlyOffTarget, 5f);

        //When finished, fade out blobs
        SpriteRenderer[] blobSprites = _backgroundBlobs.GetComponentsInChildren<SpriteRenderer>();
        while(blobSprites.First().color.a > 0) {
            for (int i = 0; i < blobSprites.Length; i++)
            {
                var blobSprite = blobSprites[i];
                blobSprite.color = new Color(blobSprite.color.r, blobSprite.color.b, blobSprite.color.g, Mathf.MoveTowards(blobSprite.color.a, 0, 2.5f * Time.deltaTime));
            }
            yield return null;
        }

        //After finished
        _zoomedOutCamera.SetActive(false);

        yield return new WaitForSeconds(3.5f);

        PlayerBlob.obj.ResetAnimator();
        PlayerBlob.obj.gameObject.SetActive(false);
        Player.obj.gameObject.SetActive(false);
        CaveAvatar.obj.gameObject.SetActive(false);
        PlayerSwitcher.obj.EnableSwitching();

        PauseMenuManager.obj.UnregisterSkippable();

        GameManager.obj.RegisterEvent(_cutsceneCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        ShadowTwinMovement.obj.UnFreeze();
    }

    private IEnumerator StartSoundEvents() {
        yield return new WaitForSeconds(_timeBeforeTransformSfx);
        StartSoundEvent(_blobTransformSfx, ref _blobTransformSfxInstance);

        StartSoundEvent(_rumblingSfx, ref _rumblingInstance);
    }

    private void StartSoundEvent(EventReference reference, ref EventInstance instance) {
        instance = SoundFXManager.obj.CreateAttachedInstance(reference, gameObject, null);
        instance.start();
        instance.release();
    }

    public void RequestSkip() {
        if(_cutsceneCoroutine != null)
            StopCoroutine(_cutsceneCoroutine);
        if(_increaseParticleSpeedCoroutine != null)
            StopCoroutine(_increaseParticleSpeedCoroutine);
        if(_startSoundEventsCoroutine != null)
            StopCoroutine(_startSoundEventsCoroutine);

        CameraShakeManager.obj.ShakeCamera(0, 0, 0);

        AudioUtils.SafeStop(ref _rumblingInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        AudioUtils.SafeStop(ref _blobTransformSfxInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);

        //Reset camera
        _zoomedOutCamera.SetActive(false);

        Player.obj.AbortFlash();
        Player.obj.SetAnimatorSpeed(1);
        Player.obj.ResetAnimator();
        Player.obj.gameObject.SetActive(false);

        if(PlayerBlob.obj != null) {
            PlayerBlob.obj.ResetAnimator();
            PlayerBlob.obj.gameObject.SetActive(false);
        }

        CaveAvatar.obj.gameObject.SetActive(false);

        _backgroundBlobs.SetActive(false);

        _particleEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        PlayerSwitcher.obj.EnableSwitching();

        GameManager.obj.RegisterEvent(_cutsceneCompleted);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);

        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        _skipCutsceneCamera.SetActive(true);
        yield return null;
        _skipCutsceneCamera.SetActive(false);
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        ShadowTwinMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    private IEnumerator GraduallyIncreaseParticleSpeed(ParticleSystem ps, float startSpeed, float endSpeed, float startLifetime, float endLifetime, float duration)
    {
        var main = ps.main;
        main.startSpeed = startSpeed;
        main.startLifetime = startLifetime;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentSpeed = Mathf.Lerp(startSpeed, endSpeed, t);
            float currentLifetime = Mathf.Lerp(startLifetime, endLifetime, t);

            main.startSpeed = currentSpeed;
            main.startLifetime = currentLifetime;
            yield return null;
        }

        main.startSpeed = endSpeed;
        main.startLifetime = endLifetime;

        ps.Stop();
    }
}
