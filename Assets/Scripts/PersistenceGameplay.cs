using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistenceGameplay : MonoBehaviour
{
    public static PersistenceGameplay obj;

    void Awake() {
        if(obj == null) {
            obj = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
}
