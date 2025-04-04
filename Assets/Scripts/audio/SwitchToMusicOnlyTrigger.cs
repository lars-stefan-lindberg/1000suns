using System.Collections;
using UnityEngine;

public class SwitchToMusicOnlyTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(MusicManager.obj.IsPlaying()) return;
        if(other.CompareTag("Player")) {
            StartCoroutine(FadeOutAmbienceAndStartMusic());
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator FadeOutAmbienceAndStartMusic() {
        MusicManager.obj.PlayCaveSong();

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
