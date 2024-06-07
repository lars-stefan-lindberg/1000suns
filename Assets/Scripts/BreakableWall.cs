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

    public bool breakWall = false;
    public bool shakeWall = false;
    public float fadeMultiplier = 0.1f;
    private bool _fadeSprite = false;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
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
            Debug.Log("Shake wall");
            shakeWall = false;
        }
        if(breakWall) {
            _visibleLayerAnimator.SetTrigger("reveal");
            _collider.enabled = false;
            _fadeSprite = true;
            breakWall = false;
        }
        if(_fadeSprite) {
            _spriteRenderer.color = new Color(_spriteRenderer.color.r, _spriteRenderer.color.b, _spriteRenderer.color.g, Mathf.MoveTowards(_spriteRenderer.color.a, 0, fadeMultiplier * Time.deltaTime));
        }
    }
}
