using FMOD.Studio;
using UnityEngine;

public class DeeAudio : MonoBehaviour
{
    [SerializeField] private DeeSoundSet _sounds;

    public void PlayForcePullStart(ref EventInstance forcePullStartInstance) {
        forcePullStartInstance = SoundFXManager.obj.CreateAttachedInstance(_sounds.forcePullStart, gameObject);
        forcePullStartInstance.start();
        forcePullStartInstance.release();
    }

    public void PlayAnchorReached() {
        SoundFXManager.obj.PlayAtPosition(_sounds.anchorReached, transform.position);
    }
}
