using UnityEngine;

public class MaybeSetFirstBabyPrisonerInactive : MonoBehaviour
{
    void Awake() {
        if(GameManager.obj.BabyPrisonerAlerted) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}
