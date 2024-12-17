using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CaveAvatar : MonoBehaviour
{
    public static CaveAvatar obj;

    [SerializeField] private bool _isCollectible = false;
    public bool IsCollected {get; set;}
    public bool IsPermantentlyCollected {get; set;}
    [SerializeField] private float _collectibleDistanceFromPlayerMultiplier = 2.5f;

    [SerializeField] private float _lerpSpeed = 2;
    [SerializeField] private float _tailPartLerpSpeed = 0.04f;
    [SerializeField] private GameObject _head;
    [SerializeField] private SpriteRenderer _headSpriteRenderer;
    [SerializeField] private GameObject[] _tailParts;
    [SerializeField] private GameObject _blackHole;

    private readonly float _distanceFromPlayerX = 1.475f;
    private readonly float _distanceFromPlayerY = 1.475f;
    [SerializeField] private float _floatDistance = 0.25f;

    private float _floatDirectionTimer = 0;
    [SerializeField] private float _floatDirectionChangeTime = 1f;
    private bool _floatUp = true;

    private bool IsIdle => !_preparingTakeOff && (Player.obj.rigidBody.velocity.x == 0 || (_isCollectible && !IsCollected && !IsPermantentlyCollected));

    private bool _startedTakeOff = false;
    private bool _preparingTakeOff = false;
    private float _squeezeX = 1.25f;
    private float _squeezeY = 0.65f;
    private float _squeezeTime = 0.08f;
    private int _numberOfSqueezes = 4;

    void Awake() {
        obj = this;
        IsCollected = false;
        IsPermantentlyCollected = false;
    }

    void FixedUpdate()
    {
        if(IsPermantentlyCollected) {
            if(!_startedTakeOff) {
                _startedTakeOff = true;
                _preparingTakeOff = true;
                SoundFXManager.obj.PlayCollectiblePickup(transform);
                StartCoroutine(PrepareTakeOff(_squeezeX, _squeezeY, _squeezeTime));
            }
        }

        Vector2 headTargetPosition;
        if(_isCollectible && !IsCollected) { //Don't follow
            float headTargetX = _head.transform.position.x;
            headTargetPosition = new(headTargetX, _head.transform.position.y);
        } else if(_preparingTakeOff) {
            headTargetPosition = _head.transform.position;
        }
        else { //Follow
            bool isPlayerFacingLeft = PlayerMovement.obj.isFacingLeft();
            _headSpriteRenderer.flipX = isPlayerFacingLeft;    
            float distanceFromPlayerX = _isCollectible ? _distanceFromPlayerX * _collectibleDistanceFromPlayerMultiplier : _distanceFromPlayerX;
            float headTargetX = isPlayerFacingLeft ? Player.obj.transform.position.x + distanceFromPlayerX : Player.obj.transform.position.x - distanceFromPlayerX;
            headTargetPosition = new (headTargetX, Player.obj.transform.position.y + _distanceFromPlayerY);
        }

        //If player is standing still, or is uncollected collectible, go into idle/floating state
        if(IsIdle) {
            _floatDirectionTimer += Time.deltaTime;
            if(_floatDirectionTimer > _floatDirectionChangeTime) {
                _floatUp = !_floatUp;
                _floatDirectionTimer = 0;
            }
            headTargetPosition = new Vector2(headTargetPosition.x, _floatUp ? headTargetPosition.y + _floatDistance : headTargetPosition.y - _floatDistance);
        } else {
            _floatDirectionTimer = 0;
        }

        _head.transform.position = Vector2.Lerp(_head.transform.position, headTargetPosition, _lerpSpeed);

        _tailParts[0].transform.position = Vector2.Lerp(_tailParts[0].transform.position, _head.transform.position, _tailPartLerpSpeed);
        _tailParts[1].transform.position = Vector2.Lerp(_tailParts[1].transform.position, _tailParts[0].transform.position, _tailPartLerpSpeed);
        _tailParts[2].transform.position = Vector2.Lerp(_tailParts[2].transform.position, _tailParts[1].transform.position, _tailPartLerpSpeed);
        _tailParts[3].transform.position = Vector2.Lerp(_tailParts[3].transform.position, _tailParts[2].transform.position, _tailPartLerpSpeed);
        _tailParts[4].transform.position = Vector2.Lerp(_tailParts[4].transform.position, _tailParts[3].transform.position, _tailPartLerpSpeed);
    }

    public void SetCaveStartingCoordinates() {
        transform.position = new Vector2(233.875f, 78.375f);
    }

    private IEnumerator PrepareTakeOff(float xSqueeze, float ySqueeze, float seconds)
    {
        yield return new WaitForSeconds(0.1f);
        foreach(GameObject tail in _tailParts) {
            tail.SetActive(false);
        }
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        int squeezeCounter = 0;
        while(squeezeCounter < _numberOfSqueezes) {
            float time = 0f;
            while (time <= 1.0)
            {
                time += Time.deltaTime / seconds;
                _headSpriteRenderer.transform.localScale = Vector3.Lerp(originalSize, newSize, time);
                yield return null;
            }
            time = 0f;
            while(time <= 1.0)
            {
                time += Time.deltaTime / seconds;
                _headSpriteRenderer.transform.localScale = Vector3.Lerp(newSize, originalSize, time);
                yield return null;
            }
            squeezeCounter += 1;
        }

        _blackHole.SetActive(true);
        yield return new WaitForSeconds(1f);
        
        _headSpriteRenderer.enabled = false;
        _blackHole.GetComponent<BlackHole>().Despawn();

        Destroy(gameObject, 5);
    }

    void OnDestroy() {
        obj = null;
    }
}
