using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LowerMusicTrigger : MonoBehaviour
{
    [SerializeField] private float _lowerVolume;
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            if(SoundMixerManager.obj.GetMusicVolume() > _lowerVolume) {
                StartCoroutine(SoundMixerManager.obj.StartMusicFade(1.3f, _lowerVolume));
                GetComponent<BoxCollider2D>().enabled = false;
            }
        }
    }
}
