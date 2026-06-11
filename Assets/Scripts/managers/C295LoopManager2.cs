using System;
using UnityEngine;
using FMOD.Studio;

public class C295LoopManager2 : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private MusicTrack _loop;
    
    [Header("Initial Volume Fade")]
    [SerializeField] private float _initialVolumeFadeSpeed = 0.5f;
    [SerializeField] private float _initialVolumeTarget = 0.3f;
    
    [Header("Distance Thresholds")]
    [SerializeField] private float _volumeFadeStartDistance = 30f;
    [SerializeField] private float _volumeFadeEndDistance = 15f;
    [SerializeField] private float _lowPassFadeStartDistance = 15f;
    [SerializeField] private float _targetReachedDistance = 0.5f;
    
    private Vector3 _startPosition;
    private bool _musicStarted = false;
    private PARAMETER_ID _fadeParamId;
    private PARAMETER_ID _fadeLowPassParamId;
    private bool _parametersInitialized = false;
    private float _currentInitialVolume = 0f;
    private bool _initialFadeComplete = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            GetComponent<BoxCollider2D>().enabled = false;
            _startPosition = transform.position;
            MusicManager.obj.Play(_loop);
            _musicStarted = true;
            _currentInitialVolume = 0f;
            _initialFadeComplete = false;
            InitializeParameters();
        }
    }

    void Update()
    {
        if (!_musicStarted)
            return;
            
        if(PlayerBlob.obj == null || PlayerBlob.obj.transform == null)
            return;
            
        if (!_parametersInitialized)
            InitializeParameters();
        
        if (!_initialFadeComplete)
        {
            _currentInitialVolume += _initialVolumeFadeSpeed * Time.deltaTime;
            if (_currentInitialVolume >= _initialVolumeTarget)
            {
                _currentInitialVolume = _initialVolumeTarget;
                _initialFadeComplete = true;
            }
        }
            
        float playerDist = 0f;
        try {
            playerDist = Vector3.Distance(_target.position, PlayerBlob.obj.transform.position);
        } catch (Exception) {
            return;
        }

        UpdateMusicParameters(playerDist);
        
        if (playerDist <= _targetReachedDistance)
        {
            _musicStarted = false;
        }
    }
    
    private void InitializeParameters()
    {
        if (MusicManager.obj == null || MusicManager.obj.CurrentTrack != _loop)
            return;
            
        try
        {
            var instance = MusicManager.obj.CurrentInstance;
            if (!instance.isValid())
                return;
            
            instance.getDescription(out var desc);
            desc.getParameterDescriptionByName("fade", out var fadeParamDesc);
            desc.getParameterDescriptionByName("fadeLowPass", out var fadeLowPassParamDesc);
            
            _fadeParamId = fadeParamDesc.id;
            _fadeLowPassParamId = fadeLowPassParamDesc.id;
            
            _parametersInitialized = true;
            
            instance.setParameterByID(_fadeParamId, 0f);
            instance.setParameterByID(_fadeLowPassParamId, 0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to initialize FMOD parameters: {e.Message}");
        }
    }
    
    private void UpdateMusicParameters(float distanceToTarget)
    {
        if (!_parametersInitialized)
            return;
            
        float fadeValue = 0f;
        float fadeLowPassValue = 0f;
        
        if (distanceToTarget >= _volumeFadeStartDistance)
        {
            fadeValue = _currentInitialVolume;
            fadeLowPassValue = 0f;
        }
        else if (distanceToTarget > _volumeFadeEndDistance)
        {
            float t = 1f - ((distanceToTarget - _volumeFadeEndDistance) / (_volumeFadeStartDistance - _volumeFadeEndDistance));
            fadeValue = Mathf.Lerp(_currentInitialVolume, 1f, t);
            fadeLowPassValue = 0f;
        }
        else if (distanceToTarget > _targetReachedDistance)
        {
            fadeValue = 1f;
            float t = 1f - ((distanceToTarget - _targetReachedDistance) / (_lowPassFadeStartDistance - _targetReachedDistance));
            fadeLowPassValue = Mathf.Clamp01(t);
        }
        else
        {
            fadeValue = 1f;
            fadeLowPassValue = 1f;
        }
        
        SetParameter(_fadeParamId, fadeValue);
        SetParameter(_fadeLowPassParamId, fadeLowPassValue);
    }
    
    private void SetParameter(PARAMETER_ID paramId, float value)
    {
        try
        {
            if (MusicManager.obj != null && MusicManager.obj.CurrentTrack == _loop)
            {
                var instance = MusicManager.obj.CurrentInstance;
                if (instance.isValid())
                {
                    instance.setParameterByID(paramId, value);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to set FMOD parameter: {e.Message}");
        }
    }

    public void CleanUp()
    {
        _musicStarted = false;
        _parametersInitialized = false;
        _initialFadeComplete = false;
        _currentInitialVolume = 0f;
    }
}
