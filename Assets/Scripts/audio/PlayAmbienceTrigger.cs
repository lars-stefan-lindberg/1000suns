using UnityEngine;

public class PlayAmbienceTrigger : MonoBehaviour
{
    [SerializeField] private AmbienceTrack _track;
    [SerializeField] private bool _stopMusic = false;

    void OnTriggerEnter2D(Collider2D collider) {
        if(!collider.CompareTag("Player"))
            return;

        if(_stopMusic)
            MusicManager.obj.Stop();
            
        AmbienceManager.obj.Play(_track);
    }
}
