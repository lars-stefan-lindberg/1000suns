using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrisonerAnimationEvents : MonoBehaviour
{
    private Prisoner prisoner;
    private PrisonerAudio _prisonerAudio;

    void Awake() {
        prisoner = GetComponent<Prisoner>();
        _prisonerAudio = GetComponent<PrisonerAudio>();
    }

    public void PlayDefaultCrawl() {
        _prisonerAudio.PlayCrawl();
    }
    public void PlaySpawn() {
        if(SceneManager.GetActiveScene().GetRootGameObjects().Contains(gameObject))
            _prisonerAudio.PlaySpawn();
    }
    public void PlaySlide() {
        _prisonerAudio.PlaySlide();
    }

    public void PlayDeath() {
        if(!prisoner.muteDeathSoundFX)
            _prisonerAudio.PlayDeath();
    }
}
