using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player obj;

    [Header("Dependencies")]
    public Rigidbody2D rigidBody;
    public Collider2D playerCollider;
    public bool hasPowerUp = false;

    void Awake()
    {
        obj = this;
    }

    private void Update()
    {
        if (transform.position.y < GameMgr.DEAD_ZONE)
        {
            Debug.Log("Player died.");
            //Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        obj = null; 
    }
}
