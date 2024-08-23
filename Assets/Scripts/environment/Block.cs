using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody2D _rigidBody;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    public float basePushPower = 7f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            _rigidBody.bodyType = RigidbodyType2D.Static;
        }
        else if (collision.transform.CompareTag("Projectile"))
        {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            bool hitFromTheLeft = Player.obj.transform.position.x < _rigidBody.position.x;
            float power = basePushPower * projectile.power;
            _rigidBody.velocity = new Vector2(hitFromTheLeft ? power : -power, 0);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public float deceleration = 1f;
    private void Update()
    {
        if (_rigidBody.velocity.x != 0)
        {
            _rigidBody.velocity = new Vector2(Mathf.MoveTowards(_rigidBody.velocity.x, 0, deceleration * Time.deltaTime), _rigidBody.velocity.y);
        }
    }
}
