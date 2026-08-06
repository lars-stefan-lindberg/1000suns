using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class ShockwaveColliderEmitter : MonoBehaviour
{
    public GameObject shockwavePrefab;

    [SerializeField] private EventReference _statueShockWaveSfx;
    [SerializeField] private SpriteRenderer _ghostRenderer;
    [SerializeField] private bool _isContinuous = true;
    [SerializeField] private bool _isDynamic = true;

    [Header("Shockwave Settings")]
    public float shockwaveTime = 0.8f;
    public float startPosition = 0.05f;
    public float endPosition = 1f;
    [Header("Shockwave Timing Settings")]
    public float minInterval = 0.5f;
    public float maxInterval = 2.0f;
    public float minPlayerDistance = 3.0f;
    public float maxPlayerDistance = 15.0f;
    public float minForce = 20f;
    public float maxForce = 60f;
    private float timer = 0f;

    [Header("Ghost Scale Animation")]
    public float scaleMultiplier = 1.2f;
    public float scaleDuration = 0.15f;
    public Ease scaleEase = Ease.OutQuad;

    private Transform _activePlayerTransform;
    private GameObject _currentShockwave;
    private EventInstance _statueShockWaveSfxInstance;

    void Start() {
        PlayerManager.PlayerType activePlayerType = PlayerManager.obj.GetActivePlayerType();
        _activePlayerTransform = PlayerManager.obj.GetPlayerTransform(activePlayerType);
    }

    private float GetDynamicInterval(float playerDist)
    {
        if (playerDist >= maxPlayerDistance)
            return maxInterval;
        if (playerDist <= minPlayerDistance)
            return 0f; // special value: disables shockwave
        float t = (playerDist - minPlayerDistance) / (maxPlayerDistance - minPlayerDistance);
        return Mathf.Lerp(minInterval, maxInterval, t);
    }

    private float GetDynamicForce(float playerDist)
    {
        if (playerDist >= maxPlayerDistance)
            return maxForce;
        if (playerDist <= minPlayerDistance)
            return 0f; // special value: disables shockwave
        float t = (playerDist - minPlayerDistance) / (maxPlayerDistance - minPlayerDistance);
        return Mathf.Lerp(minForce, maxForce, t);
    }

    [ContextMenu("Trigger Shockwave")]
    public void TriggerShockwave(float playerDist)
    {
        ShockWaveManager.obj.CallBigShockWave(transform.position, shockwaveTime, startPosition, endPosition);

        _statueShockWaveSfxInstance = SoundFXManager.obj.CreateAttachedInstance(_statueShockWaveSfx, gameObject);
        _statueShockWaveSfxInstance.start();
        _statueShockWaveSfxInstance.release();

        CameraShakeManager.obj.ForcePushShake();

        if (_ghostRenderer != null)
        {
            _ghostRenderer.transform.DOScale(_ghostRenderer.transform.localScale * scaleMultiplier, scaleDuration)
                .SetEase(scaleEase)
                .OnComplete(() => _ghostRenderer.transform.DOScale(Vector3.one, scaleDuration).SetEase(scaleEase));
        }

        float force = 0f;
        if(_isDynamic)
            force = GetDynamicForce(playerDist);
        else
            force = maxForce;

        _currentShockwave = Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
        var shockwave = _currentShockwave.GetComponent<ShockwaveCollider>();
        if (shockwave != null)
            shockwave.force = force;
    }

    public void AbortShockwave() {
        if(_currentShockwave != null) 
            Destroy(_currentShockwave);
        CameraShakeManager.obj.ShakeCamera(0, 0, 0);
        AudioUtils.SafeStop(ref _statueShockWaveSfxInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
        ShockWaveManager.obj.DestroyShockwave();
    }

    void FixedUpdate()
    {
        if(!_isContinuous)
            return;
            
        float playerDist = Vector3.Distance(transform.position, _activePlayerTransform.position);
        float dynamicInterval = GetDynamicInterval(playerDist);
        if (dynamicInterval == 0f)
        {
            timer = 0f; // Reset timer if player is too close
            return;
        }
        timer += Time.deltaTime;
        if (timer >= dynamicInterval)
        {
            TriggerShockwave(playerDist);
            timer = 0f;
        }
    }
}
