using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    private Camera _mainCamera;
    [SerializeField] private float _parallaxEffect;
    private float _startPosition;
    void Start()
    {
        _startPosition = transform.position.y;
        _mainCamera = Camera.main;
        UpdatePosition();
    }

    void FixedUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition() {
        float distance = _mainCamera.transform.position.y * _parallaxEffect;
        transform.position = new Vector2(transform.position.x, _startPosition + distance);
    }
}
