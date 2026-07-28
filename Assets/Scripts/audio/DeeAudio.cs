using FMOD.Studio;
using UnityEngine;

public class DeeAudio : MonoBehaviour
{
    [SerializeField] private DeeSoundSet _sounds;

    public void PlayShadowPullGrab(ref EventInstance forcePullStartInstance) {
        forcePullStartInstance = SoundFXManager.obj.CreateAttachedInstance(_sounds.shadowPullGrab, gameObject);
        forcePullStartInstance.start();
        forcePullStartInstance.release();
    }

    public void PlayShadowPullRelease(Transform objectTransform) {
        SoundFXManager.obj.PlayAtPosition(_sounds.shadowPullRelease, objectTransform.position);
    }

    public void PlayAnchorReached() {
        SoundFXManager.obj.PlayAtPosition(_sounds.anchorReached, transform.position);
    }

    public void StopInstanceWithFadeOut(ref EventInstance instance) {
        if (instance.isValid()) {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }
    }

    public EventInstance StartShadowPullMoveLoop() {
        EventInstance instance = SoundFXManager.obj.CreateAttachedInstance(_sounds.shadowPullMoveLoop, gameObject);
        instance.start();
        return instance;
    }

    public EventInstance StartShadowPullLoop() {
        EventInstance instance = SoundFXManager.obj.CreateAttachedInstance(_sounds.shadowPullLoop, gameObject);
        instance.start();
        return instance;
    }
}
