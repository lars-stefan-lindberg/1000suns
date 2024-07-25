using UnityEngine;

public class ChargeAnimationMgr : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator.enabled = false;
        _spriteRenderer.enabled = false;
    }

    public void Charge() {
        _spriteRenderer.enabled = true;
        _animator.enabled = true;
    }

    public void FullyCharged() {
        _animator.SetBool("fullyCharged", true);
    }

    public void Cancel() {
        _animator.SetBool("fullyCharged", false);
        _animator.enabled = false;
        _spriteRenderer.enabled = false;
    }

    void Update()
    {
        transform.position = new Vector2(Player.obj.transform.position.x, Player.obj.transform.position.y);
    }
}
