using UnityEngine;
using UnityEngine.SceneManagement;

public class C33CompletedTrigger : MonoBehaviour
{
    [SerializeField] private Transform _targetPosition;
    [SerializeField] private C33Manager _c33Manager;
    [SerializeField] private SpawnPoint _spawnPoint;
    private BoxCollider2D _collider;

    void Start()
    {
        if(LevelManager.obj.IsLevelCompleted("Cave-54")) {
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
            LevelManager.obj.SetLevelCompleted("Cave-54");
            GameManager.obj.SetCurrentSpawnPointId(_spawnPoint.SpawnPointID);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        }
    }
}
