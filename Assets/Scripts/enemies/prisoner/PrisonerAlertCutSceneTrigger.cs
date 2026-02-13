using System.Collections;
using UnityEngine;

public class PrisonerAlertCutSceneTrigger : MonoBehaviour
{
    [SerializeField] private MusicTrack _musicTrack;
    public BabyPrisoner babyPrisoner;
    public Prisoner prisoner;
    public GameObject lockedDoor;
    public float cutSceneDuration = 4.5f;
    void OnTriggerEnter2D(Collider2D other) {
        if(GameManager.obj.FirstPrisonerKilled) return;
        if(other.gameObject.CompareTag("Player")) {
            if(GameManager.obj.FirstPrisonerFightStarted) {
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
        GameManager.obj.FirstPrisonerFightStarted = true;
    }
}
