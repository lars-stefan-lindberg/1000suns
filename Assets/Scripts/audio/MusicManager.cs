using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MusicManager : MonoBehaviour
{
    public static MusicManager obj;

    [SerializeField] private AudioSource musicObject;

    [SerializeField] private AudioClip _titleSongIntro;
    [SerializeField] private AudioClip _titleSongLoop;

    [SerializeField] private AudioClip _caveSongIntro;
    [SerializeField] private AudioClip _caveSongLoop;

    [SerializeField] private AudioClip _endSong;

    private AudioSource _introSource;
    private AudioSource _loopSource;
    private AudioSource _oneTimeSource;

    void Awake() {
        obj = this;
    }

    // public void PlayTitleSong() {
    //     double _goalTime = AudioSettings.dspTime + 2;

    //     _audioSources[0].clip = _titleSongIntro;
    //     _audioSources[0].loop = false;
    //     _audioSources[0].playOnAwake = false;
    //     _audioSources[0].PlayScheduled(_goalTime);
    // }

    // void Update() {
    //     if(AudioSettings.dspTime > _goalTime - 1) {
    //         if(_isLooping)
    //             PlayAndScheduleLoop();
    //     }
    // }

    // private void PlayScheduledClip() {
    //     _audioSources[_audioToggle].clip = _titleSongLoop;
    //     _audioSources[_audioToggle].PlayScheduled(_goalTime);

    //     _musicDuration = (double)_titleSongLoop.samples / _titleSongLoop.frequency;
    //     _goalTime += _musicDuration;

    //     _audioToggle = 1 - _audioToggle;
    // }

    public void PlayTitleSong() {

        PlayIntroAndLoop(_titleSongIntro, _titleSongLoop);
    }

    [ContextMenu("Play cave song")]
    public void PlayCaveSong() {
        PlayIntroAndLoop(_caveSongIntro, _caveSongLoop);
    }
    [ContextMenu("Play cave loop")]
    public void PlayCaveLoop() {
        PlayLoop(_caveSongLoop);
    }

    [ContextMenu("Play end song")]
    public void PlayEndSong() {
        PlayOneTime(_endSong);
    }

    private void PlayIntroAndLoop(AudioClip introClip, AudioClip loopClip) {
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
    }

    private void PlayOneTime(AudioClip clip) {
        _oneTimeSource = Instantiate(musicObject, Camera.main.transform.position, Quaternion.identity);
        _oneTimeSource.transform.parent = transform;
        _oneTimeSource.clip = clip;
        _oneTimeSource.playOnAwake = false;
        _oneTimeSource.loop = false;

        double startTime = AudioSettings.dspTime + 1;
        _oneTimeSource.PlayScheduled(startTime);
    }

    private void PlayLoop(AudioClip clip) {
        _loopSource = Instantiate(musicObject, Camera.main.transform.position, Quaternion.identity);
        _loopSource.transform.parent = transform;
        _loopSource.clip = clip;
        _loopSource.playOnAwake = false;
        _loopSource.loop = true;

        double startTime = AudioSettings.dspTime + 1;
        _loopSource.PlayScheduled(startTime);
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
        if(_oneTimeSource != null) {
            _oneTimeSource.Stop();
            Destroy(_oneTimeSource.gameObject);
        }
    }

    void Destroy() {
        obj = null;
    }
}
