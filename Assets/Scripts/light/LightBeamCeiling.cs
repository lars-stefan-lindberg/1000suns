using FunkyCode;
using UnityEngine;

public class LightBeamCeiling : MonoBehaviour
{
    [SerializeField] private float _minHorizontalPos = -3f;
    [SerializeField] private float _maxHorizontalPos = 3f;
    [SerializeField] private Direction _startDirection = Direction.Left;

    [Header("Movement")]
    [SerializeField] private float _acceleration = 2.5f;
    [SerializeField] private float _deceleration = 3.5f;
    [SerializeField] private float _maxSpeed = 2f;
    [SerializeField] private float _maxSpeedRandomDelta = 0.4f;
    [SerializeField] private float _arrivePositionEpsilon = 0.01f;
    [SerializeField] private float _arriveSpeedEpsilon = 0.02f;
    [SerializeField] private float _edgeHoldSeconds = 0.35f;

    [Header("Alpha Fade")]
    [SerializeField] private float _alphaOnIntervalMin = 0.8f;
    [SerializeField] private float _alphaOnIntervalMax = 2.5f;
    [SerializeField] private float _alphaOffIntervalMin = 0.25f;
    [SerializeField] private float _alphaOffIntervalMax = 0.8f;
    [SerializeField] private float _alphaFadeSpeedMin = 0.8f;
    [SerializeField] private float _alphaFadeSpeedMax = 2.5f;

    private enum Direction { Left, Right }

    private LightSprite2D _lightSprite;
    private float _startAlpha;
    private float _position;

    private Direction _direction;
    private float _speed;
    private float _currentMaxSpeed;

    private float _currentAlpha;
    private float _targetAlpha;
    private float _currentAlphaFadeSpeed;

    private float _edgeHoldTimer;


    void Start()
    {
        _lightSprite = GetComponent<LightSprite2D>();
        if (_lightSprite != null)
            _startAlpha = _lightSprite.meshMode.alpha;

        _direction = _startDirection;

        if (_direction == Direction.Left)
            _position = _maxHorizontalPos;
        else
            _position = _minHorizontalPos;

        var p = transform.localPosition;
        p.x = _position;
        transform.localPosition = p;

        _speed = 0f;
        PickNewMaxSpeed();
        _edgeHoldTimer = 0f;

        _currentAlpha = _startAlpha;
        _targetAlpha = _startAlpha;
        _currentAlphaFadeSpeed = Random.Range(_alphaFadeSpeedMin, _alphaFadeSpeedMax);
        ApplyAlpha(_currentAlpha);

        StartCoroutine(AlphaToggleLoop());
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f)
            return;

        UpdateMovement(dt);
        UpdateAlpha(dt);
    }

    private void UpdateMovement(float dt)
    {
        if (_edgeHoldTimer > 0f)
        {
            _edgeHoldTimer -= dt;
            if (_edgeHoldTimer < 0f)
                _edgeHoldTimer = 0f;
            return;
        }

        float targetPos = _direction == Direction.Left ? _minHorizontalPos : _maxHorizontalPos;
        float remaining = Mathf.Abs(targetPos - _position);

        if (remaining <= _arrivePositionEpsilon && _speed <= _arriveSpeedEpsilon)
        {
            _position = targetPos;
            _speed = 0f;
            ApplyPosition();

            _direction = _direction == Direction.Left ? Direction.Right : Direction.Left;
            PickNewMaxSpeed();
            _edgeHoldTimer = Mathf.Max(0f, _edgeHoldSeconds);
            return;
        }

        float requiredStoppingDistance = (_speed * _speed) / (2f * Mathf.Max(0.0001f, _deceleration));
        bool shouldDecelerate = remaining <= requiredStoppingDistance;

        if (shouldDecelerate)
            _speed = Mathf.Max(0f, _speed - _deceleration * dt);
        else
            _speed = Mathf.Min(_currentMaxSpeed, _speed + _acceleration * dt);

        float dirSign = _direction == Direction.Left ? -1f : 1f;
        float newPos = _position + dirSign * _speed * dt;

        if ((_direction == Direction.Left && newPos <= targetPos) || (_direction == Direction.Right && newPos >= targetPos))
            newPos = targetPos;

        _position = newPos;
        ApplyPosition();
    }

    private void ApplyPosition()
    {
        var p = transform.localPosition;
        p.x = _position;
        transform.localPosition = p;
    }

    private void PickNewMaxSpeed()
    {
        float delta = Mathf.Abs(_maxSpeedRandomDelta);
        _currentMaxSpeed = Mathf.Max(0f, _maxSpeed + Random.Range(-delta, delta));
    }

    private void UpdateAlpha(float dt)
    {
        if (_lightSprite == null)
            return;

        _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha, _currentAlphaFadeSpeed * dt);
        ApplyAlpha(_currentAlpha);
    }

    private void ApplyAlpha(float a)
    {
        if (_lightSprite == null)
            return;

        _lightSprite.meshMode.alpha = a;
        var c = _lightSprite.color;
        c.a = a;
        _lightSprite.color = c;
    }

    private System.Collections.IEnumerator AlphaToggleLoop()
    {
        while (true)
        {
            bool currentlyOff = _targetAlpha <= 0.0001f;
            float wait = currentlyOff
                ? Random.Range(_alphaOffIntervalMin, _alphaOffIntervalMax)
                : Random.Range(_alphaOnIntervalMin, _alphaOnIntervalMax);
            if (wait < 0f)
                wait = 0f;

            yield return new WaitForSeconds(wait);

            _targetAlpha = _targetAlpha <= 0.0001f ? _startAlpha : 0f;
            _currentAlphaFadeSpeed = Random.Range(_alphaFadeSpeedMin, _alphaFadeSpeedMax);
        }
    }
}
