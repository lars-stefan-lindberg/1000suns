using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave34DeeRoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _hasShadowLash;
    [SerializeField] private GameObject _bgBlobs;

    void Start()
    {
        if(GameManager.obj.HasEvent(_hasShadowLash)) {
            _bgBlobs.SetActive(false);
        }
    }
}
