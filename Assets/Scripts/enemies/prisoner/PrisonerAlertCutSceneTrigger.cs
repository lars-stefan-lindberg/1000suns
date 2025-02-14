using System.Collections;
using UnityEngine;

public class PrisonerAlertCutSceneTrigger : MonoBehaviour
{
    public BabyPrisoner babyPrisoner;
    public Prisoner prisoner;
    public GameObject lockedDoor;
    public float cutSceneDuration = 4.5f;
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            PlayerMovement.obj.Freeze(cutSceneDuration);
            //Zoom in on prisoner
            SoundFXManager.obj.PlayBabyPrisonerScared(babyPrisoner.transform);
            prisoner.isStatic = true;
            prisoner.gameObject.SetActive(true);
            lockedDoor.SetActive(true);
            StartCoroutine(PrisonerSpawn());
            //Zoom out
        }
    }

    private IEnumerator PrisonerSpawn() {
        yield return new WaitForSeconds(1f);
        babyPrisoner.Despawn();
        yield return new WaitForSeconds(1.5f);
        prisoner.isStatic = false;
        gameObject.SetActive(false);
    }
}
