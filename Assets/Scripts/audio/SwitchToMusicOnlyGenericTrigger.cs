using System.Collections;
using UnityEngine;

public class SwitchToMusicOnlyGenericTrigger : MonoBehaviour
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
            MusicManager.obj.PlayCaveSong();
        }

        float ambienceVolume = SoundMixerManager.obj.GetAmbienceVolume();
        StartCoroutine(SoundMixerManager.obj.StartAmbienceFade(3f, 0.001f));
        while(SoundMixerManager.obj.GetAmbienceVolume() > 0.001f) {
            yield return null;
        }
        AmbienceManager.obj.StopAmbience();
        SoundMixerManager.obj.SetAmbienceVolume(ambienceVolume);
    }
}
