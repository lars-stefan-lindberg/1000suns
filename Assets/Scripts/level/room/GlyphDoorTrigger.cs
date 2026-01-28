using System.Collections;
using UnityEngine;

public class GlyphDoorTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _door;
    [SerializeField] private Transform _doorMovingTransform;
    [SerializeField] private SpriteFlash _glyphFlash;
    private ParticleSystem _doorDust;
    private SpriteRenderer _doorSprite;
    private BoxCollider2D _doorCollider;
    [SerializeField] private float _doorRaiseDistance = 3.5f;
    [SerializeField] private float _doorRaiseSpeed = 2.0f;
    [SerializeField] private float _doorSlamSpeed = 10.0f;
    [SerializeField] private float _closeDustDuration = 0.15f;
    [SerializeField] private float _shakeAmplitude = 0.05f;
    [SerializeField] private float _shakeFrequency = 25.0f;
    [SerializeField] private float _earthquakeFadeOutDuration = 0.25f;
    [SerializeField] private float _openCameraShakeAmplitude = 1.0f;
    [SerializeField] private float _openCameraShakeFrequency = 1.0f;

    private Vector3 _doorStartPos;
    private Coroutine _sequenceRoutine;
    private Coroutine _earthquakeFadeRoutine;
    private AudioSource _earthquakeSource;
    private float _earthquakeStartVolume = 1f;
    private bool _playerInside;

    private enum DoorState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    private DoorState _doorState = DoorState.Closed;

    void Awake()
    {
        if (_door != null)
        {
            _doorDust = _door.GetComponentInChildren<ParticleSystem>();
            _doorSprite = _door.GetComponentInChildren<SpriteRenderer>();
            _doorCollider = _door.GetComponentInChildren<BoxCollider2D>();
        }

        if (_doorMovingTransform == null)
        {
            if (_doorSprite != null && _doorCollider != null && _doorSprite.transform == _doorCollider.transform)
                _doorMovingTransform = _doorSprite.transform;
            else if (_doorSprite != null)
                _doorMovingTransform = _doorSprite.transform;
            else if (_doorCollider != null)
                _doorMovingTransform = _doorCollider.transform;
            else if (_door != null)
                _doorMovingTransform = _door.transform;
        }

        if (_doorMovingTransform != null)
            _doorStartPos = _doorMovingTransform.position;

        if (_doorDust != null)
            _doorDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        _playerInside = true;

        if (_sequenceRoutine != null)
            StopCoroutine(_sequenceRoutine);

        _sequenceRoutine = StartCoroutine(OpenDoorSequence());
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        _playerInside = false;

        if (_sequenceRoutine != null)
            StopCoroutine(_sequenceRoutine);

        if (_doorState == DoorState.Opening)
        {
            BeginFadeOutEarthquake();
            StopOpenCameraShake();
        }

        _sequenceRoutine = StartCoroutine(CloseDoorSequence());
    }

    private IEnumerator OpenDoorSequence()
    {
        if (_doorMovingTransform == null)
            yield break;

        _doorState = DoorState.Opening;
        StartEarthquake();

        if (_doorDust != null)
            _doorDust.Play();

        float raiseSpeed = Mathf.Max(0.01f, _doorRaiseSpeed);
        Vector3 startPos = _doorStartPos;
        Vector3 targetPos = startPos + Vector3.up * _doorRaiseDistance;

        float duration = Mathf.Abs(_doorRaiseDistance) / raiseSpeed;
        duration = Mathf.Max(0.01f, duration);

        StartOpenCameraShake(duration);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 basePos = Vector3.Lerp(startPos, targetPos, t);

            float time = Time.time * _shakeFrequency;
            float noiseX = (Mathf.PerlinNoise(time, 0.1234f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(0.5678f, time) - 0.5f) * 2f;
            Vector3 shakeOffset = new Vector3(noiseX, noiseY, 0f) * _shakeAmplitude;

            _doorMovingTransform.position = basePos + shakeOffset;
            yield return null;
        }

        _doorMovingTransform.position = targetPos;

        BeginFadeOutEarthquake();
        _doorState = DoorState.Open;

        if (_doorDust != null)
            _doorDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        _sequenceRoutine = null;
    }

    private void StartOpenCameraShake(float duration)
    {
        if (CameraShakeManager.obj == null)
            return;

        CameraShakeManager.obj.ShakeCamera(_openCameraShakeAmplitude, _openCameraShakeFrequency, duration);
    }

    private void StopOpenCameraShake()
    {
        if (CameraShakeManager.obj == null)
            return;

        CameraShakeManager.obj.ShakeCamera(0f, 0f, 0f);
    }

    private IEnumerator CloseDoorSequence()
    {
        if (_doorMovingTransform == null)
            yield break;

        _doorState = DoorState.Closing;

        if (_doorDust != null)
            _doorDust.Play();

        float slamSpeed = Mathf.Max(0.01f, _doorSlamSpeed);
        Vector3 startPos = _doorMovingTransform.position;
        Vector3 targetPos = _doorStartPos;

        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / slamSpeed;
        duration = Mathf.Max(0.01f, duration);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _doorMovingTransform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        _doorMovingTransform.position = targetPos;

        _doorState = DoorState.Closed;

        if (CameraShakeManager.obj != null)
            CameraShakeManager.obj.ForcePushShake();

        if (_doorDust != null)
        {
            yield return new WaitForSeconds(_closeDustDuration);
            _doorDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        _sequenceRoutine = null;
    }

    private void StartEarthquake()
    {
        if (SoundFXManager.obj == null)
            return;

        if (_earthquakeFadeRoutine != null)
        {
            StopCoroutine(_earthquakeFadeRoutine);
            _earthquakeFadeRoutine = null;
        }

        if (_earthquakeSource == null || !_earthquakeSource.isPlaying)
        {
            _earthquakeSource = SoundFXManager.obj.PlayEarthquake();
            if (_earthquakeSource != null)
                _earthquakeStartVolume = _earthquakeSource.volume;
        }

        if (_earthquakeSource != null)
            _earthquakeSource.volume = _earthquakeStartVolume;
    }

    private void BeginFadeOutEarthquake()
    {
        if (_earthquakeSource == null)
            return;

        if (_earthquakeFadeRoutine != null)
            StopCoroutine(_earthquakeFadeRoutine);

        _earthquakeFadeRoutine = StartCoroutine(FadeOutAndStop(_earthquakeSource, _earthquakeFadeOutDuration));
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
            source.volume = _earthquakeStartVolume;
        }

        if (source == _earthquakeSource)
            _earthquakeFadeRoutine = null;
    }
}
