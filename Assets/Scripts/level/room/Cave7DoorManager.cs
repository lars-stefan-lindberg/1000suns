using UnityEngine;

public class Cave7DoorManager : MonoBehaviour
{
    
    [SerializeField] private GameEventId _eliDoorClosed;
    [SerializeField] private GameEventId _deeDoorClosed;
    [SerializeField] private PillarDoor _door;

    void Awake() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli && !GameManager.obj.HasEvent(_eliDoorClosed)) {
            _door.SetFullyOpenImmediate();
        } else if(caveTimeline == CaveTimelineId.Id.Dee && !GameManager.obj.HasEvent(_deeDoorClosed)) {
            _door.SetFullyOpenImmediate();
        }
    }

    public void CloseDoor() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli && !GameManager.obj.HasEvent(_eliDoorClosed)) {
            _door.Close();
            GameManager.obj.RegisterEvent(_eliDoorClosed);
        } else if(caveTimeline == CaveTimelineId.Id.Dee && !GameManager.obj.HasEvent(_deeDoorClosed)) {
            _door.Close();
            GameManager.obj.RegisterEvent(_deeDoorClosed);
        }
    }
}
