using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float power;
    [SerializeField] private float movingSpeed = 10f;
    public int deadZone = 4;

    private Rigidbody2D _rigidBody;
    private CircleCollider2D _collider;
    private ParticleSystem _particles;
    private float _horizontalSpawnLocation;
    public bool isPoweredUp = false; //To break walls

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
        _particles = GetComponent<ParticleSystem>();
        _horizontalSpawnLocation = transform.position.x;
    }

    private void Update()
    {
        if(Mathf.Abs(transform.position.x - _horizontalSpawnLocation) > deadZone){
            StopAndDestroy();
        }
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
        if(collision.transform.CompareTag("Enemy") || 
            collision.transform.CompareTag("Block") || 
            collision.transform.CompareTag("Ground") || 
            collision.transform.CompareTag("Orb") || 
            collision.transform.CompareTag("FloatingPlatform") || 
            collision.transform.CompareTag("Rock") || 
            collision.transform.CompareTag("Roots") || 
            collision.transform.CompareTag("FallingPlatform") ||
            collision.transform.CompareTag("BreakableWall")) {
                StopAndDestroy();
            }
    }

    private void StopAndDestroy() {
        _rigidBody.velocity = Vector3.zero;
        _collider.enabled = false;
        _particles.Stop();
        Destroy(gameObject, 1);
    }
}
