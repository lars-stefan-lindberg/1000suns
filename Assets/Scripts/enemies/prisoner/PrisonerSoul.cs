using UnityEngine;

public class PrisonerSoul : MonoBehaviour
{
    public float timeToReachTarget = 1f;

    private Vector3 _target;
    private Vector3 _startPosition;
    private float _elapsedTime;
    private bool _hasTarget;

    public Vector3 Target
    {
        get => _target;
        set
        {
            _target = value;
            _startPosition = transform.position;
            _elapsedTime = 0f;
            _hasTarget = true;
            IsTargetReached = false;
        }
    }
    public bool IsTargetReached {get; private set;}

    void Update()
    {
        if (!_hasTarget || IsTargetReached)
        {
            return;
        }

        if (timeToReachTarget <= 0f)
        {
            transform.position = _target;
            IsTargetReached = true;
            return;
        }

        _elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsedTime / timeToReachTarget);
        transform.position = Vector3.Lerp(_startPosition, _target, t);

        if (t >= 1f)
        {
            IsTargetReached = true;
        }
    }
}
