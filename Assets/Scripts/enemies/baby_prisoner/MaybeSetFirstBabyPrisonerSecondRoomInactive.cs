using UnityEngine;

public class MaybeSetFirstBabyPrisonerSecondRoomInactive : MonoBehaviour
{
    void Awake() {
        if(GameEventManager.obj.FirstPrisonerFightStarted) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}
