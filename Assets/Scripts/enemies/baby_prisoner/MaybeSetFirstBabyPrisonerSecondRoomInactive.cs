using UnityEngine;

public class MaybeSetFirstBabyPrisonerSecondRoomInactive : MonoBehaviour
{
    [SerializeField] private GameEventId _firstPrisonerFightStarted;

    void Awake() {
        if(GameManager.obj.HasEvent(_firstPrisonerFightStarted)) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}
