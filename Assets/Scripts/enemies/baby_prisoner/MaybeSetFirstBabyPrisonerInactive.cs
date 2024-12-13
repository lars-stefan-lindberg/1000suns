using UnityEngine;

public class MaybeSetFirstBabyPrisonerInactive : MonoBehaviour
{
    void Awake() {
        if(GameEventManager.obj.BabyPrisonerAlerted) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}
