using UnityEngine;

public class PrisonerCutsceneAnimationEvents : MonoBehaviour
{
    private LightSprite2DFadeManager _lightSprite2DFadeManager;
    private PrisonerCutScene _prisoner;
    private PrisonerAudio _prisonerAudio;

    void Awake() {
        _prisoner = GetComponentInParent<PrisonerCutScene>();
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

    public void SpawnStarted() {
        _lightSprite2DFadeManager.StartFadeIn();
    }

    public void SpawningComplete() {
        _prisoner.IsSpawning = false;
    }

    public void Despawn() {
        _prisonerAudio.PlaySpawn();
    }

    public void DisableRenderer() {
        GetComponent<SpriteRenderer>().enabled = false;
    }
}
