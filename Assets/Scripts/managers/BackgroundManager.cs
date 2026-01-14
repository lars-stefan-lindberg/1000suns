using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    private Camera _mainCamera;
    [SerializeField] private float _parallaxEffect;
    [SerializeField] private Direction _direction = Direction.Vertical;
    private Vector2 _startPosition;

    private enum Direction {
        Vertical,
        Horizontal
    }

    void Start()
    {
        _mainCamera = Camera.main;
        _startPosition = transform.position;
        UpdatePosition();
    }

    void FixedUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition() {
        if(_direction == Direction.Vertical) {
            float distance = _mainCamera.transform.position.y * _parallaxEffect;
            transform.position = new Vector2(transform.position.x, _startPosition.y + distance);
        }
        else if(_direction == Direction.Horizontal) {
            float distance = (_mainCamera.transform.position.x - _startPosition.x) * _parallaxEffect;
            transform.position = new Vector2(_startPosition.x - distance, transform.position.y);
        }
    }
}
