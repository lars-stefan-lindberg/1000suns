using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistenceCommon : MonoBehaviour
{
    public static PersistenceCommon obj;

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
