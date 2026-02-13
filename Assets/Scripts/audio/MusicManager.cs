using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager obj;
    [SerializeField] private MusicLibrary musicLibrary;

    private EventInstance currentInstance;
    private MusicTrack currentTrack;
    private PARAMETER_ID endingParamId;
    private bool currentTrackHasEnding;
    private enum MusicLogicalState {
        None,
        Playing
    }
    private MusicLogicalState _musicLogicalState = MusicLogicalState.None;

    void Awake()
    {
        obj = this;
    }

    // Public API — this is what scenes call
    public void Play(MusicTrack track)
    {
        if (track == null)
            return;

        if (currentTrack == track)
            return; // already playing

        _musicLogicalState = MusicLogicalState.Playing;
        currentTrack = track;

        SwapTrack(track);
    }

    public void Stop()
    {
        if(currentTrack == null)
            return;
        if(_musicLogicalState == MusicLogicalState.None)
            return;

        _musicLogicalState = MusicLogicalState.None;
        currentTrack = null;

        StartCoroutine(StopCurrent());
    }

    public void EndCurrentTrack()
    {
        if (_musicLogicalState != MusicLogicalState.Playing)
            return;

        if (!currentTrackHasEnding)
            return;

        currentInstance.setParameterByID(endingParamId, 1f, true);

        // Release ownership immediately (FMOD will finish playback)
        currentInstance.release();
        currentInstance.clearHandle();

        // Logically, music is now done
        currentTrack = null;
        _musicLogicalState = MusicLogicalState.None;
    }

    public MusicTrack CurrentTrack => currentTrack;

    public void PlayById(string trackId)
    {
        if (musicLibrary == null)
            return;

        musicLibrary.Init();
        var track = musicLibrary.GetById(trackId);
        Play(track);
    }

    private void SwapTrack(MusicTrack nextTrack)
    {
        // Fade out old (FMOD handles timing)
        if (currentInstance.isValid())
        {
            currentInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentInstance.release();
            currentInstance.clearHandle();
        }

        // Start new
        currentInstance = RuntimeManager.CreateInstance(nextTrack.eventRef);
        currentTrackHasEnding = nextTrack.hasEnding;
        if(currentTrackHasEnding) {
            currentInstance.getDescription(out var desc);
            desc.getParameterDescriptionByName(
                nextTrack.endingParameterName,
                out var paramDesc
            );
            endingParamId = paramDesc.id;

            // Ensure we're in "normal" mode
            currentInstance.setParameterByID(endingParamId, 0f);
        }
        currentInstance.start();
    }

    IEnumerator StopCurrent()
    {
        if (!currentInstance.isValid())
            yield break;

        currentInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentInstance.release();
        currentInstance.clearHandle();
    }

    void OnDestroy()
    {
        if (currentInstance.isValid())
        {
            currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentInstance.release();
        }
        obj = null;
    }
}
