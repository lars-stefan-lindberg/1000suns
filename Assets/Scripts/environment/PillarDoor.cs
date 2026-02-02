using System.Collections;
using UnityEngine;

public class PillarDoor : MonoBehaviour
{
    [SerializeField] private Transform _doorMovingTransform;
    [SerializeField] private ParticleSystem _doorCaveDust;
    [SerializeField] private ParticleSystem _doorFloorDust;
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
    [SerializeField] private float _floorDustDuration = 0.15f;

    private Vector3 _doorStartPos;
    private Coroutine _sequenceRoutine;
    private Coroutine _earthquakeFadeRoutine;
    private Coroutine _floorDustRoutine;
    private AudioSource _earthquakeSource;
    private float _earthquakeStartVolume = 1f;

    private enum DoorState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    private DoorState _doorState = DoorState.Closed;

    private bool TryKillPlayerIfCrushed()
    {
        if (_doorCollider == null)
            return false;

        if (PlayerManager.obj == null || Reaper.obj == null)
            return false;

        PlayerManager.PlayerType playerType = PlayerManager.obj.GetActivePlayerType();
        if (!PlayerManager.obj.IsPlayerGrounded(playerType))
            return false;

        Transform playerTransform = PlayerManager.obj.GetPlayerTransform(playerType);
        if (playerTransform == null)
            return false;

        Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
        if (playerCollider == null)
            return false;

        ColliderDistance2D colliderDistance = Physics2D.Distance(_doorCollider, playerCollider);
        float touchTolerance = 0.01f;
        if (!colliderDistance.isOverlapped && colliderDistance.distance > touchTolerance)
            return false;

        Reaper.obj.KillPlayerGeneric(playerType);
        return true;
    }

    void Awake()
    {
        _doorSprite = GetComponentInChildren<SpriteRenderer>();
        _doorCollider = GetComponentInChildren<BoxCollider2D>();

        if (_doorMovingTransform == null)
        {
            if (_doorSprite != null && _doorCollider != null && _doorSprite.transform == _doorCollider.transform)
                _doorMovingTransform = _doorSprite.transform;
            else if (_doorSprite != null)
                _doorMovingTransform = _doorSprite.transform;
            else if (_doorCollider != null)
                _doorMovingTransform = _doorCollider.transform;
            else
                _doorMovingTransform = transform;
        }

        if (_doorMovingTransform != null)
            _doorStartPos = _doorMovingTransform.position;

        if (_doorCaveDust != null)
            _doorCaveDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (_doorFloorDust != null)
            _doorFloorDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void Open()
    {
        if (_doorState == DoorState.Opening)
            return;

        if (_sequenceRoutine != null)
            StopCoroutine(_sequenceRoutine);

        _sequenceRoutine = StartCoroutine(OpenDoorSequence());
    }

    public void Close()
    {
        if (_doorState == DoorState.Closed)
            return;

        if (_doorState == DoorState.Closing)
            return;

        if (_sequenceRoutine != null)
            StopCoroutine(_sequenceRoutine);

        if (_doorState == DoorState.Opening)
        {
            BeginFadeOutEarthquake();
        }

        _sequenceRoutine = StartCoroutine(CloseDoorSequence());
    }

    private IEnumerator OpenDoorSequence()
    {
        if (_doorMovingTransform == null)
            yield break;

        _doorState = DoorState.Opening;
        StartEarthquake();

        if (_doorCaveDust != null)
            _doorCaveDust.Play();

        float raiseSpeed = Mathf.Max(0.01f, _doorRaiseSpeed);
        Vector3 startPos = _doorMovingTransform.position;
        Vector3 targetPos = _doorStartPos + Vector3.up * _doorRaiseDistance;

        float duration = Mathf.Abs(_doorRaiseDistance) / raiseSpeed;
        duration = Mathf.Max(0.01f, duration);

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

        if (_doorCaveDust != null)
            _doorCaveDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        _sequenceRoutine = null;
    }

    private IEnumerator StopFloorDustAfter(float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        yield return new WaitForSeconds(duration);

        if (_doorFloorDust != null)
            _doorFloorDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        _floorDustRoutine = null;
    }

    private IEnumerator CloseDoorSequence()
    {
        if (_doorMovingTransform == null)
            yield break;

        _doorState = DoorState.Closing;

        if (_doorCaveDust != null)
            _doorCaveDust.Play();

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

            if (TryKillPlayerIfCrushed())
            {
                _sequenceRoutine = null;
                yield break;
            }

            yield return null;
        }

        _doorMovingTransform.position = targetPos;

        if (_doorFloorDust != null)
        {
            _doorFloorDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _doorFloorDust.Play();

            if (_floorDustRoutine != null)
                StopCoroutine(_floorDustRoutine);
            _floorDustRoutine = StartCoroutine(StopFloorDustAfter(_floorDustDuration));
        }

        if (TryKillPlayerIfCrushed())
        {
            _sequenceRoutine = null;
            yield break;
        }

        _doorState = DoorState.Closed;

        if (CameraShakeManager.obj != null)
            CameraShakeManager.obj.ForcePushShake();

        if (_doorCaveDust != null)
        {
            yield return new WaitForSeconds(_closeDustDuration);
            _doorCaveDust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
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
