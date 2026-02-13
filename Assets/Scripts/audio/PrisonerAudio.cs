using UnityEngine;

public class PrisonerAudio : MonoBehaviour
{
    [SerializeField] private PrisonerSoundSet _sounds;

    public void PlayCrawl() {
        SoundFXManager.obj.PlayAtPosition(_sounds.crawl, transform.position);
    }

    public void PlaySlide() {
        SoundFXManager.obj.PlayAtGameObject(_sounds.slide, gameObject);
    }

    public void PlayDeath() {
        SoundFXManager.obj.PlayAtPosition(_sounds.death, transform.position);
    }

    public void PlayHit() {
        //Not used
    }

    public void PlaySpawn() {
        SoundFXManager.obj.PlayAtPosition(_sounds.spawn, transform.position);
    }
}
