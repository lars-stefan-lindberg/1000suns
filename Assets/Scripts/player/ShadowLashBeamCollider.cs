using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ShadowLashBeamCollider : MonoBehaviour
{
    public event Action OnSurfaceHit;

    private Rigidbody2D _rigidbody2D;
    private bool _hasHit;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D.isKinematic = true;
        _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hasHit) return;
        
        _hasHit = true;
        OnSurfaceHit?.Invoke();
    }
}
