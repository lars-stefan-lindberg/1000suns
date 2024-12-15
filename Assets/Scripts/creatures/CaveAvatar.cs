using UnityEditor;
using UnityEngine;

public class CaveAvatar : MonoBehaviour
{
    public static CaveAvatar obj;

    [SerializeField] private float _lerpSpeed = 2;
    [SerializeField] private float _tailPartLerpSpeed = 0.04f;
    [SerializeField] private GameObject _head;
    [SerializeField] private SpriteRenderer _headSpriteRenderer;
    [SerializeField] private GameObject[] _tailParts;

    private readonly float _distanceFromPlayerX = 1.475f;
    private readonly float _distanceFromPlayerY = 1.475f;
    [SerializeField] private float _floatDistance = 0.25f;
    [SerializeField] private float _floatSpeed = 0.1f;

    private float _floatDirectionTimer = 0;
    [SerializeField] private float _floatDirectionChangeTime = 1f;
    private bool _floatUp = true;


    void Awake() {
        obj = this;
    }

    void FixedUpdate()
    {
        bool isPlayerFacingLeft = PlayerMovement.obj.isFacingLeft();
        _headSpriteRenderer.flipX = isPlayerFacingLeft;    
        float headTargetX = isPlayerFacingLeft ? Player.obj.transform.position.x + _distanceFromPlayerX : Player.obj.transform.position.x - _distanceFromPlayerX;
        Vector2 headTargetPosition = new (headTargetX, Player.obj.transform.position.y + _distanceFromPlayerY);

        //If player is standing still go into idle/floating state
        if(Player.obj.rigidBody.velocity.x == 0) {
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

    void OnDestroy() {
        obj = null;
    }
}
