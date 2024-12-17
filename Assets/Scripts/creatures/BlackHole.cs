using UnityEngine;

public class BlackHole : MonoBehaviour
{
    private Animator _animator;
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    [ContextMenu("Despawn")]
    public void Despawn() {
        _animator.SetTrigger("despawn");
    }

    public void Destroy() {
        gameObject.SetActive(false);
        Destroy(gameObject, 2);
    }
}
