using UnityEngine;

public class PrisonerSpawner : MonoBehaviour
{
    public GameObject prisoner;
    [SerializeField] private GameObject _leftPoint;
    [SerializeField] private GameObject _rightPoint;
    [SerializeField] private float _startingSpawnTime = 1f;
    private float _spawnTime;

    private float _spawnTimer = 0f;

    void Awake() {
        _spawnTime = _startingSpawnTime;
    }

    void Update() {
        if(_spawnTimer < _spawnTime) {
            _spawnTimer += Time.deltaTime;
        }
        else {
            SpawnPrisoner();
            _spawnTimer = 0f;
            _spawnTime = Random.Range(_startingSpawnTime - 0.2f, _startingSpawnTime + 0.2f);
        }
    }

    [ContextMenu("SpawnPrisoner")]
    public void SpawnPrisoner() {
        float randomHorizontalPosition = GetRandomHorizontalPosition();
        Instantiate(prisoner, new Vector2(randomHorizontalPosition, _leftPoint.transform.position.y), transform.rotation);
    }

    private float GetRandomHorizontalPosition()
    {
        return Random.Range(_leftPoint.transform.position.x, _rightPoint.transform.position.x);
    }
}
