using FMOD.Studio;
using UnityEngine;

public class BlockAudio : MonoBehaviour
{
    [SerializeField] private BlockSoundSet _blockSoundSet;

    public void PlayWallHit() {
        SoundFXManager.obj.PlayAtPosition(_blockSoundSet.wallHit, transform.position);
    }

    public void PlaySlideOffEdge() {
        SoundFXManager.obj.PlayAtPosition(_blockSoundSet.slideOffEdge, transform.position);
    }

    public void PlayLand() {
        SoundFXManager.obj.PlayAtPosition(_blockSoundSet.land, transform.position);
    }

    public void PlaySlide(ref EventInstance slideSfxInstance) {
        slideSfxInstance = SoundFXManager.obj.CreateAttachedInstance(_blockSoundSet.slide, gameObject);
        slideSfxInstance.start();
        slideSfxInstance.release();
    }
}
