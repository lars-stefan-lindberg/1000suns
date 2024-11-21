using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        SoundFXManager.obj.PlayPrisonerSpawn(gameObject.transform);
    }
    public void PlaySlide() {
        SoundFXManager.obj.PlayPrisonerSlide(gameObject.transform);
    }
}
