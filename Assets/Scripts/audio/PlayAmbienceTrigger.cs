using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayAmbienceTrigger : MonoBehaviour
{
    [SerializeField] private AmbienceTrack _track;
    [SerializeField] private bool _stopMusic = false;
    [SerializeField] private bool _stopAmbience = false;
    [SerializeField] private bool _stopCurrentlyPlayingAmbience = false;
    [SerializeField] private bool _playSimultaneouslyWithMusic = false;

    void OnTriggerEnter2D(Collider2D collider) {
        if(!collider.CompareTag("Player"))
            return;

        if(_stopMusic)
            MusicManager.obj.Stop();

        if(_stopAmbience)
            AmbienceManager.obj.Stop(_track);
        else {
            if(!_playSimultaneouslyWithMusic && MusicManager.obj.IsPlaying()) {
                // Do nothing, let the music continue
            } else {
                if(_stopCurrentlyPlayingAmbience) {
                    StartCoroutine(StopAndPlay(_track));
                } else {
                    AmbienceManager.obj.Play(_track);
                }
            }
        }

        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }

    private IEnumerator StopAndPlay(AmbienceTrack track) {
        yield return StartCoroutine(AmbienceManager.obj.StopAllExcept(track));
        AmbienceManager.obj.Play(track);
        yield return null;
    }
}
