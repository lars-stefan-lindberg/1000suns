using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C27FloorTilemapManager : MonoBehaviour
{
    void Start() {
        if(GameEventManager.obj.C275FloorBroken)
            gameObject.SetActive(false);
    }
}
