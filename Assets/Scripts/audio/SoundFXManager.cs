using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager obj;
    
    private Dictionary<string, EventInstance> _namedInstances = new Dictionary<string, EventInstance>();

    void Start() {
        obj = this;
    }

    void OnDestroy()
    {
        obj = null;
    }

    public void PlayAtPosition(EventReference sfx, Vector3 position) {
        RuntimeManager.PlayOneShot(sfx, position);
    }

    public void Play2D(EventReference sfx) {
        RuntimeManager.PlayOneShot(sfx);
    }

    // Spatial one-shot with parameters (footsteps, surfaces, etc.)
    public void PlayAtGameObject(
        EventReference sfx,
        GameObject emitter,
        System.Action<EventInstance> configure = null
    )
    {
        var inst = RuntimeManager.CreateInstance(sfx);

        configure?.Invoke(inst);

        RuntimeManager.AttachInstanceToGameObject(inst, emitter);
        inst.start();
        inst.release();
    }

    //Return the EventInstance to give control to caller. If the event needs to be interrupted
    public EventInstance CreateAttachedInstance(
        EventReference sfx,
        GameObject emitter,
        System.Action<EventInstance> configure = null
    )
    {
        var inst = RuntimeManager.CreateInstance(sfx);

        configure?.Invoke(inst);

        RuntimeManager.AttachInstanceToGameObject(inst, emitter);

        return inst;
    }

    // Create and start a persistent 2D looping sound that can be controlled across scenes
    public EventInstance StartPersistent2DSound(EventReference sfx)
    {
        var inst = RuntimeManager.CreateInstance(sfx);
        inst.start();
        return inst;
    }

    // Update a parameter on a persistent sound instance
    public void SetSoundParameter(EventInstance instance, string parameterName, float value)
    {
        if (instance.isValid())
        {
            instance.setParameterByName(parameterName, value);
        }
    }

    // Stop and release a persistent sound instance
    public void StopPersistentSound(EventInstance instance, bool allowFadeout = true)
    {
        if (instance.isValid())
        {
            instance.stop(allowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }
    }

    // Create and start a named persistent 2D sound that can be accessed across scenes
    public EventInstance StartNamedPersistent2DSound(string name, EventReference sfx)
    {
        var inst = RuntimeManager.CreateInstance(sfx);
        inst.start();
        _namedInstances[name] = inst;
        return inst;
    }

    // Get a named persistent sound instance
    public EventInstance GetNamedSound(string name)
    {
        if (_namedInstances.TryGetValue(name, out EventInstance instance))
        {
            return instance;
        }
        return default(EventInstance);
    }

    // Check if a named sound exists and is valid
    public bool HasNamedSound(string name)
    {
        return _namedInstances.ContainsKey(name) && _namedInstances[name].isValid();
    }

    // Stop and release a named persistent sound instance
    public void StopNamedSound(string name, bool allowFadeout = true)
    {
        if (_namedInstances.TryGetValue(name, out EventInstance instance))
        {
            if (instance.isValid())
            {
                instance.stop(allowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.release();
            }
            _namedInstances.Remove(name);
        }
    }
}
