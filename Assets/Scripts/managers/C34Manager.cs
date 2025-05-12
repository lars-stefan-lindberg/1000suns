using System.Collections;
using UnityEngine;

public class C34Manager : MonoBehaviour
{
    [SerializeField] private Transform _spikeSpawnStartPosition;
    [SerializeField] private Transform _spikeSpawnEndPosition;
    [SerializeField] private GameObject _spikePrefab;
    public float _attackInterval = 2f;
    private bool _startAttackSequence = false;
    private float _timer = 1.5f;
    private float _spikeDistance = 1f;
    private int _numberOfAttacks = 0;
    private BoxCollider2D _collider;

    void Start() {
        if(LevelManager.obj.IsLevelCompleted("C34")) {
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
        float spikeXPosition = _spikeSpawnStartPosition.position.x + _spikeDistance * _numberOfAttacks;
        GameObject spikeGameObject = Instantiate(_spikePrefab, new Vector2(spikeXPosition, _spikeSpawnStartPosition.position.y), Quaternion.identity);
        Spike spike = spikeGameObject.GetComponent<Spike>();
        spike.isRespawnable = false;
        spike.InitiateFall();
        _numberOfAttacks++;
        if(spikeXPosition >= _spikeSpawnEndPosition.position.x) {
            _startAttackSequence = false;
        }
        yield return null;
    }
}
