using UnityEngine;

public class MaybeSetFirstBabyPrisonerInactive : MonoBehaviour
{
    [SerializeField] private GameEventId _babyPrisonerAlerted;

    void Awake() {
        if(GameManager.obj.HasEvent(_babyPrisonerAlerted)) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}
