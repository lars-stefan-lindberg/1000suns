using UnityEngine;

public class AvatarTargetTestTrigger : MonoBehaviour
{
    [SerializeField] private Transform _target;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            CaveAvatar.obj.SetTarget(_target);
        }
    }
}
