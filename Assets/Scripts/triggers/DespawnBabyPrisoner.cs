using UnityEngine;

public class DespawnBabyPrisoner : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("BabyPrisoner")) {
            BabyPrisoner babyPrisoner = other.gameObject.GetComponent<BabyPrisoner>();
            babyPrisoner.Disable();
            GameEventManager.obj.BabyPrisonerAlerted = true;
        }
    }
}
