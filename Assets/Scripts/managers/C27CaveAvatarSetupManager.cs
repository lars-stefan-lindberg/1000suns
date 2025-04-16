using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C27CaveAvatarSetupManager : MonoBehaviour
{
    [SerializeField] private GameObject _caveAvatarStartingPoint;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(GameEventManager.obj.C27CutsceneCompleted) {
            return;
        }
        if (other.gameObject.CompareTag("Player")) {
            CaveAvatar.obj.SetTarget(_caveAvatarStartingPoint.transform);        
        }
    }
}
