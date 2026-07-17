using UnityEngine;

public class PrisonerAnimationEvents : MonoBehaviour
{
    private LightSprite2DFadeManager _lightSprite2DFadeManager;
    private Prisoner _prisoner;
    private PrisonerAudio _prisonerAudio;

    void Awake() {
        _prisoner = GetComponentInParent<Prisoner>();
        _prisonerAudio = GetComponentInParent<PrisonerAudio>();
        _lightSprite2DFadeManager = GetComponentInParent<LightSprite2DFadeManager>();
    }

    public void PlayDefaultCrawl() {
        _prisonerAudio.PlayCrawl();
    }
    public void PlaySpawn() {
        _prisonerAudio.PlaySpawn();
    }
    public void PlaySlide() {
        _prisonerAudio.PlaySlide();
    }

    public void PlayDeath() {
        if(!_prisoner.muteDeathSoundFX)
            _prisonerAudio.PlayDeath();
    }

    public void SpawnStarted() {
        _lightSprite2DFadeManager.StartFadeIn();
    }

    public void SpawningComplete() {
        _prisoner.IsSpawning = false;
    }

    public void Kill() {
        _prisoner.Kill();
    }
}
