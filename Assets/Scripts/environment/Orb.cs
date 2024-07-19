using Unity.VisualScripting;
using UnityEngine;

public class Orb : MonoBehaviour
{
    private BoxCollider2D _collider;
    private Rigidbody2D _rigidBody;

    public float idleMoveSpeed;
    private float _idleVerticalTargetPosition;
    private float _idleTargetVerticalPosition = 0;
    public float startingVerticalPosition;
    public float idleVerticalDistance = 0.25f;
    public float pushPower = 10f;
    public float deceleration = 20f;
    public bool moveHorizontally = false;

    void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        startingVerticalPosition = transform.position.y;
        _idleVerticalTargetPosition = startingVerticalPosition - idleVerticalDistance;
    }

    void Update()
    {
        if(moveHorizontally)
            _rigidBody.velocity = new Vector2(Mathf.MoveTowards(_rigidBody.velocity.x, 0, deceleration * Time.deltaTime), _rigidBody.velocity.y);
        else
            _rigidBody.velocity = new Vector2(0, 0);
        if(_rigidBody.velocity.x == 0)
        {
            moveHorizontally = false;
            MoveVertical();
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.transform.CompareTag("Projectile"))
        {
            Projectile projectile = other.gameObject.GetComponent<Projectile>();
            bool hitFromTheLeft = projectile.rigidBody.position.x < _rigidBody.position.x;
            MoveHorizontal(hitFromTheLeft);
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if(other.transform.CompareTag("Enemy")) {
            Prisoner prisoner = other.gameObject.GetComponent<Prisoner>();
            Reaper.obj.KillPrisoner(prisoner);
        } else
            moveHorizontally = false;
    }

    private void MoveVertical()
    {
        if (transform.position.y >= startingVerticalPosition)
            _idleTargetVerticalPosition = _idleVerticalTargetPosition;
        if (transform.position.y <= _idleVerticalTargetPosition)
            _idleTargetVerticalPosition = startingVerticalPosition;
        transform.position = new Vector2(transform.position.x, Mathf.MoveTowards(transform.position.y, _idleTargetVerticalPosition, idleMoveSpeed * Time.deltaTime));
    }

    private void MoveHorizontal(bool moveRight)
    {
        moveHorizontally = true;
        _rigidBody.velocity = new Vector2(moveRight ? pushPower : -pushPower, 0);
    }
}
