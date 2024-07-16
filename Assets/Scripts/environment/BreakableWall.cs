using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    public GameObject visibleLayer;
    private Animator _visibleLayerAnimator;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    public ParticleSystem breakAnimation;
    public ParticleSystem shakeAnimation;

    public bool breakWall = false;
    public bool shakeWall = false;
    public float fadeMultiplier = 0.1f;
    private bool _fadeSprite = false;
    public float shakeDistance = 0.1f;
    public float shakeTime = 0.12f;
    public float shakeFrameWait = 0.08f;
    private float _originXPosition;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originXPosition = _spriteRenderer.transform.position.x;
        _collider = GetComponent<BoxCollider2D>();
        _visibleLayerAnimator = visibleLayer.GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        if(collider.transform.CompareTag("Projectile")){
            var projectile = collider.gameObject.GetComponent<Projectile>();
            if(projectile.isPoweredUp)
                breakWall = true;
            else
                shakeWall = true;
        }
    }
    void FixedUpdate()
    {
        if(shakeWall) {
            shakeAnimation.Emit(10);
            shakeWall = false;
            StartCoroutine(ShakeWall());
        }
        if(breakWall) {
            _visibleLayerAnimator.SetTrigger("reveal");
            _collider.enabled = false;
            _fadeSprite = true;
            breakWall = false;
            breakAnimation.Emit(6);
        }
        if(_fadeSprite) {
            _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.b, _spriteRenderer.color.g, Mathf.MoveTowards(_spriteRenderer.color.a, 0, fadeMultiplier * Time.deltaTime));
        }
    }

    private IEnumerator ShakeWall() {
        float leftX = _originXPosition - shakeDistance;
        float rightX = _originXPosition + shakeDistance;
        float[] positions = new float[2] {leftX, rightX};
        float time = 0f;
        int index = 1;
        while(time <= 1.0) {
            time += (Time.deltaTime / shakeTime) + shakeFrameWait;
            index += 1;
            _spriteRenderer.transform.position = new Vector2(positions[index % 2], _spriteRenderer.transform.position.y);
            yield return new WaitForSeconds(shakeFrameWait);
        }
        _spriteRenderer.transform.position = new Vector2(_originXPosition, _spriteRenderer.transform.position.y);
    }
}
