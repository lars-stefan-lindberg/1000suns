using System.Collections;
using UnityEngine;

public class BreakableFloor : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private BoxCollider2D _floorCollider;
    private BoxCollider2D _playerOnTopDetectionCollider;
    public ParticleSystem shakeAnimation;

    public bool unbreakable = false;
    public int collisionsBeforeBreak = 3;
    private int _collisionCount = 0;
    private bool _breakFloor = false;
    private bool _shakeFloor = false;
    public float fadeMultiplier = 0.1f;
    public int numberOfShakeParticles = 50;
    private bool _fadeSprite = false;
    public float shakeDistance = 0.1f;
    public float shakeTime = 0.12f;
    public float shakeFrameWait = 0.08f;
    private float _originYPosition;

    private void Awake() {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _originYPosition = _spriteRenderer.transform.position.y;
        _playerOnTopDetectionCollider = GetComponent<BoxCollider2D>();
    }

    private readonly float _collisionMargin = 0.3f;
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            //Check if player is landing on top
            Bounds playerCollisionBounds = other.bounds;
            Bounds floorBounds = _playerOnTopDetectionCollider.bounds;
            Vector2 playerBottom = new(playerCollisionBounds.center.x, playerCollisionBounds.center.y - playerCollisionBounds.extents.y);
            Vector2 floorTop = new(floorBounds.center.x, floorBounds.center.y + floorBounds.extents.y); 

            if(playerBottom.y + _collisionMargin > floorTop.y) {
                _collisionCount += 1;
                if(_collisionCount == collisionsBeforeBreak) {
                    _breakFloor = true;
                } else {
                    _shakeFloor = true;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if(_shakeFloor) {
            _shakeFloor = false;
            StartCoroutine(ShakeWall());
        }
        if(_breakFloor) {
            _breakFloor = false;
            SoundFXManager.obj.PlayBreakableWallBreak(transform);
            shakeAnimation.Emit(numberOfShakeParticles);
            _floorCollider.enabled = false;
            _playerOnTopDetectionCollider.enabled = false;
            _fadeSprite = true;
            GameEventManager.obj.PowerUpRoomsFloorBroken = true;
            Destroy(gameObject, 5);
        }
        if(_fadeSprite) {
            _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.b, _spriteRenderer.color.g, Mathf.MoveTowards(_spriteRenderer.color.a, 0, fadeMultiplier * Time.deltaTime));
        }
    }

    private IEnumerator ShakeWall() {
        yield return new WaitForSeconds(0.05f);
        SoundFXManager.obj.PlayBreakableWallCrackling(transform);
        shakeAnimation.Emit(numberOfShakeParticles);
        float downY = _originYPosition - shakeDistance;
        float upY = _originYPosition + shakeDistance;
        float[] positions = new float[2] {downY, upY};
        float time = 0f;
        int index = 1;
        while(time <= 1.0) {
            time += (Time.deltaTime / shakeTime) + shakeFrameWait;
            index += 1;
            _spriteRenderer.transform.position = new Vector2(_spriteRenderer.transform.position.x, positions[index % 2]);
            yield return new WaitForSeconds(shakeFrameWait);
        }
        _spriteRenderer.transform.position = new Vector2(_spriteRenderer.transform.position.x, _originYPosition);
    }
}
