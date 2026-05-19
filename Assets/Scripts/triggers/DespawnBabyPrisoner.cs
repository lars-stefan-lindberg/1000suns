using UnityEngine;

public class DespawnBabyPrisoner : MonoBehaviour
{
    [SerializeField] private GameEventId _babyPrisonerAlerted;
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("BabyPrisoner")) {
            if(_babyPrisonerAlerted != null)
                GameManager.obj.RegisterEvent(_babyPrisonerAlerted);
            BabyPrisoner babyPrisoner = other.gameObject.GetComponent<BabyPrisoner>();
            babyPrisoner.Disable();
        }
    }
}
