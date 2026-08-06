using UnityEngine;

public class PowerUpRoomsFloorManager : MonoBehaviour
{
    [SerializeField] private GameObject _floor;
    [SerializeField] private GameEventId _cave33FloorBroken;

    void Start() {
        CaveTimelineId.Id id = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(id == CaveTimelineId.Id.Eli && GameManager.obj.HasEvent(_cave33FloorBroken))
            _floor.SetActive(false);
        else if(id == CaveTimelineId.Id.Both || id == CaveTimelineId.Id.Dee)
            _floor.SetActive(false);
    }
}
