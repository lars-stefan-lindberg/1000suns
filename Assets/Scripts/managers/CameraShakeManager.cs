using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager obj;
    private float _shakeTimer = 0;
    private CinemachineVirtualCamera _activeVirtualCamera;

    [ContextMenu("ShakeCamera")]
    public void ShakeCamera(float intensity, float time) {
        _activeVirtualCamera = (CinemachineVirtualCamera)CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if (_activeVirtualCamera != null) {
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = _activeVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
            _shakeTimer = time;
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
