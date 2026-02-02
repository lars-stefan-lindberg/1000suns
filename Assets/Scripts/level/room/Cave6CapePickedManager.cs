using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cave6CapePickedManager : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _capePicked;
    [SerializeField] private GameObject _cape;
    [SerializeField] private List<GameObject> _blobs;
    [SerializeField] private GameObject _tutorialCanvas;
    [SerializeField] private Transform _finalPlayerPosition;
    [SerializeField] private GameObject _blobsContainer;
    [SerializeField] private GameObject _pickCapeTrigger;
    [SerializeField] private float _roarSfxFadeOutDuration = 1f;

    private Coroutine _cutsceneCoroutine;
    private Coroutine _roarSfxCoroutine;
    private AudioSource _roarSfxAudioSource;
    private float _roarSfxStartVolume = 1f;

    public void RequestSkip() {
        StopCoroutine(_cutsceneCoroutine);
        _pickCapeTrigger.SetActive(false);

        CameraShakeManager.obj.ShakeCamera(0, 0, 0);

        WhiteFadeManager.obj.Reset();

        _cape.SetActive(false);
        _blobsContainer.SetActive(false);
        StartCoroutine(FadeOutAndStopAmbience());

        CaveAvatar.obj.SetPosition(_finalPlayerPosition.position);
        CaveAvatar.obj.FollowPlayer();
        CaveAvatar.obj.SetFloatingEnabled(true);
        Player.obj.SetAnimatorLayerAndHasCape(true);
        PlayerPowersManager.obj.EliCanForcePush = true;
        Player.obj.transform.position = _finalPlayerPosition.position;
        Player.obj.ResetAnimator();

        BeginFadeOutRoarSfx();

        MusicManager.obj.PlayCaveFirstSong();

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

    public void Activate() {
        if(GameManager.obj.HasEvent(_capePicked)) 
            return;
        _cutsceneCoroutine = StartCoroutine(StartCutscene());
    }

    private IEnumerator StartCutscene() {
        PauseMenuManager.obj.RegisterSkippable(this);

        PlayerMovement.obj.Freeze();
        StartCoroutine(FadeOutAndStopAmbience());

        StartRoarSfx();

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
        _tutorialCanvas.SetActive(true);
        TutorialDialogManager.obj.StartFadeIn();
        SoundFXManager.obj.PlayPowerUpDialogueStinger();
        while(!TutorialDialogManager.obj.tutorialCompleted) {
            yield return null;
        }
        _tutorialCanvas.SetActive(false);
        Time.timeScale = 1;
        GameManager.obj.IsPauseAllowed = true;

        PlayerMovement.obj.SetNewPowerRecevied();
        yield return new WaitForSeconds(2);
        PlayerMovement.obj.UnFreeze();

        MusicManager.obj.PlayCaveFirstSong();
        
        GameManager.obj.RegisterEvent(_capePicked);

        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    public IEnumerator FadeOutAndStopAmbience() {
        float ambienceVolume = SoundMixerManager.obj.GetAmbienceVolume();
        StartCoroutine(SoundMixerManager.obj.StartAmbienceFade(1f, 0.001f));
        while(SoundMixerManager.obj.GetAmbienceVolume() > 0.001f) {
            yield return null;
        }
        //Give SoundMixerManager time to fully complete the fading
        yield return new WaitForSeconds(0.1f);
        AmbienceManager.obj.StopAmbience();
        SoundMixerManager.obj.SetAmbienceVolume(ambienceVolume);
    }

    private void StartRoarSfx()
    {
        if (SoundFXManager.obj == null)
            return;

        if (_roarSfxCoroutine != null)
        {
            StopCoroutine(_roarSfxCoroutine);
            _roarSfxCoroutine = null;
        }

        if (_roarSfxAudioSource == null || !_roarSfxAudioSource.isPlaying)
        {
            _roarSfxAudioSource = SoundFXManager.obj.PlayCapePickUp(Camera.main.transform);
            if (_roarSfxAudioSource != null)
                _roarSfxStartVolume = _roarSfxAudioSource.volume;
        }

        if (_roarSfxAudioSource != null)
            _roarSfxAudioSource.volume = _roarSfxStartVolume;
    }

    private void BeginFadeOutRoarSfx()
    {
        if (_roarSfxAudioSource == null)
            return;

        if (_roarSfxCoroutine != null) {
            StopCoroutine(_roarSfxCoroutine);
        }
        _roarSfxCoroutine = StartCoroutine(FadeOutAndStop(_roarSfxAudioSource, _roarSfxFadeOutDuration));
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        if (source == null)
            yield break;

        float t = 0f;
        float startVolume = source.volume;
        duration = Mathf.Max(0.01f, duration);

        while (t < duration)
        {
            if (source == null)
                yield break;

            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            source.volume = Mathf.Lerp(startVolume, 0f, a);
            yield return null;
        }

        if (source != null)
        {
            source.volume = 0f;
            source.Stop();
            source.volume = _roarSfxStartVolume;
        }

        if (source == _roarSfxAudioSource)
            _roarSfxCoroutine = null;
    }
}
