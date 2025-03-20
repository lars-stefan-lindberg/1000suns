using System.Collections;
using UnityEngine;

public class BeforeFirstPrisoner : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 5f;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            StartCoroutine(SwitchAudio());
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator SwitchAudio() {
        float musicVolume = SoundMixerManager.obj.GetMusicVolume();
        StartCoroutine(SoundMixerManager.obj.StartMusicFade(_fadeDuration, 0.001f));
        AmbienceManager.obj.PlayCaveAmbience();
        AmbienceManager.obj.FadeInAmbienceSource1(2f);
        while(SoundMixerManager.obj.GetMusicVolume() > 0.001f) {
            yield return null;
        }
        MusicManager.obj.StopPlaying();
        SoundMixerManager.obj.SetMusicVolume(musicVolume);
        MusicManager.obj.PlayCaveBeforeFirstPrisoner();
    }
}
