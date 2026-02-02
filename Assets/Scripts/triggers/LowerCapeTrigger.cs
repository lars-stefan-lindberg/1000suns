using System.Collections;
using Cinemachine;
using FunkyCode;
using UnityEngine;

public class LowerCapeTrigger : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _capePicked;
    [SerializeField] private CinemachineVirtualCamera _defaultCamera;
    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private LightSprite2D _beamOfLightLight;
    [SerializeField] private GameObject _cape;
    [SerializeField] private Transform _capeMoveTarget;
    [SerializeField] private SpriteRenderer _beamOfLightRenderer;
    [SerializeField] private Transform _finalPlayerPosition;
    [SerializeField] private GameObject _pickCapeTrigger;
    [SerializeField] private GameObject _blobsContainer;
    [Range(0.1f, 10f), SerializeField] private float _fadeSpeed = 5f;
    [SerializeField] private float _capeIntroductionSfxFadeOutDuration = 0.5f;
    [SerializeField] private Cave6CapePickedManager _cave6CapePickedManager;
    private Color _fadeStartColor;
    private bool _isTriggered = false;
    private Coroutine _cutsceneCoroutine;
    private Coroutine _capeIntroductionSfxCoroutine;
    private AudioSource _capeIntroductionSfxAudioSource;
    private float _capeIntroductionSfxStartVolume = 1f;

    void Awake() {
        if(GameManager.obj.HasEvent(_capePicked)) {
            _isTriggered = true;
            Destroy(gameObject, 3);
        }
        _fadeStartColor = new Color(_beamOfLightRenderer.color.r, _beamOfLightRenderer.color.g, _beamOfLightRenderer.color.b, 0);
    }

    private void OnTriggerExit2D(Collider2D collider) {
        if(collider.transform.CompareTag("Player") && !_isTriggered) {
            PlayerMovement.obj.Freeze();
            _cutsceneCoroutine = StartCoroutine(EnterCapeRoomSequence());
            _isTriggered = true;
        }
    }

    public void RequestSkip() {
        StopCoroutine(_cutsceneCoroutine);

        _pickCapeTrigger.SetActive(false);
        _cape.SetActive(false);
        _blobsContainer.SetActive(false);
        
        CaveAvatar.obj.SetPosition(_finalPlayerPosition.position);
        CaveAvatar.obj.FollowPlayer();
        CaveAvatar.obj.SetFloatingEnabled(true);
        Player.obj.SetAnimatorLayerAndHasCape(true);
        PlayerPowersManager.obj.EliCanForcePush = true;
        Player.obj.transform.position = _finalPlayerPosition.position;
        Player.obj.ResetAnimator();
        _beamOfLightLight.gameObject.SetActive(false);
        BeginFadeOutCapeIntroductionSfx();

        _defaultCamera.enabled = true;
        _zoomedCamera.SetActive(false);
        CinemachineVirtualCamera zoomedCameraVcam = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedCameraVcam.enabled = false;
        StartCoroutine(_cave6CapePickedManager.FadeOutAndStopAmbience());

        MusicManager.obj.PlayCaveFirstSong();

        GameManager.obj.RegisterEvent(_capePicked);
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

    private IEnumerator EnterCapeRoomSequence() {
        PauseMenuManager.obj.RegisterSkippable(this);
        
        _zoomedCamera.SetActive(true);
        CinemachineVirtualCamera zoomedCameraVcam = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedCameraVcam.enabled = true;
        _defaultCamera.enabled = false;

        yield return new WaitForSeconds(2.5f);

        StartCapeIntroductionSfx();

        while(_beamOfLightRenderer.color.a < 0.65f) {
            _fadeStartColor.a += Time.deltaTime * _fadeSpeed;
            _beamOfLightRenderer.color = _fadeStartColor;
            _beamOfLightLight.color.a = _fadeStartColor.a;
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        _cape.GetComponent<Cape>().StartAnimation();
        yield return new WaitForSeconds(2f);

        //Lower cape
        while(_cape.transform.position != _capeMoveTarget.position) {
            _cape.transform.position = Vector2.MoveTowards(_cape.transform.position, _capeMoveTarget.position, 1.1f * Time.deltaTime);
            yield return null;
        }
        
        yield return new WaitForSeconds(3);

        //Fade out beam
        while(_beamOfLightRenderer.color.a > 0f) {
            _fadeStartColor.a -= Time.deltaTime * _fadeSpeed;
            _beamOfLightRenderer.color = _fadeStartColor;
            _beamOfLightLight.color.a = _fadeStartColor.a;
            yield return null;
        }

        //Zoom out
        _defaultCamera.enabled = true;
        zoomedCameraVcam.enabled = false;

        yield return new WaitForSeconds(2);

        PlayerMovement.obj.UnFreeze();
        PauseMenuManager.obj.UnregisterSkippable();

        yield return null;
    }

    private void StartCapeIntroductionSfx()
    {
        if (SoundFXManager.obj == null)
            return;

        if (_capeIntroductionSfxCoroutine != null)
        {
            StopCoroutine(_capeIntroductionSfxCoroutine);
            _capeIntroductionSfxCoroutine = null;
        }

        if (_capeIntroductionSfxAudioSource == null || !_capeIntroductionSfxAudioSource.isPlaying)
        {
            _capeIntroductionSfxAudioSource = SoundFXManager.obj.PlayCapeIntroduction(_cape.transform);
            if (_capeIntroductionSfxAudioSource != null)
                _capeIntroductionSfxStartVolume = _capeIntroductionSfxAudioSource.volume;
        }

        if (_capeIntroductionSfxAudioSource != null)
            _capeIntroductionSfxAudioSource.volume = _capeIntroductionSfxStartVolume;
    }

    private void BeginFadeOutCapeIntroductionSfx()
    {
        if (_capeIntroductionSfxAudioSource == null)
            return;

        if (_capeIntroductionSfxCoroutine != null)
            StopCoroutine(_capeIntroductionSfxCoroutine);

        _capeIntroductionSfxCoroutine = StartCoroutine(FadeOutAndStop(_capeIntroductionSfxAudioSource, _capeIntroductionSfxFadeOutDuration));
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
            source.volume = _capeIntroductionSfxStartVolume;
        }

        if (source == _capeIntroductionSfxAudioSource)
            _capeIntroductionSfxCoroutine = null;
    }
}
