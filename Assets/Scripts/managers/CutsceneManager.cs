using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager obj;

    public bool capePicked = false;
    public bool babyPrisonerAlerted = false;
    public bool firstPrisonerSpawned = false;

    void Awake() {
        obj = this;
    }

    void OnDestroy() {
        obj = null;
    }
}
