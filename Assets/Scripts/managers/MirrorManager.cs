using UnityEngine;

public class MirrorManager : MonoBehaviour
{
    [SerializeField] private Transform _mirrorReflections;
    [SerializeField] private float _parallaxEffect;
    [SerializeField] private Transform _roomStart;
    private float _mirrorReflectionsStartPosition;
    private Camera _mainCamera;

    void Start()
    {
        _mirrorReflectionsStartPosition = _mirrorReflections.position.y;
        _mainCamera = Camera.main;
    }

    void FixedUpdate()
    {
        float distance = (_mainCamera.transform.position.x - _roomStart.position.x) * _parallaxEffect;
        _mirrorReflections.position = new Vector2(_mirrorReflections.position.x, _mirrorReflectionsStartPosition - distance);
    }
}
