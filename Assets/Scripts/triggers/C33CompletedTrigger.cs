using UnityEngine;

public class C33CompletedTrigger : MonoBehaviour
{
    [SerializeField] private Transform _targetPosition;
    [SerializeField] private C33Manager _c33Manager;
    private BoxCollider2D _collider;

    void Start()
    {
        if(GameEventManager.obj.C33Completed) {
            Destroy(gameObject);
        }
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) {
            _c33Manager.Stop();
            _collider.enabled = false;
            CaveAvatar.obj.SetTarget(_targetPosition, 10);
            GameEventManager.obj.C33Completed = true;
        }
    }
}
