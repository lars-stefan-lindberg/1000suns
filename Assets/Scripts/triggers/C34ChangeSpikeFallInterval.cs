using UnityEngine;

public class C34ChangeSpikeFallInterval : MonoBehaviour
{
    [SerializeField] private C34Manager _c34Manager;
    [SerializeField] private float _newAttackInterval;

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            _c34Manager._attackInterval = _newAttackInterval;
        }
    }
}
