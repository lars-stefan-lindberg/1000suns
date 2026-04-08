using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class PlayForestWaterfallSfx : MonoBehaviour
{
    [SerializeField] private EventReference _sfx;

    private EventInstance _eventInstance;

    void Start()
    {
        StartAmbience();
    }

    void OnDestroy() {
        StopAmbience();
    }

    public void StartAmbience() {
        if(!_eventInstance.isValid()) {
            _eventInstance = SoundFXManager.obj.CreateAttachedInstance(_sfx, gameObject);
            _eventInstance.start();
        }
    }

    public void StopAmbience() {
        _eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _eventInstance.release();
    }
}
