using UnityEngine;

public class ForestBirdAmbienceToggle : MonoBehaviour
{
    [SerializeField] private AmbienceTrack _birdAmbience;
    [SerializeField] private bool _switchOff = true;

    void OnTriggerEnter2D(Collider2D collider) {
        if(!collider.CompareTag("Player"))
            return;

        if(_switchOff) {
            AmbienceManager.obj.Stop();
        } else {
            AmbienceManager.obj.Play(_birdAmbience);
        }
    }
}
