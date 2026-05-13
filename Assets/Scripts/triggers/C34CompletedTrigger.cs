using UnityEngine;
using UnityEngine.SceneManagement;

public class C34CompletedTrigger : MonoBehaviour
{
    [SerializeField] private Transform _targetPosition;
    [SerializeField] private AmbienceTrack _caveMainAmbience;
    [SerializeField] private SpawnPoint _spawnPoint;
    private BoxCollider2D _collider;

    void Start()
    {
        if(LevelManager.obj.IsLevelCompleted("Cave-55")) {
            Destroy(gameObject);
        }
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) {
            _collider.enabled = false;
            CaveAvatar.obj.SetTarget(_targetPosition, 10);
            LevelManager.obj.SetLevelCompleted("Cave-55");
            MusicManager.obj.EndCurrentTrack();
            AmbienceManager.obj.Play(_caveMainAmbience);
            GameManager.obj.SetCurrentSpawnPointId(_spawnPoint.SpawnPointID);
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        }
    }
}
