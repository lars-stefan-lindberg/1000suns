using System;
using System.Collections;
using UnityEngine;

public class CaveCollectibleCreature : MonoBehaviour
{
    public event Action OnPicked;
    public event Action OnPermanentlyCollected;
    public bool IsPicked {get; set;}
    public bool IsPermanentlyCollected {get; set;}
    public bool IsDespawned = false;
    public Transform portal;

    [SerializeField] private string _id;

    private BoxCollider2D _collider;

    [SerializeField] private float _lerpSpeed = 2;
    [SerializeField] private float _tailPartLerpSpeed = 0.04f;
    [SerializeField] private GameObject _head;
    [SerializeField] private SpriteRenderer _headSpriteRenderer;
    [SerializeField] private GameObject[] _tailParts;
    [SerializeField] private float _floatDistance = 0.25f;
    private LightSprite2DFadeManager _lightSprite2DFadeManager;

    private float _floatDirectionTimer = 0;
    [SerializeField] private float _floatDirectionChangeTime = 1f;
    private bool _floatUp = true;

    private bool _startedTakeOff = false;
    private float _squeezeX = 1.25f;
    private float _squeezeY = 0.65f;
    private float _squeezeTime = 0.16f;
    private int _numberOfSqueezes = 3;

    private bool _hasTarget = false;
    private Transform _targetTransform;

    void Awake() {
        if(CollectibleManager.obj != null && CollectibleManager.obj.IsCollectiblePicked(_id)) {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        _collider = GetComponent<BoxCollider2D>();
        IsPicked = false;
        IsPermanentlyCollected = false;
        _lightSprite2DFadeManager = GetComponentInChildren<LightSprite2DFadeManager>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            _collider.enabled = false;
            IsPicked = true;
            OnPicked?.Invoke();
            StartCoroutine(FadeOutLight());
        }
    }

    private IEnumerator FadeOutLight()
    {
        _lightSprite2DFadeManager.StartFadeOut();
        yield return null;
    }

    public void SetSaved() {
        DontDestroyOnLoad(gameObject); 
        transform.SetParent(CollectibleManager.obj.transform);
    }

    void FixedUpdate()
    {
        if(IsPermanentlyCollected) {
            if(!_startedTakeOff) {
                _startedTakeOff = true;
                _targetTransform = portal; //Portal needs to be set before this is called
                OnPermanentlyCollected?.Invoke();
                StartCoroutine(PrepareTakeOff(_squeezeX, _squeezeY, _squeezeTime));
            }
        }

        Vector2 headTargetPosition;
        if(!IsPicked) { 
            headTargetPosition = transform.position;
        } else { //Follow
            bool isCaveAvatarFacingLeft = CaveAvatar.obj.IsFacingLeft();
            Transform target;   
            if(!_hasTarget) {
                CaveCollectibleCreature followingCollectible = CollectibleManager.obj.GetCaveCollectibleToFollow();
                if(followingCollectible == null) {
                    target = CaveAvatar.obj.GetHeadTransform();
                } else {
                    target = followingCollectible.GetHeadTransform();
                }
                _targetTransform = target;
                _hasTarget = true;
            } else {
                target = _targetTransform;
            }
            float headTargetX;
            if(!IsPermanentlyCollected) {
                headTargetX = isCaveAvatarFacingLeft ? target.position.x + 1.475f : target.position.x - 1.475f;
                _headSpriteRenderer.flipX = isCaveAvatarFacingLeft;
            } else
                headTargetX = target.position.x;
            headTargetPosition = new(headTargetX, target.position.y);
        }

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

        _head.transform.position = Vector2.Lerp(_head.transform.position, headTargetPosition, _lerpSpeed);

        _tailParts[0].transform.position = Vector2.Lerp(_tailParts[0].transform.position, _head.transform.position, _tailPartLerpSpeed);
        _tailParts[1].transform.position = Vector2.Lerp(_tailParts[1].transform.position, _tailParts[0].transform.position, _tailPartLerpSpeed);
        _tailParts[2].transform.position = Vector2.Lerp(_tailParts[2].transform.position, _tailParts[1].transform.position, _tailPartLerpSpeed);
        _tailParts[3].transform.position = Vector2.Lerp(_tailParts[3].transform.position, _tailParts[2].transform.position, _tailPartLerpSpeed);
        _tailParts[4].transform.position = Vector2.Lerp(_tailParts[4].transform.position, _tailParts[3].transform.position, _tailPartLerpSpeed);
    }

    private float _targetReachedMargin = 0.5f;
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

    public Transform GetHeadTransform() {
        return _head.transform;
    }

    private IEnumerator PrepareTakeOff(float xSqueeze, float ySqueeze, float seconds)
    {
        // Vector3 originalSize = Vector3.one;
        // Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        // int squeezeCounter = 0;
        // while(squeezeCounter < _numberOfSqueezes) {
        //     float time = 0f;
        //     while (time <= 1.0)
        //     {
        //         time += Time.deltaTime / seconds;
        //         _headSpriteRenderer.transform.localScale = Vector3.Lerp(originalSize, newSize, time);
        //         yield return null;
        //     }
        //     time = 0f;
        //     while(time <= 1.0)
        //     {
        //         time += Time.deltaTime / seconds;
        //         _headSpriteRenderer.transform.localScale = Vector3.Lerp(newSize, originalSize, time);
        //         yield return null;
        //     }
        //     squeezeCounter += 1;
        // }
        
        yield return new WaitUntil(() => IsTargetReached(_head.transform.position, _targetTransform.position));
        yield return new WaitForSeconds(0.2f);

        foreach(GameObject tail in _tailParts) {
            tail.SetActive(false);
        }

        float fadeMultiplier = 3f;
        Color startingColor = _headSpriteRenderer.color;
        while (_headSpriteRenderer.color.a > 0)
        {
            _headSpriteRenderer.color = new Color(startingColor.r, startingColor.g, startingColor.b, Mathf.MoveTowards(_headSpriteRenderer.color.a, 0, fadeMultiplier * Time.deltaTime));
            yield return null;
        }
        _headSpriteRenderer.enabled = false;
        
        yield return new WaitForSeconds(0.3f);

        gameObject.SetActive(false);
        IsDespawned = true;

        Destroy(gameObject, 1);
    }

    public void SetStartingPosition(Vector2 position) {
        float horizontalPosition = PlayerMovement.obj.isFacingLeft() ? position.x + 1.475f : position.x - 1.475f;
        SetPosition(new Vector2(horizontalPosition, position.y));
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

    public string GetId() {
        return _id;
    }
}
