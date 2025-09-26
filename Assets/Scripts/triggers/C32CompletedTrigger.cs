using UnityEngine;
using UnityEngine.SceneManagement;

public class C32CompletedTrigger : MonoBehaviour
{
    [SerializeField] private Transform _targetPosition;
    [SerializeField] private C32Manager _c32Manager;
    private BoxCollider2D _collider;

    void Start()
    {
        if(LevelManager.obj.IsLevelCompleted("C32")) {
            Destroy(gameObject);
        }
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) {
            _c32Manager.Stop();
            _collider.enabled = false;
            CaveAvatar.obj.SetTarget(_targetPosition, 10);
            LevelManager.obj.SetLevelCompleted("C32");
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        }
    }
}
