using UnityEngine;

public class MaybeSetFirstBabyPrisonerInactive : MonoBehaviour
{
    [SerializeField] private GameEventId _babyPrisonerAlerted;

    void Awake() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli && GameManager.obj.HasEvent(_babyPrisonerAlerted)) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}
