using UnityEngine;

public class BabyPrisonerAnimationEvents : MonoBehaviour
{
    private BabyPrisonerAudio _babyPrisonerAudio;

    void Awake() {
        _babyPrisonerAudio = GetComponent<BabyPrisonerAudio>();
    }

    public void PlayDefaultCrawl() {
        _babyPrisonerAudio.PlayCrawl();
    }
}
