using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float power;
    public int deadZone = 4;

    private ParticleSystem _pushEffect;
    public Rigidbody2D rigidBody;
    private float _horizontalSpawnLocation;

    void Awake()
    {
        _pushEffect = GetComponent<ParticleSystem>();
        rigidBody = GetComponent<Rigidbody2D>();
        _horizontalSpawnLocation = transform.position.x;
    }

    private void Update()
    {
        if(Mathf.Abs(transform.position.x - _horizontalSpawnLocation) > deadZone){
            Destroy(gameObject);
        }
    }

    public void shoot(int horizontalDirection, float power)
    {
        this.power = power;
        maybeRotateParticleEffect(horizontalDirection);
        _pushEffect.Emit(5);

        //horizontalDirection = -1 -> Left facing
        //horizontalDirection = 1 -> Right facing
        rigidBody.velocity = new Vector2(power * horizontalDirection, 0);
    }

    private void maybeRotateParticleEffect(int playerFacingDirection)
    {
        if (playerFacingDirection == 1 && _pushEffect.shape.rotation.x == 180) return;
        if (playerFacingDirection == -1 && _pushEffect.shape.rotation.x == 0) return;

        var shapeModule = _pushEffect.shape;
        shapeModule.rotation = new Vector3(playerFacingDirection == -1 ? 0f : 180f, _pushEffect.shape.rotation.y, _pushEffect.shape.rotation.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.CompareTag("Enemy") || collision.transform.CompareTag("Block") || collision.transform.CompareTag("Ground"))
            Destroy(gameObject);
    }
}
