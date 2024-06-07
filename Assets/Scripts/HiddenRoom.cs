using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;

public class HiddenRoom : MonoBehaviour
{
    public bool reveal = false;
    public GameObject visibleLayer;
    private Animator _visibleLayerAnimator;

    private void Awake() {
        _visibleLayerAnimator = visibleLayer.GetComponent<Animator>();
    }

    private void FixedUpdate() {
    }
}
