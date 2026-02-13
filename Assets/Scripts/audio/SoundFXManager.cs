using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager obj;

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
}
