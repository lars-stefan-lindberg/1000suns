using FMOD.Studio;
using UnityEngine;

public class BabyPrisonerAudio : MonoBehaviour
{
    [SerializeField] private BabyPrisonerSoundSet _sounds;

    public void PlayCrawl() {
        SoundFXManager.obj.PlayAtPosition(_sounds.crawl, transform.position);
    }

    public void PlayScared() {
        SoundFXManager.obj.PlayAtGameObject(_sounds.scared, gameObject);
    }

    public void PlayIdle() {
        SoundFXManager.obj.PlayAtPosition(_sounds.idle, transform.position);
    }

    public void PlayDespawn() {
        SoundFXManager.obj.PlayAtPosition(_sounds.despawn, transform.position);
    }

    public void PlayAlert() {
        SoundFXManager.obj.PlayAtPosition(_sounds.alert, transform.position);
    }

    public void PlayEscape(ref EventInstance escapeSfxInstance) {
        escapeSfxInstance = SoundFXManager.obj.CreateAttachedInstance(_sounds.escapeLoop, gameObject);
        escapeSfxInstance.start();
        escapeSfxInstance.release();
    }
}
