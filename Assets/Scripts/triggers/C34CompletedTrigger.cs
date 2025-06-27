using UnityEngine;

public class C34CompletedTrigger : MonoBehaviour
{
    [SerializeField] private Transform _targetPosition;
    private BoxCollider2D _collider;

    void Start()
    {
        if(LevelManager.obj.IsLevelCompleted("C34")) {
            Destroy(gameObject);
        }
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) {
            _collider.enabled = false;
            CaveAvatar.obj.SetTarget(_targetPosition, 10);
            LevelManager.obj.SetLevelCompleted("C34");
            MusicManager.obj.ScheduleClipOnNextBar(MusicManager.obj.caveAvatarChaseOutro, 210, false);
        }
    }
}
