using UnityEngine;

public class PlayCapeRoomAmbienceTrigger : MonoBehaviour
{
    [SerializeField] private AmbienceTrack _capeRoomAmbience;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            AmbienceManager.obj.Play(_capeRoomAmbience);
        }
    }
}
