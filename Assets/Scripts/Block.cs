using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody2D _rigidBody;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    public float forcePushPower = 20f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            _rigidBody.bodyType = RigidbodyType2D.Static;
        }
        else if (collision.transform.CompareTag("Projectile"))
        {
            bool hitFromTheLeft = collision.transform.position.x < _rigidBody.position.x;
            _rigidBody.velocity = new Vector2(hitFromTheLeft ? forcePushPower : -forcePushPower, 0);
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
