using UnityEngine;

public class C31CompletedTrigger : MonoBehaviour
{
    [SerializeField] private Transform _targetPosition;
    [SerializeField] private C31Manager _c31Manager;
    private BoxCollider2D _collider;

    void Start()
    {
        if(GameEventManager.obj.C31Completed) {
            Destroy(gameObject);
        }
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) {
            _c31Manager.Stop();
            _collider.enabled = false;
            CaveAvatar.obj.SetTarget(_targetPosition, 10);
            GameEventManager.obj.C31Completed = true;
        }
    }
}
