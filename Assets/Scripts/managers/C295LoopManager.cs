using System;
using UnityEngine;

public class C295LoopManager : MonoBehaviour
{
    [SerializeField] private Transform _target;
    private AudioSource _loopSource;
    private bool _isPlaying = false;
    private float _originalVolume = 1f;
    private float _pitch1 = 1.1f;
    private float _pitch2 = 1.2f;
    private float _pitch3 = 1.4f;
    private float _distance1 = 19f;
    private float _distance2 = 8f;
    private float _distance3 = 4f;

    // Hysteresis and smoothing
    private float _hysteresisMargin = 2.5f; // Margin for going back left
    private float _targetPitch = 1f;
    private float _pitchLerpSpeed = 0.9f; // Speed of pitch lerp
    private float _lastPlayerDist = 0f;
    private int _pitchState = 0; // 0: normal, 1: pitch1, 2: pitch2, 3: pitch3
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            GetComponent<BoxCollider2D>().enabled = false;
            MusicManager.obj.PlayCaveSpaceRoomLoop();
            if (_loopSource == null)
                _loopSource = MusicManager.obj.GetLoopSource();
            _isPlaying = true;
            _lastPlayerDist = Vector3.Distance(_target.position, PlayerManager.obj.GetPlayerTransform().position);
            _pitchState = 0;
            _targetPitch = 1f;
            if (_loopSource != null)
            {
                _loopSource.pitch = 1f;
                _originalVolume = _loopSource.volume;
                _loopSource.volume = _originalVolume;
            }
        }
    }

    void Update()
    {
        if (_isPlaying && _loopSource != null)
        {
            if(PlayerBlob.obj == null || PlayerBlob.obj.transform == null)
                return;
            float playerDist = 0f;
            try {
                playerDist = Vector3.Distance(_target.position, PlayerBlob.obj.transform.position);
            } catch (Exception e) {
                //Debug.LogException(e);
                return;
            }
            float margin = _hysteresisMargin;

            // Determine direction
            bool movingAway = playerDist > _lastPlayerDist;
            bool movingCloser = playerDist < _lastPlayerDist;

            // Hysteresis pitch logic
            switch (_pitchState)
            {
                case 0: // Normal
                    if (playerDist < _distance1 && movingAway)
                    {
                        _pitchState = 1;
                        _targetPitch = _pitch1;
                    }
                    break;
                case 1: // Pitch1
                    if (playerDist < _distance2 && movingAway)
                    {
                        _pitchState = 2;
                        _targetPitch = _pitch2;
                    }
                    else if (playerDist >= _distance1 + margin && movingCloser)
                    {
                        _pitchState = 0;
                        _targetPitch = 1f;
                    }
                    break;
                case 2: // Pitch2
                    if (playerDist < _distance3 && movingAway)
                    {
                        _pitchState = 3;
                        _targetPitch = _pitch3;
                    }
                    else if (playerDist >= _distance2 + margin && movingCloser)
                    {
                        _pitchState = 1;
                        _targetPitch = _pitch1;
                    }
                    break;
                case 3: // Pitch3
                    if (playerDist >= _distance3 + margin && movingCloser)
                    {
                        _pitchState = 2;
                        _targetPitch = _pitch2;
                    }
                    break;
            }

            // Smooth pitch transition
            _loopSource.pitch = Mathf.Lerp(_loopSource.pitch, _targetPitch, Time.deltaTime * _pitchLerpSpeed);

            if (playerDist <= 0.01f)
            {
                _isPlaying = false;
            }

            _lastPlayerDist = playerDist;
        }
    }
}
