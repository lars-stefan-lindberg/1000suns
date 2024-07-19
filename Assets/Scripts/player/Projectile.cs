using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float power;
    [SerializeField] private float movingSpeed = 10f;
    public int deadZone = 4;

    public Rigidbody2D rigidBody;
    private float _horizontalSpawnLocation;
    public bool isPoweredUp = false; //To break walls

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        _horizontalSpawnLocation = transform.position.x;
    }

    private void Update()
    {
        if(Mathf.Abs(transform.position.x - _horizontalSpawnLocation) > deadZone){
            Destroy(gameObject);
        }
    }

    public void shoot(int horizontalDirection, float power, bool isPoweredUp)
    {
        this.power = power;
        this.isPoweredUp = isPoweredUp;

        //horizontalDirection = -1 -> Left facing
        //horizontalDirection = 1 -> Right facing
        rigidBody.velocity = new Vector2(movingSpeed * horizontalDirection, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.CompareTag("Enemy") || collision.transform.CompareTag("Block") || collision.transform.CompareTag("Ground")
            || collision.transform.CompareTag("Orb") || collision.transform.CompareTag("FloatingPlatform") || collision.transform.CompareTag("FallingPlatform"))
            Destroy(gameObject);
    }
}
