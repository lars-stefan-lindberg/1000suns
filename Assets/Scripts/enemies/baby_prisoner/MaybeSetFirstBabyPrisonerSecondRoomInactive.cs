using UnityEngine;

public class MaybeSetFirstBabyPrisonerSecondRoomInactive : MonoBehaviour
{
    void Awake() {
        if(GameManager.obj.FirstPrisonerFightStarted) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}
