using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CutsceneControls : MonoBehaviour
{
    public static CutsceneControls obj;

    private PlayerInput _cutsceneControls;

    void Awake() {
        obj = this;
    }

    void OnDestroy() {
        obj = null;
    }

    void Start() {
        _cutsceneControls = GetComponent<PlayerInput>();
    }

    public void Enable() {
        _cutsceneControls.enabled = true;
    }
    
    public void Disable() {
        _cutsceneControls.enabled = false;
    }
}
