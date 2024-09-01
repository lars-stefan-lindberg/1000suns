using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private BoxCollider2D _collider;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
    }

    public float basePushPower = 7f;
    private bool isPlayerBeneath = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            _rigidBody.bodyType = RigidbodyType2D.Static;
            if(HitUnderneath(collision)) {
                if(PlayerMovement.obj.isGrounded)
                    Reaper.obj.KillPlayerGeneric();
                else
                    isPlayerBeneath = true;
            }
        }
        else if (collision.transform.CompareTag("Projectile"))
        {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            bool hitFromTheLeft = Player.obj.transform.position.x < _rigidBody.position.x;
            float power = basePushPower * projectile.power;
            _rigidBody.velocity = new Vector2(hitFromTheLeft ? power : -power, 0);
        } else if(collision.transform.CompareTag("Enemy")) {
            if(HitUnderneath(collision)) {
                Prisoner prisoner = collision.GetComponent<Prisoner>();
                if(prisoner.isGrounded) {
                    _rigidBody.bodyType = RigidbodyType2D.Static;
                    Reaper.obj.KillPrisoner(prisoner);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player") || collision.transform.CompareTag("Enemy"))
        {
            _rigidBody.bodyType = RigidbodyType2D.Dynamic;
            isPlayerBeneath = false;
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

    private void FixedUpdate() {
        if(isPlayerBeneath) {
            if(PlayerMovement.obj.isGrounded)
                Reaper.obj.KillPlayerGeneric();
        }
    }

    private bool HitUnderneath(Collider2D collider) {
        //For reference:
        //Vector2 topRight = new Vector2(boxBounds.center.x + boxBounds.extents.x, boxBounds.center.y + boxBounds.extents.y);
        Bounds collisionBounds = collider.bounds;
        Vector2 top = new(collisionBounds.center.x, collisionBounds.center.y + collisionBounds.extents.y);

        Bounds blockBounds = _collider.bounds;
        Vector2 bottom = new(blockBounds.center.x, blockBounds.center.y - blockBounds.extents.y);
        return bottom.y > top.y;
    }
}
