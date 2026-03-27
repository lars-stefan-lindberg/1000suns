using UnityEngine;

public class ForestGrass : MonoBehaviour
{
    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();    
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) {
            _animator.SetTrigger("wiggle");            
        }
    }
}
