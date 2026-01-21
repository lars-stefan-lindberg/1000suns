using System.Collections;
using UnityEngine;

public class PrisonerAlertCutSceneTrigger : MonoBehaviour
{
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
                MusicManager.obj.StopPlaying();
                PlayerMovement.obj.Freeze(cutSceneDuration);
                MusicManager.obj.PlayCaveIntense1();
                SoundFXManager.obj.PlayBabyPrisonerScared(babyPrisoner.transform);
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
