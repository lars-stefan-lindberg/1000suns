using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{

    private Rigidbody2D _rigidBody;
    private SpriteRenderer _spriteRenderer;

    public bool isPlayerOnPlatform = false;
    public float timeBeforeFall = 1f;
    public float spawnTimer = 0f;
    public float timeFallingBeforeDestroy = 3f;
    public float spawnTime = 1f;
    public float fallTimer = 0f;
    private bool _startFallCountDown = false;
    private bool _respawning = false;
    private bool _falling = false;
    private Vector2 _startingPosition;

    private Color _fadeOutStartColor;
    [Range(0.1f, 10f), SerializeField] private float _fadeSpeed = 5f;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _startingPosition = transform.position;
        _fadeOutStartColor = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.CompareTag("Player"))
        {
            isPlayerOnPlatform = true;
            _startFallCountDown = true;
            PlayerMovement.obj.platformRigidBody = _rigidBody;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            isPlayerOnPlatform = false;
            PlayerMovement.obj.platformRigidBody = null;
        }
    }

    private void Update()
    {
        if(_startFallCountDown)
            fallTimer += Time.deltaTime;
        if(!_falling && !_respawning && fallTimer >= timeBeforeFall) {
            _falling = true;
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
            _rigidBody.gravityScale = 1;
        }
        if(_falling && fallTimer >= timeFallingBeforeDestroy) {
            StartRespawning();
        }
        if(_respawning) {
            spawnTimer += Time.deltaTime;
            if(spawnTimer >= spawnTime) {
                _respawning = false;
                transform.position = _startingPosition;
                StartCoroutine(FadeInSprite());
            }
        }
    }

    private void StartRespawning() {
        _respawning = true;
        _falling = false;
        fallTimer = 0;
        _startFallCountDown = false;
        spawnTimer = 0;
        _rigidBody.velocity = new Vector3(0,0,0);
        _rigidBody.gravityScale = 0;
        _rigidBody.bodyType = RigidbodyType2D.Kinematic;
        _fadeOutStartColor.a = 0;
        _spriteRenderer.color = _fadeOutStartColor;
    }

    private IEnumerator FadeInSprite() {
        while(_spriteRenderer.color.a < 1f) {
            _fadeOutStartColor.a += Time.deltaTime * _fadeSpeed;
            _spriteRenderer.color = _fadeOutStartColor;
            yield return null;
        }
    }
}
