using System.Collections;
using FunkyCode.Rendering.Lightmap;
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

    //In case we want to reuse the same blackhole multiple times
    //This can happen if you clear a room, leave the creatures, but then pick up
    //another one in the same room.
    void OnEnable() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _particleSystem = GetComponent<ParticleSystem>();
        _lightSprite2DFlicker = GetComponent<LightSprite2DFlicker>();
        _spriteRenderer.enabled = true;
        _lightSprite2DFlicker.enabled = true;
        if(!_particleSystem.isPlaying)
            _particleSystem.Play();
    }

    [ContextMenu("Despawn")]
    public void Despawn() {
        _animator.SetTrigger("despawn");
    }

    public void FadeInLight() {
        _lightSprite2DFadeManager.StartFadeIn();
    }

    public void Destroy() {
        StartCoroutine(DelayedDestroy());
    }

    private IEnumerator DelayedDestroy() {
        _lightSprite2DFlicker.enabled = false;
        _lightSprite2DFadeManager.StartFadeOut();
        _particleSystem.Stop();
        _spriteRenderer.enabled = false;
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }

}
