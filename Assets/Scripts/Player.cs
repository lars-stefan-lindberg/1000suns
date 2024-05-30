using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player obj;

    [Header("Dependecies")]
    public Rigidbody2D rigidBody;

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
