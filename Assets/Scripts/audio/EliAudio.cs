using FMOD.Studio;
using UnityEngine;

public class EliAudio : MonoBehaviour
{
    [SerializeField] private EliSoundSet _sounds;

    public void PlayForcePushStart(ref EventInstance forcePushStartInstance) {
        forcePushStartInstance = SoundFXManager.obj.CreateAttachedInstance(_sounds.forcePushStart, gameObject);
        forcePushStartInstance.start();
        forcePushStartInstance.release();
    }

    public void PlayForcePushRelease() {
        SoundFXManager.obj.PlayAtPosition(_sounds.forcePushRelease, transform.position);
    }
    
    public void PlayHeavyLand() {
        SoundFXManager.obj.PlayAtPosition(_sounds.heavyLand, transform.position);
    }
    
    public void PlayLongFall() {
        SoundFXManager.obj.PlayAtPosition(_sounds.longFall, transform.position);
    }
    
    public void PlayShapeshiftToBlob() {
        SoundFXManager.obj.PlayAtPosition(_sounds.shapeshiftToBlob, transform.position);
    }
    
    public void PlayForcePushJump() {
        SoundFXManager.obj.PlayAtPosition(_sounds.forcePushJump, transform.position);
    }
    
    public void PlayForcePushLand() {
        SoundFXManager.obj.PlayAtPosition(_sounds.forcePushLand, transform.position);
    }

    public void PlayHeadMovingOutOfTent() {
        SoundFXManager.obj.PlayAtPosition(_sounds.headOutOfTent, transform.position);
    }

    public void PlayGetOutOfTent() {
        SoundFXManager.obj.PlayAtPosition(_sounds.getOutOfTent, transform.position);
    }

    public void PlayYawn() {
        SoundFXManager.obj.PlayAtPosition(_sounds.yawn, transform.position);
    }
}
