using UnityEngine;

public class MirrorManager : MonoBehaviour
{
    [SerializeField] private Transform _mirrorReflections;
    [SerializeField] private float _parallaxEffect;
    private float _mirrorReflectionsStartPosition;
    private float _roomStartX = 802.75f;

    void Start()
    {
        _mirrorReflectionsStartPosition = _mirrorReflections.position.y;
    }

    void FixedUpdate()
    {
        float distance = (Player.obj.transform.position.x - _roomStartX) * _parallaxEffect;
        _mirrorReflections.position = new Vector2(_mirrorReflections.position.x, _mirrorReflectionsStartPosition + distance);
    }
}
