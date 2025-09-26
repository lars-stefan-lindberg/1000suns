using System.Collections;
using UnityEngine;

public class FadeOutMusicTrigger : MonoBehaviour
{
    [SerializeField] float _fadeOutDuration = 3f;
    
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            StartCoroutine(FadeOutAndStopMusic());
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator FadeOutAndStopMusic() {
        float musicVolume = SoundMixerManager.obj.GetMusicVolume();
        MusicManager.obj.SetCurrentMusicId(MusicManager.MusicId.None);
        StartCoroutine(SoundMixerManager.obj.StartMusicFade(_fadeOutDuration, 0.001f));
        AmbienceManager.obj.PlayCapeRoomAmbience();
        AmbienceManager.obj.FadeInAmbienceSource2And3(_fadeOutDuration);
        while(SoundMixerManager.obj.GetMusicVolume() > 0.001f) {
            yield return null;
        }
        //Give SoundMixerManager time to fully complete the fading
        yield return new WaitForSeconds(0.1f);
        MusicManager.obj.StopPlaying();
        SoundMixerManager.obj.SetMusicVolume(musicVolume);
    }
}
