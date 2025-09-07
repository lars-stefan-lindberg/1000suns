using System.Collections;
using UnityEngine;

public class C32Manager : MonoBehaviour
{
    [SerializeField] private Transform _spikeSpawnStartPosition;
    [SerializeField] private GameObject _spikePrefab;
    [SerializeField] private Transform _maxHorizontalSpikeSpawn;
    [SerializeField] private Transform _minHorizontalSpikeSpawn;
    public float _attackInterval = 2f;
    private bool _startAttackSequence = false;
    private float _timer = 1.5f;
    private BoxCollider2D _collider;
    private readonly float _spikeDistance = 1f;
    private readonly float _spikesOffset = 3f;

    void Start() {
        if(LevelManager.obj.IsLevelCompleted("C32")) {
            Destroy(gameObject);
            return;
        }
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            _startAttackSequence = true;
            _collider.enabled = false;
        }
    }

    public void Stop()
    {
        _startAttackSequence = false;
    }

    [ContextMenu("Start Attack Sequence")]
    public void StartAttackSequence() {
        _startAttackSequence = true;
    }

    void FixedUpdate()
    {
        if (_startAttackSequence) {
            _timer += Time.fixedDeltaTime;
            if (_timer > _attackInterval) {
                _timer = 0;
                StartCoroutine(FireAttack());
            }
        }
    }

    private IEnumerator FireAttack() {
        CaveAvatar.obj.Attack();

        yield return new WaitForSeconds(0.5f);
        
        float firstSpikeXPosition = PlayerManager.obj.GetPlayerTransform().position.x + _spikesOffset;
        if(firstSpikeXPosition > _maxHorizontalSpikeSpawn.position.x) {
            firstSpikeXPosition = _maxHorizontalSpikeSpawn.position.x;
        } else if(firstSpikeXPosition < _minHorizontalSpikeSpawn.position.x) {
            firstSpikeXPosition = _minHorizontalSpikeSpawn.position.x;
        }

        float secondSpikeXPosition = firstSpikeXPosition + _spikeDistance;
        float thirdSpikeXPosition = secondSpikeXPosition + _spikeDistance;
        float fourthSpikeXPosition = firstSpikeXPosition - _spikeDistance;
        float fifthSpikeXPosition = fourthSpikeXPosition - _spikeDistance;
        float sixthSpikeXPosition = thirdSpikeXPosition + _spikeDistance;
        float seventhSpikeXPosition = fifthSpikeXPosition - _spikeDistance;

        SpawnSpike(firstSpikeXPosition, _spikeSpawnStartPosition.position.y);
        SpawnSpike(secondSpikeXPosition, _spikeSpawnStartPosition.position.y);
        SpawnSpike(thirdSpikeXPosition, _spikeSpawnStartPosition.position.y);
        SpawnSpike(fourthSpikeXPosition, _spikeSpawnStartPosition.position.y);
        SpawnSpike(fifthSpikeXPosition, _spikeSpawnStartPosition.position.y);
        SpawnSpike(sixthSpikeXPosition, _spikeSpawnStartPosition.position.y);
        SpawnSpike(seventhSpikeXPosition, _spikeSpawnStartPosition.position.y);

        yield return null;
    }

    private void SpawnSpike(float x, float y) {
        GameObject spikeGameObject = Instantiate(_spikePrefab, new Vector2(x, y), Quaternion.identity);
        Spike spike = spikeGameObject.GetComponent<Spike>();
        spike.isRespawnable = false;
        spike.InitiateFall();
    }
}
