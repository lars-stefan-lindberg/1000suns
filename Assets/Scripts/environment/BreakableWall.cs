using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    public bool isBig = false;
    public GameObject visibleLayer;
    public GameObject background;
    private Animator _visibleLayerAnimator;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    public ParticleSystem breakAnimation;
    public ParticleSystem shakeAnimation;

    public bool unbreakable = false;
    public GameObject otherBreakableWall;
    public bool breakWall = false;
    public bool shakeWall = false;
    public bool hintWall = false;
    public float hintWallCooldownTime = 1f;
    private float _hintWallCooldownTimer = 0;
    public float fadeMultiplier = 0.1f;
    public int numberOfShakeParticles = 10;
    private bool _fadeSprite = false;
    public float shakeDistance = 0.1f;
    public float shakeTime = 0.12f;
    public float shakeFrameWait = 0.08f;
    private float _originXPosition;
    public bool isSecret = true;
    public bool hasHint = false;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originXPosition = _spriteRenderer.transform.position.x;
        _collider = GetComponent<BoxCollider2D>();
        _visibleLayerAnimator = visibleLayer.GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player")) {
            // When the player touches the wall we only play the shake animation to hint it's breakable.
            // Guard with a cooldown to avoid spamming.
            if(hasHint && Time.time >= _hintWallCooldownTimer) {
                hintWall = true;
                _hintWallCooldownTimer = Time.time + hintWallCooldownTime;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        if(collider.transform.CompareTag("Projectile")){
            var projectile = collider.gameObject.GetComponent<Projectile>();
            if(!isBig && projectile.power >= PlayerPush.obj.maxForce && !unbreakable) {
                breakWall = true;
            } else if(isBig && projectile.isPoweredUp && projectile.power >= PlayerPush.obj.maxForce && !unbreakable) {
                breakWall = true;
            }
            else
                shakeWall = true;
        }
    }
    void FixedUpdate()
    {
        if(hintWall) {
            hintWall = false;
            SoundFXManager.obj.PlayBreakableWallHint(transform);
            shakeAnimation.Emit(numberOfShakeParticles);
        }
        if(shakeWall) {
            SoundFXManager.obj.PlayBreakableWallCrackling(transform);
            shakeAnimation.Emit(numberOfShakeParticles);
            shakeWall = false;
            StartCoroutine(ShakeWall());
        }
        if(breakWall) {
            if(background != null)
                background.SetActive(false);
            SoundFXManager.obj.PlayBreakableWallBreak(transform);
            if(isSecret)
                SoundFXManager.obj.PlayRevealSecret(transform);
            _visibleLayerAnimator.SetTrigger("reveal");
            //Fade out other breakable wall, if there is one
            if(otherBreakableWall != null) {
                StartCoroutine(FadeOutOtherBreakableWall());
            }
            _collider.enabled = false;
            _fadeSprite = true;
            breakWall = false;
            breakAnimation.Emit(6);
        }
        if(_fadeSprite) {
            _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.b, _spriteRenderer.color.g, Mathf.MoveTowards(_spriteRenderer.color.a, 0, fadeMultiplier * Time.deltaTime));
        }
    }

    private IEnumerator FadeOutOtherBreakableWall() {
        SpriteRenderer otherBreakableWallSpriteRenderer = otherBreakableWall.GetComponent<SpriteRenderer>();
        Color tempColor = otherBreakableWallSpriteRenderer.color;
        while(tempColor.a > 0) {
            tempColor.a = Mathf.MoveTowards(tempColor.a, 0, fadeMultiplier * Time.deltaTime);
            otherBreakableWallSpriteRenderer.color = tempColor;
            yield return null;
        }
        otherBreakableWall.SetActive(false);
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
