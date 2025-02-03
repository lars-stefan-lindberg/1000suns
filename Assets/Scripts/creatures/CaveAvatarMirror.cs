using System.Collections;
using UnityEngine;

public class CaveAvatarMirror : MonoBehaviour
{
    [SerializeField] private float _followPlayerLerpSpeed = 0.05f;
    [SerializeField] private float _tailPartLerpSpeed = 0.04f;
    [SerializeField] private GameObject _head;
    [SerializeField] private SpriteRenderer _headSpriteRenderer;
    [SerializeField] private GameObject[] _tailParts;
    [SerializeField] private Transform _playerTargetLeft;
    [SerializeField] private Transform _playerTargetRight;

    [SerializeField] private float _floatDistance = 0.25f;

    private float _floatDirectionTimer = 0;
    [SerializeField] private float _floatDirectionChangeTime = 1f;
    private bool _floatUp = true;

    void Awake() {
        //Make sure to sync floating to the main cave avatar
        _floatUp = CaveAvatar.obj.IsFloatingUp();
    }

    void FixedUpdate()
    {
        Vector2 headTargetPosition;

        bool isPlayerFacingLeft = PlayerMovement.obj.isFacingLeft();
        _headSpriteRenderer.flipX = isPlayerFacingLeft;   
        headTargetPosition = isPlayerFacingLeft ? _playerTargetRight.position : _playerTargetLeft.position;

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

        _head.transform.position = Vector2.Lerp(_head.transform.position, headTargetPosition, _followPlayerLerpSpeed);

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
}
