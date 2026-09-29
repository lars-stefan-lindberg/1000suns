using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using System.Collections;

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
        List<EventInstance> tracksToStop = new List<EventInstance>(activeInstances.Values);
        activeInstances.Clear();
        foreach (var inst in tracksToStop)
        {
            if (inst.isValid())
            {
                inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                inst.release();
                inst.clearHandle();
            }
        }
    }

    public IEnumerator StopAllExcept(AmbienceTrack track)
    {
        List<EventInstance> tracksToStop = new List<EventInstance>();
        EventInstance trackToKeep = default(EventInstance);
        bool hasTrackToKeep = false;
        
        foreach (var kvp in activeInstances)
        {
            if (kvp.Key.ambienceId != track.ambienceId)
            {
                tracksToStop.Add(kvp.Value);
            }
            else
            {
                trackToKeep = kvp.Value;
                hasTrackToKeep = true;
            }
        }
        activeInstances.Clear();
        
        // Re-add the track we want to keep
        if (hasTrackToKeep)
        {
            activeInstances[track] = trackToKeep;
        }
        
        foreach (var trackToStop in tracksToStop)
        {
            if (trackToStop.isValid())
            {
                trackToStop.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                trackToStop.release();
                trackToStop.clearHandle();
            }
        }
        
        yield return null;
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
