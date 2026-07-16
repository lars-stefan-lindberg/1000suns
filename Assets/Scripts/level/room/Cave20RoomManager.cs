using UnityEngine;

public class Cave20RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _secretRevealed;
    [SerializeField] private GameEventId _eliCompletedRoom;
    [SerializeField] private GameObject _secretWall;
    [SerializeField] private GameObject[] _prisonerTriggers;

    void Start()
    {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(GameManager.obj.HasEvent(_eliCompletedRoom) && caveTimeline == CaveTimelineId.Id.Eli) {
            foreach(GameObject trigger in _prisonerTriggers) {
                trigger.SetActive(false);
            }
        }

        if(GameManager.obj.HasEvent(_secretRevealed)) {
            _secretWall.SetActive(false);
        }
    }

    public void SetSecretRevealed() {
        GameManager.obj.RegisterEvent(_secretRevealed);
    }

    public void SetEliCompletedRoom() {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli) {
            GameManager.obj.RegisterEvent(_eliCompletedRoom);
        }
    }
}
