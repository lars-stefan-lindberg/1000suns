using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager obj;
    private float _shakeTimer = 0;
    private CinemachineVirtualCamera _activeVirtualCamera;

    [ContextMenu("ShakeCamera")]
    public void ShakeCamera(float amplitude, float frequency, float time) {
        _activeVirtualCamera = (CinemachineVirtualCamera)CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if (_activeVirtualCamera != null) {
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = _activeVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = amplitude;
            cinemachineBasicMultiChannelPerlin.m_FrequencyGain = frequency;
            _shakeTimer = time;
        }
    }

    public void ForcePushShake() {
        _activeVirtualCamera = (CinemachineVirtualCamera)CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if(_activeVirtualCamera.Follow == null) {
            _activeVirtualCamera.VirtualCameraGameObject.transform.DOShakePosition(0.13f, new Vector3(0.15f, 0.15f, 0), 30, 90, false, true, ShakeRandomnessMode.Harmonic);
        } else {
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = _activeVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 4f;
            cinemachineBasicMultiChannelPerlin.m_FrequencyGain = 2f;
            _shakeTimer = 0.13f;
        }
    }

    void Awake() {
        obj = this;
    }

    void Update()
    {
        if(_shakeTimer > 0f) {
            _shakeTimer -= Time.deltaTime;
            if(_shakeTimer < 0f) {
                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = _activeVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0f;
            }
        }
    }

    void OnDestroy() {
        obj = null;
    }
}
