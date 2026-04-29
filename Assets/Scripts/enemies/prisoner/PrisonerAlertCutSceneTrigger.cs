using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrisonerAlertCutSceneTrigger : MonoBehaviour
{
    [SerializeField] private MusicTrack _musicTrack;
    [SerializeField] private GameEventId _firstPrisonerFightEnded;
    [SerializeField] private GameEventId _firstPrisonerFightStarted;
    [SerializeField] private SpawnPoint _spawnPoint;
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
            } else {
                PlayerMovement.obj.Freeze(cutSceneDuration);
                MusicManager.obj.Play(_musicTrack);
                babyPrisoner.PlayScaredSfx();
                prisoner.isStatic = true;
                prisoner.gameObject.SetActive(true);
                lockedDoor.SetActive(true);
                StartCoroutine(PrisonerSpawn());
            }
        }
    }

    private IEnumerator PrisonerSpawn() {
        yield return new WaitForSeconds(1f);
        babyPrisoner.Despawn();
        yield return new WaitForSeconds(2.3f);
        prisoner.isStatic = false;
        gameObject.SetActive(false);
        GameManager.obj.RegisterEvent(_firstPrisonerFightStarted);
        GameManager.obj.SetCurrentSpawnPointId(_spawnPoint.SpawnPointID);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
    }
}
