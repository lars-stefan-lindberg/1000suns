using UnityEngine;

public class BabyPrisonerAnimationEvents : MonoBehaviour
{
    public void PlayDefaultCrawl() {
        SoundFXManager.obj.PlayBabyPrisonerCrawl(gameObject.transform);
    }

    public void PlayIdle() {
        SoundFXManager.obj.PlayBabyPrisonerIdle(gameObject.transform);
    }
}
