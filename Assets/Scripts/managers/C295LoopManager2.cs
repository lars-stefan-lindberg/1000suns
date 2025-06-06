using System;
using UnityEngine;

public class C295LoopManager2 : MonoBehaviour
{
    [SerializeField] private Transform _target;
    private AudioSource _loopSource;
    private AudioLowPassFilter _lowPassFilter;
    private bool _isPlaying = false;

    private float _distance1 = 31f;
    private float _distance2 = 17f;
    private float _distance3 = 11f;
    private float _distance4 = 6f;
    private float _distance5 = 3f;

    private int _currentVolumeLevel = 0;
    private float _lastPlayerDist = -1f; // For tracking direction
    private const float _distanceOffset = 3f; // Margin for leftward transitions

    private float _targetVolume = 0f;
    private float _volumeLerpTime = 0f;
    private float _volumeLerpDuration = 3.0f;
    private float _volumeStartValue = 0f;


    private float _targetCutoff = 22000f;
    private float _cutoffLerpTime = 0f;
    private float _cutoffLerpDuration = 3.0f; // seconds to ramp
    private float _cutoffStartValue = 22000f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            GetComponent<BoxCollider2D>().enabled = false;
            MusicManager.obj.PlayCaveSpaceRoomInfiniteLoop();
            if (_loopSource == null)
            {
                _loopSource = MusicManager.obj.GetLoopSource();
                if (_loopSource != null)
                {
                    _lowPassFilter = _loopSource.GetComponent<AudioLowPassFilter>(); //Assumption that low pass filter has already been added to _loopSource
                }
            }
            _isPlaying = true;
            if (_loopSource != null)
            {
                _loopSource.volume = 0f; // Start at 0 volume
                _targetVolume = 0f;
                _volumeStartValue = 0f;
                _volumeLerpTime = 0f;

                if (_lowPassFilter != null)
                {
                    _lowPassFilter.cutoffFrequency = 200f; // Start fully closed
                    _targetCutoff = 200f;
                    _cutoffStartValue = 200f;
                    _cutoffLerpTime = 0f;
                }
            }
            _currentVolumeLevel = 0;
        }
    }

    void Update()
    {
        if (_isPlaying)
        {
            // Always get the current loop source from MusicManager
            AudioSource currentSource = MusicManager.obj.GetLoopSource();
            if (currentSource != _loopSource)
            {
                _loopSource = currentSource;
                if (_loopSource != null)
                {
                    _lowPassFilter = _loopSource.GetComponent<AudioLowPassFilter>();
                }
            }

            if (_loopSource == null) return;


            if(PlayerBlob.obj == null || PlayerBlob.obj.transform == null)
                return;
            float playerDist = 0f;
            try {
                playerDist = Vector3.Distance(_target.position, PlayerBlob.obj.transform.position);
            } catch (Exception) {
                return;
            }

            if (playerDist <= 0.5f)
            {
                Debug.Log("Player is too close to loop source");
                _isPlaying = false;
            }

            // --- Hysteresis logic for volume/cutoff transitions ---
            int newVolumeLevel = _currentVolumeLevel;

            // Determine direction
            bool movingRight = (_lastPlayerDist < 0f) || (playerDist < _lastPlayerDist); // right == getting closer
            bool movingLeft = (_lastPlayerDist >= 0f) && (playerDist > _lastPlayerDist); // left == getting farther

            // Thresholds for each level
            float[] thresholds = {_distance1, _distance2, _distance3, _distance4, _distance5};

            // Going right: use normal thresholds (increase level)
            if (movingRight) {
                if (playerDist <= _distance5 && _currentVolumeLevel < 5)
                    newVolumeLevel = 5;
                else if (playerDist <= _distance4 && _currentVolumeLevel < 4)
                    newVolumeLevel = 4;
                else if (playerDist <= _distance3 && _currentVolumeLevel < 3)
                    newVolumeLevel = 3;
                else if (playerDist <= _distance2 && _currentVolumeLevel < 2)
                    newVolumeLevel = 2;
                else if (playerDist <= _distance1 && _currentVolumeLevel < 1)
                    newVolumeLevel = 1;
            }
            // Going left: use thresholds + offset (decrease level)
            else if (movingLeft) {
                if (playerDist > _distance1 + _distanceOffset && _currentVolumeLevel > 0)
                    newVolumeLevel = 0;
                else if (playerDist > _distance2 + _distanceOffset && _currentVolumeLevel > 1)
                    newVolumeLevel = 1;
                else if (playerDist > _distance3 + _distanceOffset && _currentVolumeLevel > 2)
                    newVolumeLevel = 2;
                else if (playerDist > _distance4 + _distanceOffset && _currentVolumeLevel > 3)
                    newVolumeLevel = 3;
                else if (playerDist > _distance5 + _distanceOffset && _currentVolumeLevel > 4)
                    newVolumeLevel = 4;
            }

            _lastPlayerDist = playerDist;

            if (newVolumeLevel != _currentVolumeLevel)
            {
                _currentVolumeLevel = newVolumeLevel;
                
                float[] volumeLevels = {0f, 0.1f, 0.2f, 0.4f, 0.6f, 1f};
                //float[] cutoffLevels = {200f, 820f, 1440f, 2060f, 2680f, 3300f};
                float[] cutoffLevels = {800f, 3280f, 5760f, 8240f, 10720f, 15000f};
                _volumeStartValue = _loopSource.volume;
                _targetVolume = volumeLevels[_currentVolumeLevel];
                _volumeLerpTime = 0f;
                if (_lowPassFilter != null)
                {
                    _cutoffStartValue = _lowPassFilter.cutoffFrequency;
                    _targetCutoff = cutoffLevels[_currentVolumeLevel];
                    _cutoffLerpTime = 0f;
                }
            }

            // Smoothly interpolate volume to target (logarithmic curve)
            if (Mathf.Abs(_loopSource.volume - _targetVolume) > 0.001f)
            {
                _volumeLerpTime += Time.deltaTime;
                float t = Mathf.Clamp01(_volumeLerpTime / _volumeLerpDuration);
                // Logarithmic interpolation: ease in
                float logT = Mathf.Log10(9 * t + 1); // log10(1) = 0, log10(10) = 1
                float newVolume = Mathf.Lerp(_volumeStartValue, _targetVolume, logT);
                _loopSource.volume = newVolume;
            } else {
                _loopSource.volume = _targetVolume;
            }

            // Smoothly interpolate low-pass filter
            if (_lowPassFilter != null && Mathf.Abs(_lowPassFilter.cutoffFrequency - _targetCutoff) > 1f)
            {
                _cutoffLerpTime += Time.deltaTime;
                float t = Mathf.Clamp01(_cutoffLerpTime / _cutoffLerpDuration);
                float logT = Mathf.Log10(9 * t + 1);
                float newCutoff = Mathf.Lerp(_cutoffStartValue, _targetCutoff, logT);
                _lowPassFilter.cutoffFrequency = newCutoff;
            } else if (_lowPassFilter != null) {
                _lowPassFilter.cutoffFrequency = _targetCutoff;
            }
        }
    }
}
