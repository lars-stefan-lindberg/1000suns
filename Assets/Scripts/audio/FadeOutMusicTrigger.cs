using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FadeOutMusicTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            StartCoroutine(FadeOutAndStopMusic());
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator FadeOutAndStopMusic() {
        float musicVolume = SoundMixerManager.obj.GetMusicVolume();
        StartCoroutine(SoundMixerManager.obj.StartMusicFade(3f, 0.001f));
        while(SoundMixerManager.obj.GetMusicVolume() > 0.001f) {
            yield return null;
        }
        MusicManager.obj.StopPlaying();
        SoundMixerManager.obj.SetMusicVolume(musicVolume);
        AmbienceManager.obj.PlayCapeRoomAmbience();
        AmbienceManager.obj.FadeInAmbienceSource2And3(3f);
    }
}
