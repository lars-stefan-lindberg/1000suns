using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndRoomShowBackgroundBlobsTrigger : MonoBehaviour
{
    [SerializeField] private C26CutsceneManager _cutsceneManager;

    void OnTriggerEnter2D(Collider2D other) {
        if(GameEventManager.obj.C26CutsceneCompleted) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            _cutsceneManager.FadeInBlobs();
        }
    }
}
