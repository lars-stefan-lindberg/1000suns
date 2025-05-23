using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class C33Manager : MonoBehaviour
{
    [SerializeField] private Transform _blockSpawnStartPosition;
    [SerializeField] private Transform _blockSpawnEndPosition;
    [SerializeField] private GameObject _blockPrefab;
    [SerializeField] private GameObject _blockingWall;
    [SerializeField] private float _blockingWallFadeMultiplier = 1.5f;
    [SerializeField] private Transform _caveAvatarFleeTargetPosition;
    public float _attackInterval = 2f;
    public int _numberOfAttacks = 20;
    private int _attackCount = 0;
    private bool _startAttackSequence = false;
    private float _timer = 1.5f;
    private BoxCollider2D _collider;
    private LayerMask _blockLayerMask;
    private Tilemap _blockingWallTilemap;


    void Start() {
        _collider = GetComponent<BoxCollider2D>();
        if(LevelManager.obj.IsLevelCompleted("C33")) {
            _collider.enabled = false;
            _blockingWall.SetActive(true);
            _blockingWall.GetComponentInChildren<Tilemap>().color = new Color(1, 1, 1, 1);
            return;
        }
        _blockingWallTilemap = _blockingWall.GetComponentInChildren<Tilemap>();
        _blockLayerMask = LayerMask.GetMask("Block");
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Player")) {
            _collider.enabled = false;
            _startAttackSequence = true;
            _blockingWall.SetActive(true);
            SoundFXManager.obj.PlayBrokenFloorReappear(transform);
            StartCoroutine(FadeInBlockingWall());
        }
    }

    private IEnumerator FadeInBlockingWall() {
        while(_blockingWallTilemap.color.a < 1) {
            _blockingWallTilemap.color = new Color(_blockingWallTilemap.color.r, _blockingWallTilemap.color.g, _blockingWallTilemap.color.b, Mathf.MoveTowards(_blockingWallTilemap.color.a, 1, _blockingWallFadeMultiplier * Time.deltaTime));
            yield return null;
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
        float xSpawnPosition = PlayerManager.obj.GetPlayerTransform().position.x;
        if(xSpawnPosition > _blockSpawnEndPosition.position.x) {
            xSpawnPosition = _blockSpawnEndPosition.position.x;
        } else if(xSpawnPosition < _blockSpawnStartPosition.position.x) {
            xSpawnPosition = _blockSpawnStartPosition.position.x;
        }
        if(!IsBlockInTheWay(new Vector2(xSpawnPosition, _blockSpawnStartPosition.position.y))) {    
            CaveAvatar.obj.Attack();
            GameObject block = Instantiate(_blockPrefab, new Vector2(xSpawnPosition, _blockSpawnStartPosition.position.y), Quaternion.identity);
            block.GetComponent<Block>().SetGravity(1.5f);
            _attackCount++;
            if(_attackCount >= _numberOfAttacks) {
                CaveAvatar.obj.SetTarget(_caveAvatarFleeTargetPosition, 10);
                _startAttackSequence = false;
            }
        }
        yield return null;
    }

    private bool IsBlockInTheWay(Vector2 spawnPosition) {
        Vector2 halfExtents = new Vector2(0.5f, 0.5f);
        Collider2D hit = Physics2D.OverlapBox(spawnPosition, halfExtents, 0, _blockLayerMask);
        if (hit != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
