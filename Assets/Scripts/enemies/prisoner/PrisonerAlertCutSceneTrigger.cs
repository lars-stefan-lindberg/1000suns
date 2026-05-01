using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PrisonerAlertCutSceneTrigger : MonoBehaviour
{
    [SerializeField] private MusicTrack _musicTrack;
    [SerializeField] private GameEventId _firstPrisonerFightEnded;
    [SerializeField] private GameEventId _firstPrisonerFightStarted;
    [SerializeField] private SpawnPoint _spawnPoint;
    [SerializeField] private GameObject _blockingWall;
    [SerializeField] private Tilemap _blockingWallTilemap;
    public BabyPrisoner babyPrisoner;
    public Prisoner prisoner;
    public GameObject lockedDoor;
    public float cutSceneDuration = 4.5f;
    void OnTriggerEnter2D(Collider2D other) {
        if(GameManager.obj.HasEvent(_firstPrisonerFightEnded)) return;
        if(other.gameObject.CompareTag("Player")) {
            if(GameManager.obj.HasEvent(_firstPrisonerFightStarted)) {
                prisoner.gameObject.SetActive(true);
                lockedDoor.SetActive(true);
                gameObject.SetActive(false);
                _blockingWall.SetActive(true);
                _blockingWallTilemap.color = new Color(_blockingWallTilemap.color.r, _blockingWallTilemap.color.g, _blockingWallTilemap.color.b, 1f);
            } else {
                PlayerMovement.obj.Freeze();
                babyPrisoner.PlayScaredSfx();
                lockedDoor.SetActive(true);
                _blockingWall.SetActive(true);
                StartCoroutine(PrisonerSpawn());
            }
        }
    }

    private IEnumerator PrisonerSpawn() {
        Color startColor = _blockingWallTilemap.color;
        startColor.a = 0f;
        _blockingWallTilemap.color = startColor;
        Color targetColor = startColor;
        targetColor.a = 1f;
        DOTween.To(() => _blockingWallTilemap.color, x => _blockingWallTilemap.color = x, targetColor, 1f);

        prisoner.isStatic = true;
        prisoner.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        MusicManager.obj.Play(_musicTrack);
        babyPrisoner.Despawn();

        yield return new WaitForSeconds(2.3f);

        prisoner.isStatic = false;
        gameObject.SetActive(false);
        PlayerMovement.obj.UnFreeze();
        GameManager.obj.RegisterEvent(_firstPrisonerFightStarted);
        GameManager.obj.SetCurrentSpawnPointId(_spawnPoint.SpawnPointID);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
