using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager obj;

    [SerializeField] private AudioSource musicObject;
    [SerializeField] private float crossfadeDuration = 1.5f;
    [SerializeField] private AnimationCurve fadeCurve;

    [SerializeField] private AudioClip _titleSongIntro;
    [SerializeField] private AudioClip _titleSongLoop;

    [SerializeField] private AudioClip _caveSongIntro;
    [SerializeField] private AudioClip _caveSongLoop;    
    [SerializeField] private AudioClip _caveSongOutro;    
    [SerializeField] private AudioClip _caveSongFirstIntro;
    [SerializeField] private AudioClip _caveSongFirstLoop;    
    [SerializeField] private AudioClip _caveSongSecondIntro;
    [SerializeField] private AudioClip _caveSongSecondLoop;
    public AudioClip caveBeforeFirstPrisonerLoop;

    [SerializeField] private AudioClip _introSong;
    [SerializeField] private AudioClip _powerUpIntroSong;
    [SerializeField] private AudioClip _powerUpPickupSong;
    [SerializeField] private AudioClip _caveIntense1Intro;
    [SerializeField] private AudioClip _caveIntense1Loop;    
    [SerializeField] private AudioClip _caveIntense2Intro;
    [SerializeField] private AudioClip _caveIntense2Loop;
    public AudioClip caveIntense1Outro;
    [SerializeField] private AudioClip _caveAvatarChaseIntro;
    [SerializeField] private AudioClip _caveAvatarChaseLoop;
    public AudioClip caveAvatarChaseOutro;
    [SerializeField] private AudioClip _caveSpaceRoomIntro;
    public AudioClip caveSpaceRoomOutro;
    [SerializeField] private AudioClip _caveSpaceRoomLoopIntro;
    [SerializeField] private AudioClip _caveSpaceRoomLoop;

    [SerializeField] private AudioClip _endSong;
    [SerializeField] private AudioClip _blobTransform;
    [SerializeField] private AudioClip _blobRooms;

    private AudioSource _introSource;
    private AudioSource _loopSource;
    private AudioSource _oneTimeSource;
    private AudioSource _nextBarSource;
    private float _currentVolume = 1f;

    void Awake() {
        obj = this;
    }

    public void PlayTitleSong() {
        PlayIntroAndLoop(_titleSongIntro, _titleSongLoop);
    }

    [ContextMenu("Play cave song")]
    public void PlayCaveSong() {
        PlayIntroAndLoop(_caveSongIntro, _caveSongLoop);
    }    
    public void PlayCaveSongOutro() {
        PlayOneTime(_caveSongOutro);
    }
    public void PlayCaveSpaceRoomIntro() {
        PlayOneTime(_caveSpaceRoomIntro);
    }
    public void PlayCaveSpaceRoomLoop() {
        PlayIntroAndLoop(_caveSpaceRoomLoopIntro, _caveSpaceRoomLoop);
    }
    [ContextMenu("Play cave first song")]
    public void PlayCaveFirstSong() {
        PlayIntroAndLoop(_caveSongFirstIntro, _caveSongFirstLoop);
    }
    [ContextMenu("Play cave second song")]
    public void PlayCaveSecondSong() {
        PlayIntroAndLoop(_caveSongSecondIntro, _caveSongSecondLoop);
    }
    [ContextMenu("Play cave loop")]
    public void PlayCaveLoop() {
        PlayLoop(_caveSongLoop);
    }
    public void PlayBlobRooms() {
        PlayLoop(_blobRooms);
    }

    [ContextMenu("Play end song")]
    public void PlayEndSong() {
        PlayOneTime(_endSong);
    }

    [ContextMenu("Play intro song")]
    public void PlayIntroSong() {
        PlayOneTime(_introSong);
    }

    [ContextMenu("Play power up intro song")]
    public void PlayPowerUpIntroSong() {
        PlayOneTime(_powerUpIntroSong);
    }

    [ContextMenu("Play power up pickup song")]
    public void PlayPowerUpPickupSong() {
        PlayOneTime(_powerUpPickupSong);
    }

    public void PlayBlobTransform() {
        PlayOneTime(_blobTransform);
    }

    [ContextMenu("Play cave before first prisoner song")]
    public void PlayCaveBeforeFirstPrisoner() {
        PlayLoop(caveBeforeFirstPrisonerLoop);
    }

    [ContextMenu("Play cave intense 1")]
    public void PlayCaveIntense1() {
        PlayIntroAndLoop(_caveIntense1Intro, _caveIntense1Loop);
    }

    [ContextMenu("Play cave intense 2")]
    public void PlayCaveIntense2() {
        PlayIntroAndLoop(_caveIntense2Intro, _caveIntense2Loop);
    }
    [ContextMenu("Play cave avatar chase")]
    public void PlayCaveAvatarChase() {
        PlayIntroAndLoop(_caveAvatarChaseIntro, _caveAvatarChaseLoop);
    }

    private void PlayIntroAndLoop(AudioClip introClip, AudioClip loopClip) {
        StopPlaying();
        _introSource = Instantiate(musicObject, Camera.main.gameObject.transform.position, Quaternion.identity);
        _introSource.transform.parent = transform;
        _loopSource = Instantiate(musicObject, Camera.main.gameObject.transform.position, Quaternion.identity);
        _loopSource.transform.parent = transform;
        _introSource.clip = introClip;
        _introSource.playOnAwake = false;
        double introDuration = (double)_introSource.clip.samples / _introSource.clip.frequency;
        
        _loopSource.clip = loopClip;
        _loopSource.playOnAwake = false;
        _loopSource.loop = true;

        double startTime = AudioSettings.dspTime + 1;
        _introSource.PlayScheduled(startTime);
        _loopSource.PlayScheduled(startTime + introDuration);
        
        // Set the current volume
        _introSource.volume = _currentVolume;
        _loopSource.volume = _currentVolume;
    }

    private void PlayOneTime(AudioClip clip) {
        StopPlaying();
        _oneTimeSource = Instantiate(musicObject, Camera.main.transform.position, Quaternion.identity);
        _oneTimeSource.transform.parent = transform;
        _oneTimeSource.clip = clip;
        _oneTimeSource.playOnAwake = false;
        _oneTimeSource.loop = false;
        _oneTimeSource.volume = _currentVolume;

        double startTime = AudioSettings.dspTime + 1;
        _oneTimeSource.PlayScheduled(startTime);
    }

    private void PlayLoop(AudioClip clip) {
        StopPlaying();
        _loopSource = Instantiate(musicObject, Camera.main.transform.position, Quaternion.identity);
        _loopSource.transform.parent = transform;
        _loopSource.clip = clip;
        _loopSource.playOnAwake = false;
        _loopSource.loop = true;
        _loopSource.volume = _currentVolume;

        double startTime = AudioSettings.dspTime + 1;
        _loopSource.PlayScheduled(startTime);
    }

    public void ScheduleClipOnNextBar(AudioClip clip, int beatsPerMinute, bool loop = true) {
        if (_loopSource == null && _oneTimeSource == null) {
            PlayLoop(clip);
            return;
        }

        // Calculate time to next bar
        double currentTime = AudioSettings.dspTime;
        double beatsPerSecond = beatsPerMinute / 60.0;
        double currentBeat = currentTime * beatsPerSecond;
        double nextBar = Mathf.Ceil((float)currentBeat / 4) * 4; // Assuming 4/4 time signature
        double nextBarTime = nextBar / beatsPerSecond;

        // Create and setup the new audio source
        if (_nextBarSource != null) {
            Destroy(_nextBarSource.gameObject);
        }
        _nextBarSource = Instantiate(musicObject, Camera.main.transform.position, Quaternion.identity);
        _nextBarSource.transform.parent = transform;
        _nextBarSource.clip = clip;
        _nextBarSource.playOnAwake = false;
        _nextBarSource.loop = loop;
        _nextBarSource.volume = 0f;

        // Apply low-pass filter to help with transition
        AudioLowPassFilter lowPassFilter = _nextBarSource.gameObject.AddComponent<AudioLowPassFilter>();
        lowPassFilter.cutoffFrequency = 22000; // Start with no filtering

        // Schedule the new clip
        _nextBarSource.PlayScheduled(nextBarTime);

        // Start crossfade
        StartCoroutine(CrossfadeToNewClip(nextBarTime));
    }

    private System.Collections.IEnumerator CrossfadeToNewClip(double startTime) {
        // Capture current volume levels before we start fading
        float startVolume = _currentVolume;
        
        // Determine which source is currently active
        bool usingLoopSource = _loopSource != null;
        bool usingIntroSource = _introSource != null;
        
        if (!usingLoopSource && !usingIntroSource) {
            yield break;
        }
        
        // Store the initial volumes
        float loopSourceInitialVolume = usingLoopSource ? _loopSource.volume : 0f;
        float introSourceInitialVolume = usingIntroSource ? _introSource.volume : 0f;
        
        // Wait until just before the scheduled time
        double prepTime = startTime - 0.1;
        while (AudioSettings.dspTime < prepTime) {
            yield return null;
        }
        
        // Add low-pass filters to active sources
        AudioLowPassFilter loopSourceLowPass = null;
        AudioLowPassFilter introSourceLowPass = null;
        
        if (usingLoopSource && !_loopSource.gameObject.TryGetComponent<AudioLowPassFilter>(out loopSourceLowPass)) {
            loopSourceLowPass = _loopSource.gameObject.AddComponent<AudioLowPassFilter>();
            loopSourceLowPass.cutoffFrequency = 22000;
        }
        
        if (usingIntroSource && !_introSource.gameObject.TryGetComponent<AudioLowPassFilter>(out introSourceLowPass)) {
            introSourceLowPass = _introSource.gameObject.AddComponent<AudioLowPassFilter>();
            introSourceLowPass.cutoffFrequency = 22000;
        }
        
        AudioLowPassFilter nextLowPass = _nextBarSource.GetComponent<AudioLowPassFilter>();
        if (nextLowPass == null) {
            nextLowPass = _nextBarSource.gameObject.AddComponent<AudioLowPassFilter>();
            nextLowPass.cutoffFrequency = 1000; // Start with filtering
        }
        
        // Wait until the exact scheduled time
        while (AudioSettings.dspTime < startTime) {
            yield return null;
        }

        float elapsed = 0f;
        
        // Perform the crossfade with smooth curves
        while (elapsed < crossfadeDuration) {
            float normalizedTime = elapsed / crossfadeDuration;
            
            // Use either the animation curve or a smoothstep function for more natural fading
            float fadeOutValue = fadeCurve != null && fadeCurve.keys.Length > 0 
                ? fadeCurve.Evaluate(1 - normalizedTime) 
                : Mathf.SmoothStep(1, 0, normalizedTime);
                
            float fadeInValue = fadeCurve != null && fadeCurve.keys.Length > 0 
                ? fadeCurve.Evaluate(normalizedTime) 
                : Mathf.SmoothStep(0, 1, normalizedTime);
            
            // Apply volume changes directly to active sources
            if (usingLoopSource && _loopSource != null) {
                _loopSource.volume = loopSourceInitialVolume * fadeOutValue;
                if (loopSourceLowPass != null) {
                    loopSourceLowPass.cutoffFrequency = Mathf.Lerp(22000, 1000, normalizedTime);
                }
            }
            
            if (usingIntroSource && _introSource != null) {
                _introSource.volume = introSourceInitialVolume * fadeOutValue;
                if (introSourceLowPass != null) {
                    introSourceLowPass.cutoffFrequency = Mathf.Lerp(22000, 1000, normalizedTime);
                }
            }
            
            // Fade in the new track
            _nextBarSource.volume = startVolume * fadeInValue;
            
            // Gradually remove low-pass filter from the fading in track
            if (nextLowPass != null) {
                nextLowPass.cutoffFrequency = Mathf.Lerp(1000, 22000, normalizedTime);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the volumes are fully at 0 before stopping
        if (usingLoopSource && _loopSource != null) _loopSource.volume = 0f;
        if (usingIntroSource && _introSource != null) _introSource.volume = 0f;
        _nextBarSource.volume = startVolume;
        
        // Remove the low-pass filter from the new source
        if (nextLowPass != null) {
            Destroy(nextLowPass);
        }
        
        // Store references to old sources for cleanup
        AudioSource oldIntroSource = _introSource;
        AudioSource oldLoopSource = _loopSource;
        
        // Move next source to appropriate source variable
        if (_nextBarSource.loop) {
            _loopSource = _nextBarSource;
        } else {
            _oneTimeSource = _nextBarSource;
            _loopSource = null;
        }
        _introSource = null;
        _nextBarSource = null;
        
        // Clean up old sources after a longer delay to ensure smooth transition
        yield return new WaitForSeconds(0.5f);
        
        // Clean up old sources
        if (oldIntroSource != null && oldIntroSource != _loopSource && oldIntroSource != _oneTimeSource) {
            oldIntroSource.Stop();
            Destroy(oldIntroSource.gameObject);
        }
        if (oldLoopSource != null && oldLoopSource != _loopSource && oldLoopSource != _oneTimeSource) {
            oldLoopSource.Stop();
            Destroy(oldLoopSource.gameObject);
        }
    }

    public void StopPlaying() {
        if(_introSource != null) {
            _introSource.Stop();
            Destroy(_introSource.gameObject);
        }
        if(_loopSource != null) {
            _loopSource.Stop();
            Destroy(_loopSource.gameObject);
        }
    }

    public void StopPlayingOneTime() {
        if(_oneTimeSource != null) {
            _oneTimeSource.Stop();
            Destroy(_oneTimeSource);
        }
    }

    public bool IsPlaying() {
        return (_introSource != null && _introSource.isPlaying) || 
               (_loopSource != null && _loopSource.isPlaying);
    }

    void FixedUpdate() {
        // Check for audio sources that are not playing but still exist
        if (_introSource != null && !_introSource.isPlaying) {
            Destroy(_introSource.gameObject);
            _introSource = null;
        }
        
        if (_loopSource != null && !_loopSource.isPlaying) {
            Destroy(_loopSource.gameObject);
            _loopSource = null;
        }
        
        if (_oneTimeSource != null && !_oneTimeSource.isPlaying) {
            Destroy(_oneTimeSource.gameObject);
            _oneTimeSource = null;
        }
        
        if (_nextBarSource != null && !_nextBarSource.isPlaying) {
            Destroy(_nextBarSource.gameObject);
            _nextBarSource = null;
        }
    }

    public AudioSource GetLoopSource() {
        return _loopSource;
    }

    void OnDestroy() {
        obj = null;
    }
}
