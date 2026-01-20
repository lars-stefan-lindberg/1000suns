using UnityEngine;

public class BabyPrisonerAnimationEvents : MonoBehaviour
{
    public void PlayDefaultCrawl() {
        SoundFXManager.obj.PlayBabyPrisonerCrawl(gameObject.transform);
    }
}
