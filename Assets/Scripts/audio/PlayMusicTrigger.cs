using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayMusicTrigger : MonoBehaviour
{
    [SerializeField] private MusicTrack _track;
    [SerializeField] private bool _stopAmbience = false;

    void OnTriggerEnter2D(Collider2D collider) {
        if(!collider.CompareTag("Player"))
            return;

        if(_stopAmbience)
            AmbienceManager.obj.Stop();

        if(_track == null)
            MusicManager.obj.Stop();
        else            
            MusicManager.obj.Play(_track);

        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
