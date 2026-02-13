using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayRegularCaveAmbienceTrigger : MonoBehaviour
{
    [SerializeField] private AmbienceTrack _caveMainAmbience;
    
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            AmbienceManager.obj.Play(_caveMainAmbience);
        }
    }
}
