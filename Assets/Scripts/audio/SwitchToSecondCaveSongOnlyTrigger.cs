using System.Collections;
using UnityEngine;

public class SwitchToSecondCaveSongOnlyTrigger : MonoBehaviour
{
    void Awake() {
        if(GameEventManager.obj.AfterPowerUpRoomsCompletedWallBreak) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            StartCoroutine(FadeOutAmbienceAndStartMusic());
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator FadeOutAmbienceAndStartMusic() {
        if(!MusicManager.obj.IsPlaying()) {
            MusicManager.obj.PlayCaveSecondSong();
        }

        float ambienceVolume = SoundMixerManager.obj.GetAmbienceVolume();
        StartCoroutine(SoundMixerManager.obj.StartAmbienceFade(3f, 0.001f));
        while(SoundMixerManager.obj.GetAmbienceVolume() > 0.001f) {
            yield return null;
        }
        //Give SoundMixerManager time to fully complete the fading
        yield return new WaitForSeconds(0.1f);
        AmbienceManager.obj.StopAmbience();
        SoundMixerManager.obj.SetAmbienceVolume(ambienceVolume);
    }
}
