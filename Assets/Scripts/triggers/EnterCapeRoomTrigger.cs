using System.Collections;
using Cinemachine;
using FunkyCode;
using UnityEngine;

public class EnterCapeRoomTrigger : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _defaultCamera;
    [SerializeField] private GameObject _zoomedCamera;
    [SerializeField] private LightSprite2D _beamOfLightLight;
    [SerializeField] private GameObject _cape;
    [SerializeField] private Transform _capeMoveTarget;
    [SerializeField] private SpriteRenderer _beamOfLightRenderer;
    [Range(0.1f, 10f), SerializeField] private float _fadeSpeed = 5f;
    private Color _fadeStartColor;
    private bool _isTriggered = false;

    void Awake() {
        if(GameEventManager.obj.CapePicked) {
            _isTriggered = true;
            Destroy(gameObject, 3);
        }
        _fadeStartColor = new Color(_beamOfLightRenderer.color.r, _beamOfLightRenderer.color.g, _beamOfLightRenderer.color.b, 0);
    }

    private void OnTriggerExit2D(Collider2D collider) {
        if(collider.transform.CompareTag("Player") && !_isTriggered) {
            PlayerMovement.obj.Freeze();
            StartCoroutine(FadeOutAndStopAmbience());
            StartCoroutine(EnterCapeRoomSequence());
            _isTriggered = true;
        }
    }

    private IEnumerator FadeOutAndStopAmbience() {
        float ambienceVolume = SoundMixerManager.obj.GetAmbienceVolume();
        StartCoroutine(SoundMixerManager.obj.StartAmbienceFade(1f, 0.001f));
        while(SoundMixerManager.obj.GetAmbienceVolume() > 0.001f) {
            yield return null;
        }
        AmbienceManager.obj.StopAmbience();
        SoundMixerManager.obj.SetAmbienceVolume(ambienceVolume);
    }

    private IEnumerator EnterCapeRoomSequence() {
        //Zoom in on cape
        _zoomedCamera.SetActive(true);
        CinemachineVirtualCamera zoomedCameraVcam = _zoomedCamera.GetComponent<CinemachineVirtualCamera>();
        zoomedCameraVcam.enabled = true;
        _defaultCamera.enabled = false;

        _cape.GetComponent<Cape>().StopHover();

        yield return new WaitForSeconds(1f);

        //Fade in beam of light
        SoundFXManager.obj.PlayCapeIntroduction(_cape.transform);
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

        yield return new WaitForSeconds(1);

        PlayerMovement.obj.UnFreeze();
    }
}
