using System.Collections;
using UnityEngine;

public class CaveCollectibleCreature : MonoBehaviour
{
    public bool IsCollected {get; set;}
    public bool IsPermantentlyCollected {get; set;}

    [SerializeField] private float _lerpSpeed = 2;
    [SerializeField] private float _tailPartLerpSpeed = 0.04f;
    [SerializeField] private GameObject _head;
    [SerializeField] private SpriteRenderer _headSpriteRenderer;
    [SerializeField] private GameObject[] _tailParts;
    [SerializeField] private GameObject _blackHole;
    [SerializeField] private float _floatDistance = 0.25f;

    private float _floatDirectionTimer = 0;
    [SerializeField] private float _floatDirectionChangeTime = 1f;
    private bool _floatUp = true;

    private bool IsIdle => !_preparingTakeOff || (!IsCollected && !IsPermantentlyCollected);

    private bool _startedTakeOff = false;
    private bool _preparingTakeOff = false;
    private float _squeezeX = 1.25f;
    private float _squeezeY = 0.65f;
    private float _squeezeTime = 0.08f;
    private int _numberOfSqueezes = 4;

    void Awake() {
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
        if(!IsCollected || _preparingTakeOff) { //Don't follow
            headTargetPosition = _head.transform.position;
        }
        else { //Follow
            bool isCaveAvatarFacingLeft = CaveAvatar.obj.IsFacingLeft();
            Transform avatarHead = CaveAvatar.obj.GetHead();
            float headTargetX = isCaveAvatarFacingLeft ? avatarHead.position.x + 1.475f : avatarHead.position.x - 1.475f;
            headTargetPosition = new(headTargetX, avatarHead.position.y);
            _headSpriteRenderer.flipX = isCaveAvatarFacingLeft;
        }

        //If uncollected collectible, go into idle/floating state
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
}