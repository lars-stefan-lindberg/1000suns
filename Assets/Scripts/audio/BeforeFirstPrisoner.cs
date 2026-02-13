using System.Collections;
using UnityEngine;

public class BeforeFirstPrisoner : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 5f;
    [SerializeField] private MusicTrack _musicTrack;
    [SerializeField] private AmbienceTrack _caveMainAmbience;    

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            StartCoroutine(SwitchAudio());
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private IEnumerator SwitchAudio() {
        AmbienceManager.obj.Play(_caveMainAmbience);
        
        MusicManager.obj.Play(_musicTrack);
        yield return null;
    }
}
