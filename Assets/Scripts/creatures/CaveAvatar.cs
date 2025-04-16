using System.Collections;
using UnityEngine;

public class CaveAvatar : MonoBehaviour
{
    public static CaveAvatar obj;

    [SerializeField] private float _followPlayerLerpSpeed = 0.05f;
    [SerializeField] private float _approachTargetLerpSpeed = 0.03f;
    [SerializeField] private float _tailPartLerpSpeed = 0.04f;
    [SerializeField] private GameObject _head;
    [SerializeField] private SpriteRenderer _headSpriteRenderer;
    [SerializeField] private GameObject[] _tailParts;
    [SerializeField] private Transform _playerTargetLeft;
    [SerializeField] private Transform _playerTargetRight;
    private Transform _target;
    public bool IsFollowingPlayer {get; set;}

    [SerializeField] private float _floatDistance = 0.25f;

    private float _floatDirectionTimer = 0;
    [SerializeField] private float _floatDirectionChangeTime = 1f;
    private bool _floatUp = true;

    void Awake() {
        obj = this;
        IsFollowingPlayer = true;
    }

    void FixedUpdate()
    {
        Vector2 headTargetPosition;

        if(IsFollowingPlayer) {
            bool isPlayerFacingLeft = PlayerMovement.obj.isFacingLeft();
            _headSpriteRenderer.flipX = isPlayerFacingLeft;   
            headTargetPosition = isPlayerFacingLeft ? _playerTargetRight.position : _playerTargetLeft.position;
        } else {
            if(_target == null) {
                headTargetPosition = _head.transform.position;
            } else {
                headTargetPosition = _target.transform.position;
                _headSpriteRenderer.flipX = _head.transform.position.x > _target.transform.position.x;
            }
        }

        //If player is standing still, go into idle/floating state
        if(IsTargetReached(_head.transform.position, headTargetPosition)) {
            _floatDirectionTimer += Time.deltaTime;
            if(_floatDirectionTimer > _floatDirectionChangeTime) {
                _floatUp = !_floatUp;
                _floatDirectionTimer = 0;
            }
            headTargetPosition = new Vector2(headTargetPosition.x, _floatUp ? headTargetPosition.y + _floatDistance : headTargetPosition.y - _floatDistance);
        } else {
            _floatDirectionTimer = 0;
        }

        if(IsFollowingPlayer)
            _head.transform.position = Vector2.Lerp(_head.transform.position, headTargetPosition, _followPlayerLerpSpeed);
        else {
            _head.transform.position = Vector2.Lerp(_head.transform.position, headTargetPosition, _approachTargetLerpSpeed);
        }

        _tailParts[0].transform.position = Vector2.Lerp(_tailParts[0].transform.position, _head.transform.position, _tailPartLerpSpeed);
        _tailParts[1].transform.position = Vector2.Lerp(_tailParts[1].transform.position, _tailParts[0].transform.position, _tailPartLerpSpeed);
        _tailParts[2].transform.position = Vector2.Lerp(_tailParts[2].transform.position, _tailParts[1].transform.position, _tailPartLerpSpeed);
        _tailParts[3].transform.position = Vector2.Lerp(_tailParts[3].transform.position, _tailParts[2].transform.position, _tailPartLerpSpeed);
        _tailParts[4].transform.position = Vector2.Lerp(_tailParts[4].transform.position, _tailParts[3].transform.position, _tailPartLerpSpeed);
    }

    public float _targetReachedMargin = 0.5f;
    private bool IsTargetReached(Vector2 position, Vector2 target) {
        if(
            IsWithinInterval(position.x, target.x - _targetReachedMargin, target.x + _targetReachedMargin)
            && IsWithinInterval(position.y, target.y - _targetReachedMargin, target.y + _targetReachedMargin)
        ) {
            return true;
        }
        return false;
    }

    private bool IsWithinInterval(float value, float lowerBound, float upperBound)
    {
        return value >= lowerBound && value <= upperBound;
    }

    public void SetCaveStartingCoordinates() {
        SetPosition(new Vector2(191.975f, 24.89f));
    }

    public void SetStartingPosition() {
        Vector2 headTargetPosition = PlayerMovement.obj.isFacingLeft() ? _playerTargetRight.position : _playerTargetLeft.position;
        SetPosition(headTargetPosition);
    }

    public void SetStartingPositionInRoom27() {
        SetPosition(new Vector2(1390.125f, -16f));
        IsFollowingPlayer = false;
        _target = null;
    }

    public void SetPosition(Vector2 target) {
        bool isPlayerFacingLeft = PlayerMovement.obj.isFacingLeft();
        _headSpriteRenderer.flipX = isPlayerFacingLeft;   

        transform.position = target;
        _head.transform.position = target;
        for (int i = 0; i < _tailParts.Length; i++) {
            _tailParts[i].transform.position = target;
        }
    }

    public void SetTarget(Transform target) {
        IsFollowingPlayer = false;
        _target = target;
    }

    public void FollowPlayer() {
        _target = null;
        IsFollowingPlayer = true;
    }

    public bool IsFacingLeft()
    {
        return _headSpriteRenderer.flipX;
    }

    public Vector2 GetTarget() {
        if(IsFollowingPlayer) {
            bool isPlayerFacingLeft = PlayerMovement.obj.isFacingLeft();
            _headSpriteRenderer.flipX = isPlayerFacingLeft;   
            return isPlayerFacingLeft ? _playerTargetRight.position : _playerTargetLeft.position;
        } else {
            return _target.transform.position;
        }
    }

    public Transform GetHeadTransform() {
        return _head.transform;
    }

    public bool IsFloatingUp() {
        return _floatUp;
    }

    void OnDestroy() {
        obj = null;
    }
}
