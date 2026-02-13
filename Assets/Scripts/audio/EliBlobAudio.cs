using FMOD.Studio;
using UnityEngine;

public class EliBlobAudio : MonoBehaviour
{
    [SerializeField] private EliBlobSoundSet _sounds;

    public void PlayChargeStart(ref EventInstance chargeStartInstance) {
        chargeStartInstance = SoundFXManager.obj.CreateAttachedInstance(_sounds.chargeStart, gameObject);
        chargeStartInstance.start();
        chargeStartInstance.release();
    }

    public void PlayChargeRelease() {
        SoundFXManager.obj.PlayAtPosition(_sounds.chargeRelease, transform.position);
    }
}
