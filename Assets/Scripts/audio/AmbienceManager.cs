using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AmbienceManager : MonoBehaviour
{
    public static AmbienceManager obj;

    [SerializeField] private AmbienceLibrary ambienceLibrary;

    private EventInstance currentInstance;
    private AmbienceTrack currentAmbience;

    void Awake() {
        obj = this;
    }

    public void Play(AmbienceTrack track)
    {
        if (track == null)
            return;

        if (currentAmbience == track)
            return;

        Stop();

        currentAmbience = track;
        currentInstance = RuntimeManager.CreateInstance(track.eventRef);
        currentInstance.start();
    }

    public void Stop()
    {
        if (!currentInstance.isValid())
            return;

        currentInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentInstance.release();
        currentInstance.clearHandle();

        currentAmbience = null;
    }

    public AmbienceTrack CurrentAmbience => currentAmbience;

    public void PlayById(string trackId)
    {
        if (ambienceLibrary == null)
            return;

        ambienceLibrary.Init();
        var track = ambienceLibrary.GetById(trackId);
        Play(track);
    }

    void OnDestroy() {
        if (currentInstance.isValid())
        {
            currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentInstance.release();
        }
        obj = null;
    }
}
