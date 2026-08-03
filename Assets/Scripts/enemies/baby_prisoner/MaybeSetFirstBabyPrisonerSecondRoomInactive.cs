using UnityEngine;

public class MaybeSetFirstBabyPrisonerSecondRoomInactive : MonoBehaviour
{
    [SerializeField] private GameEventId _firstPrisonerFightStarted;

    void Start() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli && GameManager.obj.HasEvent(_firstPrisonerFightStarted)) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        } else if(caveTimeline == CaveTimelineId.Id.Dee) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}
