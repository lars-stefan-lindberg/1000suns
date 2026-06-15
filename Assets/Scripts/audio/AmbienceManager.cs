using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AmbienceManager : MonoBehaviour
{
    public static AmbienceManager obj;

    [SerializeField] private AmbienceLibrary ambienceLibrary;

    private Dictionary<AmbienceTrack, EventInstance> activeInstances = new Dictionary<AmbienceTrack, EventInstance>();

    void Awake() {
        obj = this;
    }

    public void Play(AmbienceTrack track)
    {
        if (track == null)
            return;

        if (activeInstances.ContainsKey(track))
            return;

        EventInstance instance = RuntimeManager.CreateInstance(track.eventRef);
        instance.start();
        activeInstances[track] = instance;
    }

    public void Stop(AmbienceTrack track)
    {
        if (track == null || !activeInstances.ContainsKey(track))
            return;

        EventInstance instance = activeInstances[track];
        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            instance.clearHandle();
        }

        activeInstances.Remove(track);
    }

    public void Stop() {
        StopAll();
    }

    public void StopAll()
    {
        foreach (var kvp in activeInstances)
        {
            if (kvp.Value.isValid())
            {
                kvp.Value.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                kvp.Value.release();
                kvp.Value.clearHandle();
            }
        }

        activeInstances.Clear();
    }

    public IReadOnlyDictionary<AmbienceTrack, EventInstance> ActiveInstances => activeInstances;

    public void PlayById(string trackId)
    {
        if (ambienceLibrary == null)
            return;

        ambienceLibrary.Init();
        var track = ambienceLibrary.GetById(trackId);
        Play(track);
    }

    void OnDestroy() {
        foreach (var kvp in activeInstances)
        {
            if (kvp.Value.isValid())
            {
                kvp.Value.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                kvp.Value.release();
            }
        }
        activeInstances.Clear();
        obj = null;
    }
}
