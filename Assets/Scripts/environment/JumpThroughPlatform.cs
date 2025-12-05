using System.Collections;
using UnityEngine;

public class JumpThroughPlatform : MonoBehaviour
{
    private BoxCollider2D _collider;
    private Rigidbody2D _rigidBody;

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.transform.CompareTag("Player"))
        {
            PlayerMovement.obj.jumpThroughPlatform = this;
            PlayerMovement.obj.moveableRigidBody = _rigidBody;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            PlayerMovement.obj.jumpThroughPlatform = null;
            PlayerMovement.obj.moveableRigidBody = null;
        }
    }

    public void PassThrough() {
        _collider.enabled = false;
        StartCoroutine(EnableCollider());
    }

    private IEnumerator EnableCollider() {
        yield return new WaitForSeconds(0.5f);
        _collider.enabled = true;
    }
}
