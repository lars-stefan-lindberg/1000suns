using System.Collections;
using UnityEngine;

public class ChargeAnimationMgr : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    
    [SerializeField] private float _animationFadeMultiplier = 7f;

    private bool _isStarted = false;
    private float _playerOffset = 0.2f;
    private Color _fadeStartColor;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator.enabled = false;
        _spriteRenderer.enabled = false;
        _fadeStartColor = new Color(_spriteRenderer.color.r, _spriteRenderer.color.g, _spriteRenderer.color.b, 1f);
    }

    public void Charge() {
        _fadeStartColor.a = 1f;
        _spriteRenderer.color = _fadeStartColor;
        _spriteRenderer.enabled = true;
        _animator.enabled = true;
        if(!_isStarted) {
            _isStarted = true;
            _animator.Play("charge_start");
        }
    }

    public void FullyCharged() {
        _animator.SetBool("fullyCharged", true);
    }

    public void FullyChargedPoweredUp() {
        _animator.SetBool("fullyCharged", true);
        _animator.SetBool("poweredUp", true);
    }

    public void Cancel() {
        _isStarted = false;
        StartCoroutine(SpriteFadeOutAndCancel(_animationFadeMultiplier));
    }

    public void HardCancel() {
        _animator.SetBool("fullyCharged", false);
        _animator.SetBool("poweredUp", false);
        _animator.enabled = false;
        _fadeStartColor.a = 0;
        _spriteRenderer.color = _fadeStartColor;
        _spriteRenderer.enabled = false;
    }

    private IEnumerator SpriteFadeOutAndCancel(float fadeMultiplier) {
        while(_spriteRenderer.color.a > 0) {
            _fadeStartColor.a -= Time.deltaTime * fadeMultiplier;
            _spriteRenderer.color = _fadeStartColor;
            yield return null;
        }

        _animator.SetBool("fullyCharged", false);
        _animator.SetBool("poweredUp", false);
        _animator.enabled = false;
        _spriteRenderer.enabled = false;
    }

    void Update()
    {
        transform.position = new Vector2(Player.obj.transform.position.x, Player.obj.transform.position.y + _playerOffset);
    }
}
