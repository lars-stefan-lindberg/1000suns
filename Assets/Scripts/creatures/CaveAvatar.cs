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
    private Animator _animator;
    public bool IsFollowingPlayer {get; set;}

    [SerializeField] private float _floatDistance = 0.25f;

    private float _floatDirectionTimer = 0;
    [SerializeField] private float _floatDirectionChangeTime = 1f;
    private bool _floatUp = true;

    private Coroutine _eyeColorCoroutine;
    [SerializeField] private float _eyeColorLerpDuration = 0.5f;

    private Vector2 _linearMoveStartPos;
    private Vector2 _linearMoveTargetPos;
    private float _linearMoveElapsed = 0f;
    [SerializeField] private float _moveSpeed = 5f; // units per second for eased movement
    [SerializeField] private float _linearMoveDuration = 0.5f;
    private bool _isLinearMoving = false;
    private bool _isFloatingEnabled = true;

    void Awake() {
        obj = this;
    }

    void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        IsFollowingPlayer = false;
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
                _headSpriteRenderer.flipX = _head.transform.position.x >= _target.transform.position.x;
            }
        }

        bool isAtTarget = IsTargetReached(_head.transform.position, headTargetPosition);

        // Floating logic: always runs if close enough to target
        if(isAtTarget) {
            if(_isFloatingEnabled) {
                _floatDirectionTimer += Time.deltaTime;
                if(_floatDirectionTimer > _floatDirectionChangeTime) {
                    _floatUp = !_floatUp;
                    _floatDirectionTimer = 0;
                }
                headTargetPosition = new Vector2(headTargetPosition.x, _floatUp ? headTargetPosition.y + _floatDistance : headTargetPosition.y - _floatDistance);
            }
            // Always float the head when at target, regardless of _target
            _head.transform.position = Vector2.Lerp(_head.transform.position, headTargetPosition, _followPlayerLerpSpeed);
        } else if(IsFollowingPlayer) {
            _head.transform.position = Vector2.Lerp(_head.transform.position, headTargetPosition, _followPlayerLerpSpeed);
        } else if(_target != null) {
            // Eased move logic
            if(!_isLinearMoving) {
                _linearMoveStartPos = _head.transform.position;
                _linearMoveTargetPos = _target.transform.position;
                float distance = Vector2.Distance(_linearMoveStartPos, _linearMoveTargetPos);
                _linearMoveDuration = (distance > 0.01f && _moveSpeed > 0.01f) ? distance / _moveSpeed : 0.1f;
                _linearMoveElapsed = 0f;
                _isLinearMoving = true;
            }
            if(_isLinearMoving) {
                _linearMoveElapsed += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(_linearMoveElapsed / _linearMoveDuration);
                // Smoothstep easing: t = t*t*(3-2*t)
                float easedT = t * t * (3f - 2f * t);
                _head.transform.position = Vector2.Lerp(_linearMoveStartPos, _linearMoveTargetPos, easedT);
                if(t >= 1f) {
                    _isLinearMoving = false;
                }
            }
        } else {
            _head.transform.position = Vector2.Lerp(_head.transform.position, headTargetPosition, _approachTargetLerpSpeed);
        }

        _tailParts[0].transform.position = Vector2.Lerp(_tailParts[0].transform.position, _head.transform.position, _tailPartLerpSpeed);
        _tailParts[1].transform.position = Vector2.Lerp(_tailParts[1].transform.position, _tailParts[0].transform.position, _tailPartLerpSpeed);
        _tailParts[2].transform.position = Vector2.Lerp(_tailParts[2].transform.position, _tailParts[1].transform.position, _tailPartLerpSpeed);
        _tailParts[3].transform.position = Vector2.Lerp(_tailParts[3].transform.position, _tailParts[2].transform.position, _tailPartLerpSpeed);
        _tailParts[4].transform.position = Vector2.Lerp(_tailParts[4].transform.position, _tailParts[3].transform.position, _tailPartLerpSpeed);
    }

    [ContextMenu("Attack")]
    public void Attack() {
        _animator.SetTrigger("attack");
        CameraShakeManager.obj.ForcePushShake();
        SoundFXManager.obj.PlayForcePushExecute(transform);
    }

    public void NudgeUpwards() {
        StartCoroutine(NudgeUpwardsCoroutine(0.4f));
    }

    private IEnumerator NudgeUpwardsCoroutine(float duration) {
        SetPosition(transform.position + Vector3.up * 0.125f, false);
        float timer = 0;
        while(timer < duration) {
            timer += Time.deltaTime;
            yield return null;
        }
        SetPosition(transform.position - Vector3.up * 0.125f, false);
    }

    public void SetFloatingEnabled(bool enabled) {
        _isFloatingEnabled = enabled;
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

    public void SetFollowPlayerStartingPosition() {
        Vector2 headTargetPosition = PlayerMovement.obj.isFacingLeft() ? _playerTargetRight.position : _playerTargetLeft.position;
        SetPosition(headTargetPosition);
    }

    public void SetStartingPositionInRoom27() {
        SetPosition(new Vector2(1390.125f, -16f));
        IsFollowingPlayer = false;
        _target = null;
    }

    public void SetStartingPositionInRoom30() {
        SetPosition(new Vector2(1475.125f, -14.875f));
        IsFollowingPlayer = false;
        _target = null;
        //Also this is a good time to reset the red eyes color of the sprite, and set the red eye animation layer instead
        _headSpriteRenderer.color = Color.white;
        Debug.Log("Set red eye animation layer");
        Debug.Log("Red eye animation layer index: " + _animator.GetLayerIndex("red_eyes"));
        Debug.Log("Layer count: " + _animator.layerCount);
        _animator.SetLayerWeight(1, 1);
        Debug.Log(_animator.gameObject.name);
    }

    public void SetStartingPositionInRoom31() {
        SetPosition(new Vector2(1518f, -5.25f));
        IsFollowingPlayer = false;
        _target = null;
        _headSpriteRenderer.flipX = true;
    }

    public void SetStartingPositionInRoom32() {
        SetPosition(new Vector2(1598.75f, -16.875f));
        IsFollowingPlayer = false;
        _target = null;
        _headSpriteRenderer.flipX = true;
    }

    public void SetStartingPositionInRoom33() {
        SetPosition(new Vector2(1639.875f, -9.125f));
        IsFollowingPlayer = false;
        _target = null;
        _headSpriteRenderer.flipX = true;
    }

    public void SetStartingPositionInRoom34() {
        SetPosition(new Vector2(1797f, -14.125f));
        IsFollowingPlayer = false;
        _target = null;
        _headSpriteRenderer.flipX = true;
    }

    public void SetStartingPositionInRoom35() {
        SetPosition(new Vector2(1865.75f, -18.75f));
        IsFollowingPlayer = false;
        _target = null;
        _headSpriteRenderer.flipX = false;
    }

    public void SetStartingPositionInRoom1() {
        SetPosition(new Vector2(241f, 21.25f), false);
        IsFollowingPlayer = false;
        _target = null;
        SetFloatingEnabled(false);
    }
    public void SetPosition(Vector2 target, bool adjustFlipXToPlayer = true) {
        if(adjustFlipXToPlayer) {
            bool isPlayerFacingLeft = PlayerMovement.obj.isFacingLeft();
            _headSpriteRenderer.flipX = isPlayerFacingLeft;   
        }

        transform.position = target;
        _head.transform.position = target;
        for (int i = 0; i < _tailParts.Length; i++) {
            _tailParts[i].transform.position = target;
        }

        _target = null;
    }

    public void SetFlipX(bool flipX) {
        _headSpriteRenderer.flipX = flipX;
    }

    public void SetTarget(Transform target) {
        SetTarget(target, 5);
    }

    public void SetTarget(Transform target, float moveSpeed) {
        IsFollowingPlayer = false;
        _target = target;
        // Start new eased move if target is set
        if(_target != null) {
            _linearMoveStartPos = _head.transform.position;
            _linearMoveTargetPos = _target.transform.position;
            _moveSpeed = moveSpeed;
            float distance = Vector2.Distance(_linearMoveStartPos, _linearMoveTargetPos);
            _linearMoveDuration = (distance > 0.01f && _moveSpeed > 0.01f) ? distance / _moveSpeed : 0.1f;
            _linearMoveElapsed = 0f;
            _isLinearMoving = true;
        } else {
            _isLinearMoving = false;
        }
    }

    public void SetEyeColor(Color color) {
        if (_eyeColorCoroutine != null) {
            StopCoroutine(_eyeColorCoroutine);
        }
        _eyeColorCoroutine = StartCoroutine(LerpEyeColor(color, _eyeColorLerpDuration));
    }

    private IEnumerator LerpEyeColor(Color targetColor, float duration) {
        Color startColor = _headSpriteRenderer.color;
        float time = 0f;
        while (time < duration) {
            _headSpriteRenderer.color = Color.Lerp(startColor, targetColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        _headSpriteRenderer.color = targetColor;
        _eyeColorCoroutine = null;
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

    void OnDestroy() {
        obj = null;
    }
}
