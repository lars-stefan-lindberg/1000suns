using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    private Camera _mainCamera;

    [SerializeField] private float _parallaxEffect = 0.5f;
    [SerializeField] private float _smoothTime = 8f; // tweak: 5–12 is typical
    [SerializeField] private Direction _direction = Direction.Vertical;

    private Vector3 _startPosition;
    private Vector3 _currentVelocity;

    private enum Direction
    {
        Vertical,
        Horizontal
    }

    void Start()
    {
        _mainCamera = Camera.main;
        _startPosition = transform.position;
    }

    void LateUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector3 target = _startPosition;

        if (_direction == Direction.Vertical)
        {
            float cameraOffsetY = _mainCamera.transform.position.y;
            target.y = _startPosition.y + cameraOffsetY * _parallaxEffect;
        }
        else // Horizontal
        {
            float cameraOffsetX = _startPosition.x -_mainCamera.transform.position.x;
            target.x = _startPosition.x + cameraOffsetX * _parallaxEffect;
        }

        // Smoothly approach target (visual-only smoothing)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            target,
            ref _currentVelocity,
            1f / _smoothTime
        );
    }
}
