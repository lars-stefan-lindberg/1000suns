using UnityEngine;

public class C27SetFloorBrokenTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            GameEventManager.obj.C275FloorBroken = true;  
        }
    }
}
