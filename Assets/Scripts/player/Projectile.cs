using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    public float power;
    [SerializeField] private float movingSpeed = 10f;
    public int deadZone = 4;

    private Rigidbody2D _rigidBody;
    private CircleCollider2D _collider;
    private ParticleSystem _particles;
    private float _horizontalSpawnLocation;
    public bool isPoweredUp = false;
    
    private static readonly HashSet<string> _collisionTags = new HashSet<string>
    {
        "Enemy", "Block", "Ground", "FloatingPlatform", 
        "Rock", "Roots", "Grass", "Wood", "TreeBranch", "BreakableWall"
    };

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
        _particles = GetComponent<ParticleSystem>();
        _horizontalSpawnLocation = transform.position.x;
        
        float maxTravelTime = deadZone / movingSpeed;
        Invoke(nameof(StopAndDestroy), maxTravelTime);
    }

    public void Shoot(int horizontalDirection, float power, bool isPoweredUp)
    {
        this.power = power;
        this.isPoweredUp = isPoweredUp;

        //horizontalDirection = -1 -> Left facing
        //horizontalDirection = 1 -> Right facing
        _rigidBody.velocity = new Vector2(movingSpeed * horizontalDirection, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Transform collisionTransform = collision.transform;
        
        if(_collisionTags.Contains(collisionTransform.tag)) {
            StopAndDestroy();
        }
        else if(collisionTransform.CompareTag("Player")) {
            if(PlayerManager.obj.GetPlayerTypeFromCollider(collision) != PlayerManager.PlayerType.HUMAN) {
                StopAndDestroy();
            }
        }
    }

    private void StopAndDestroy() {
        _rigidBody.velocity = Vector3.zero;
        _collider.enabled = false;
        _particles.Stop();
        Destroy(gameObject, 1);
    }
}
