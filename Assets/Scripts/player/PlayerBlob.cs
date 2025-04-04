using UnityEngine;

public class PlayerBlob : MonoBehaviour
{
    public static PlayerBlob obj;
    public Rigidbody2D rigidBody;
    private BoxCollider2D _collider;
    private LayerMask _groundLayerMasks;
    public Surface surface = Surface.Rock;

    void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
        _groundLayerMasks = LayerMask.GetMask("Ground");
    }

    void OnCollisionEnter2D(Collision2D other) {
        if((_groundLayerMasks.value & (1 << other.gameObject.layer)) != 0) {
            string surfaceTag = other.gameObject.tag;
            if(surfaceTag == "Rock")
                surface = Surface.Rock;
            else if(surfaceTag == "Roots")
                surface = Surface.Roots;
        }
    }

    void OnDestroy()
    {
        obj = null; 
    }
}
