using UnityEngine;

public class Orb : MonoBehaviour
{
    private BoxCollider2D _collider;
    private Rigidbody2D _rigidBody;

    [SerializeField] private float idleMoveSpeed;
    [SerializeField] private float idleVerticalDistance = 0.25f;
    [SerializeField] private float basePushPower = 5f;
    [SerializeField] private float deceleration = 20f;
    private float _idleVerticalTargetPosition;
    private float _idleTargetVerticalPosition = 0;
    private float _startingVerticalPosition;
    private bool _moveHorizontally = false;

    void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _startingVerticalPosition = transform.position.y;
        _idleVerticalTargetPosition = _startingVerticalPosition - idleVerticalDistance;
    }

    void Update()
    {
        if(_moveHorizontally)
            _rigidBody.velocity = new Vector2(Mathf.MoveTowards(_rigidBody.velocity.x, 0, deceleration * Time.deltaTime), _rigidBody.velocity.y);
        else
            _rigidBody.velocity = new Vector2(0, 0);
        if(_rigidBody.velocity.x == 0)
        {
            _moveHorizontally = false;
            MoveVertical();
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.transform.CompareTag("Projectile"))
        {
            Projectile projectile = other.gameObject.GetComponent<Projectile>();
            bool hitFromTheLeft = other.bounds.center.x < _rigidBody.position.x;
            MoveHorizontal(projectile.power, hitFromTheLeft);
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if(other.transform.CompareTag("Enemy")) {
            Prisoner prisoner = other.gameObject.GetComponent<Prisoner>();
            Reaper.obj.KillPrisoner(prisoner);
        } else
            _moveHorizontally = false;
    }

    private void MoveVertical()
    {
        if (transform.position.y >= _startingVerticalPosition)
            _idleTargetVerticalPosition = _idleVerticalTargetPosition;
        if (transform.position.y <= _idleVerticalTargetPosition)
            _idleTargetVerticalPosition = _startingVerticalPosition;
        transform.position = new Vector2(transform.position.x, Mathf.MoveTowards(transform.position.y, _idleTargetVerticalPosition, idleMoveSpeed * Time.deltaTime));
    }

    private void MoveHorizontal(float force, bool moveRight)
    {
        _moveHorizontally = true;
        float power = basePushPower * force;
        _rigidBody.velocity = new Vector2(moveRight ? power : -power, 0);
    }
}
