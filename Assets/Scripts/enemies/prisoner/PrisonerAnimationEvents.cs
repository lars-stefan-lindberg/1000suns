using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrisonerAnimationEvents : MonoBehaviour
{
    private Prisoner prisoner;

    void Awake() {
        prisoner = GetComponent<Prisoner>();
    }

    public void PlayDefaultCrawl() {
        if(!prisoner.offScreen)
            SoundFXManager.obj.PlayPrisonerCrawl(gameObject.transform);
    }
    public void PlaySpawn() {
        if(SceneManager.GetActiveScene().GetRootGameObjects().Contains(gameObject))
            SoundFXManager.obj.PlayPrisonerSpawn(gameObject.transform);
    }
    public void PlaySlide() {
        SoundFXManager.obj.PlayPrisonerSlide(gameObject.transform);
    }
}
