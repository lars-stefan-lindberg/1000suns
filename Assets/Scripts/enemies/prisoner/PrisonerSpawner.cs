using UnityEngine;

public class PrisonerSpawner : MonoBehaviour
{
    public GameObject prisoner;
    [SerializeField] private GameObject _leftPoint;
    [SerializeField] private GameObject _middlePoint;
    [SerializeField] private GameObject _rightPoint;
    [SerializeField] private float _spawnInterval = 1f;
    private float _spawnTime;

    private float _spawnTimer = 0f;
    
    private enum SpawnPosition { Left, Middle, Right }
    private SpawnPosition[] _spawnPositionOrder = { SpawnPosition.Left, SpawnPosition.Middle, SpawnPosition.Right, SpawnPosition.Middle };
    private int _spawnPositionIndex = 0;

    void Awake() {
        _spawnTime = _spawnInterval;
    }

    void Update() {
        if(_spawnTimer < _spawnTime) {
            _spawnTimer += Time.deltaTime;
        }
        else {
            SpawnPrisoner();
            _spawnTimer = 0f;
            _spawnTime = _spawnInterval;
        }
    }

    [ContextMenu("SpawnPrisoner")]
    public void SpawnPrisoner() {
        float randomHorizontalPosition = GetRandomHorizontalPosition();
        GameObject newPrisoner = Instantiate(prisoner, new Vector2(randomHorizontalPosition, _leftPoint.transform.position.y), transform.rotation);
        newPrisoner.transform.parent = transform;
        newPrisoner.transform.SetParent(null);
        newPrisoner.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        newPrisoner.GetComponent<Prisoner>().muteDeathSoundFX = true;
    }

    private float GetRandomHorizontalPosition()
    {
        SpawnPosition spawnPosition = _spawnPositionOrder[_spawnPositionIndex];
        _spawnPositionIndex = (_spawnPositionIndex + 1) % _spawnPositionOrder.Length;

        if(spawnPosition == SpawnPosition.Left)
            return _leftPoint.transform.position.x;
        else if(spawnPosition == SpawnPosition.Middle)
            return _middlePoint.transform.position.x;
        else
            return _rightPoint.transform.position.x;
    }
}
