using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Spike : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private BoxCollider2D _collider;
    private bool _isFalling = false;
    public float castDistance = 0;
    public float gravity = 0;

    private void Awake() {
        _rigidBody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
    }

    private void Update() {
        Debug.DrawRay(transform.position, Vector3.down * castDistance, Color.red);
        if (!_isFalling) {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.down, castDistance);

            if(hit.transform != null) {
                if(hit.transform.CompareTag("Player")) {
                    _isFalling = true;
                    _rigidBody.gravityScale = gravity;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(collision.transform.CompareTag("Player")) {
            Reaper.obj.KillPlayerGeneric();
        } else
        {
            _rigidBody.gravityScale = 0;
            _collider.enabled = false;
        }

    }
}
