using System.Collections;
using Cinemachine;
using FMOD.Studio;
using FMODUnity;
using FunkyCode;
using UnityEngine;

public class LowerCrownTrigger : MonoBehaviour, ISkippable
{
    [SerializeField] private GameEventId _crownPicked;
    [SerializeField] private MusicTrack _musicTrack;
    [SerializeField] private CinemachineVirtualCamera _defaultCamera;
    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private LightSprite2D _beamOfLightLight;
    [SerializeField] private GameObject _crown;
    [SerializeField] private Transform _crownMoveTarget;
    [SerializeField] private SpriteRenderer _beamOfLightRenderer;
    [SerializeField] private Transform _finalPlayerPosition;
    [SerializeField] private GameObject _pickCrownTrigger;
    [SerializeField] private GameObject _blobsContainer;
    [Range(0.1f, 10f), SerializeField] private float _fadeSpeed = 5f;
    [SerializeField] private Cave6CapePickedManager _cave6CapePickedManager;
    [SerializeField] private EventReference _lightBeamSfx;
    private Color _fadeStartColor;
    private bool _isTriggered = false;
    private Coroutine _cutsceneCoroutine;
    private EventInstance _lightBeamSfxInstance;

    void Awake() {
        if(GameManager.obj.HasEvent(_crownPicked)) {
            _isTriggered = true;
            Destroy(gameObject, 3);
        }
        _fadeStartColor = new Color(_beamOfLightRenderer.color.r, _beamOfLightRenderer.color.g, _beamOfLightRenderer.color.b, 0);
    }

    private void OnTriggerExit2D(Collider2D collider) {
        if(collider.transform.CompareTag("Player") && !_isTriggered) {
            ShadowTwinMovement.obj.Freeze();
            _cutsceneCoroutine = StartCoroutine(EnterCrownRoomSequence());
            _isTriggered = true;
        }
    }

    private void StartSoundEvent(EventReference reference, ref EventInstance instance) {
        instance = SoundFXManager.obj.CreateAttachedInstance(reference, gameObject, null);
        instance.start();
        instance.release();
    }

    public void RequestSkip() {
        StopCoroutine(_cutsceneCoroutine);

        _pickCrownTrigger.SetActive(false);
        _crown.SetActive(false);
        _blobsContainer.SetActive(false);
        
        ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(true);
        PlayerPowersManager.obj.DeeCanForcePull = true;
        ShadowTwinPlayer.obj.transform.position = _finalPlayerPosition.position;
        ShadowTwinMovement.obj.SetStartingOnGround();
        ShadowTwinMovement.obj.isGrounded = true;
        ShadowTwinMovement.obj.CancelJumping();
        ShadowTwinPlayer.obj.ResetAnimator();
        _beamOfLightLight.gameObject.SetActive(false);

        AudioUtils.SafeStop(ref _lightBeamSfxInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);

        _defaultCamera.enabled = true;
        _zoomedCamera.SetActive(false);
        CinemachineVirtualCamera zoomedCameraVcam = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedCameraVcam.enabled = false;
        _cave6CapePickedManager.FadeOutAndStopAmbience();

        MusicManager.obj.Play(_musicTrack);

        GameManager.obj.RegisterEvent(_crownPicked);
        StartCoroutine(ResumeGameplay());
    }

    private IEnumerator ResumeGameplay() {
        SceneFadeManager.obj.StartFadeIn();
        while(SceneFadeManager.obj.IsFadingIn) {
            yield return null;
        }
        ShadowTwinMovement.obj.UnFreeze();
        GameManager.obj.IsPauseAllowed = true;
        yield return null;
    }

    private IEnumerator EnterCrownRoomSequence() {
        PauseMenuManager.obj.RegisterSkippable(this);
        
        _zoomedCamera.SetActive(true);
        CinemachineVirtualCamera zoomedCameraVcam = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedCameraVcam.enabled = true;
        _defaultCamera.enabled = false;

        _crown.GetComponent<Crown>().StopHover();
        yield return new WaitForSeconds(2.5f);

        StartSoundEvent(_lightBeamSfx, ref _lightBeamSfxInstance);

        while(_beamOfLightRenderer.color.a < 0.65f) {
            _fadeStartColor.a += Time.deltaTime * _fadeSpeed;
            _beamOfLightRenderer.color = _fadeStartColor;
            _beamOfLightLight.color.a = _fadeStartColor.a;
            yield return null;
        }
        yield return new WaitForSeconds(2f);

        //Lower crown
        while(_crown.transform.position != _crownMoveTarget.position) {
            _crown.transform.position = Vector2.MoveTowards(_crown.transform.position, _crownMoveTarget.position, 1.1f * Time.deltaTime);
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

        ShadowTwinMovement.obj.UnFreeze();
        PauseMenuManager.obj.UnregisterSkippable();

        yield return null;
    }
}
