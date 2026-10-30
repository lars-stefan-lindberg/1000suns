using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager obj;
    private const float PushShakeAmplitude = 4f;
    private const float PushShakeFrequency = 2f;
    private const float PushShakeDuration = 0.13f;

    private float _shakeTimer = 0;
    private float _shakeAmplitude;
    private float _shakeFrequency;
    // Short push shakes play on top of a longer shake, which resumes afterwards
    private float _pushShakeTimer = 0;
    private CinemachineVirtualCamera _activeVirtualCamera;
    private CinemachineBasicMultiChannelPerlin _cachedPerlin;

    private CinemachineVirtualCamera GetActiveVirtualCamera() {
        return (CinemachineVirtualCamera)CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
    }

    [ContextMenu("ShakeCamera")]
    public void ShakeCamera(float amplitude, float frequency, float time) {
        _activeVirtualCamera = GetActiveVirtualCamera();
        if (_activeVirtualCamera != null) {
            _cachedPerlin = _activeVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            _shakeAmplitude = amplitude;
            _shakeFrequency = frequency;
            _shakeTimer = time;
            // A zero-length shake is used to cancel all shaking
            if (time <= 0f) _pushShakeTimer = 0f;
            ApplyShake();
        }
    }

    public void ForcePushShake() {
        _activeVirtualCamera = GetActiveVirtualCamera();
        if(_activeVirtualCamera.Follow == null) {
            _activeVirtualCamera.VirtualCameraGameObject.transform.DOShakePosition(0.13f, new Vector3(0.15f, 0.15f, 0), 30, 90, false, true, ShakeRandomnessMode.Harmonic);
        } else {
            _cachedPerlin = _activeVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            _pushShakeTimer = PushShakeDuration;
            ApplyShake();
        }
    }

    void Awake() {
        obj = this;
    }

    void Update()
    {
        if(_shakeTimer <= 0f && _pushShakeTimer <= 0f) return;
        _shakeTimer = Mathf.Max(0f, _shakeTimer - Time.deltaTime);
        _pushShakeTimer = Mathf.Max(0f, _pushShakeTimer - Time.deltaTime);
        ApplyShake();
    }

    private void ApplyShake() {
        if(_cachedPerlin == null) return;
        if(_pushShakeTimer > 0f) {
            _cachedPerlin.m_AmplitudeGain = PushShakeAmplitude;
            _cachedPerlin.m_FrequencyGain = PushShakeFrequency;
        } else if(_shakeTimer > 0f) {
            _cachedPerlin.m_AmplitudeGain = _shakeAmplitude;
            _cachedPerlin.m_FrequencyGain = _shakeFrequency;
        } else {
            _cachedPerlin.m_AmplitudeGain = 0f;
        }
    }

    void OnDestroy() {
        obj = null;
    }
}
