using System.Collections;
using UnityEngine;

public class BlackHole : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator; 
    private LightSprite2DFlicker _lightSprite2DFlicker;

    [SerializeField] private LightSprite2DFadeManager _lightSprite2DFadeManager;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _particleSystem = GetComponent<ParticleSystem>();
        _lightSprite2DFlicker = GetComponent<LightSprite2DFlicker>();
    }

    [ContextMenu("Despawn")]
    public void Despawn() {
        _animator.SetTrigger("despawn");
    }

    public void FadeInLight() {
        _lightSprite2DFadeManager.StartFadeIn();
    }

    public void EnableFlicker() {
        _lightSprite2DFlicker.enabled = true;
    }

    public void Destroy() {
        StartCoroutine(DelayedDestroy());
    }

    private IEnumerator DelayedDestroy() {
        _lightSprite2DFlicker.enabled = false;
        _lightSprite2DFadeManager.StartFadeOut();
        _particleSystem.Stop();
        _spriteRenderer.enabled = false;
        Destroy(this, 5);
        yield return null;
    }

}
