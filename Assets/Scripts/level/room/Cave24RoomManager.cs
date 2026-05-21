using UnityEngine;

public class Cave24RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject _breakableWall;
    [SerializeField] private GameObject _torchUpperLit;
    [SerializeField] private GameObject _torchUpperUnlit;
    [SerializeField] private GameObject _torchLowerLit;
    [SerializeField] private GameObject _torchLowerUnlit;
    [SerializeField] private GameEventId _wallBroken;

    void Start()
    {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimeline == CaveTimelineId.Id.Eli) {
            if(GameManager.obj.HasEvent(_wallBroken))
                _breakableWall.SetActive(false);
            _torchLowerLit.SetActive(true);
            _torchUpperUnlit.SetActive(true);
        } else if(caveTimeline == CaveTimelineId.Id.Dee) {
            _torchUpperLit.SetActive(true);
            _torchLowerUnlit.SetActive(true);
        } else if(caveTimeline == CaveTimelineId.Id.Both) {
            _torchLowerLit.SetActive(true);
            _torchUpperLit.SetActive(true);
            _breakableWall.SetActive(false);
        }
    }

    public void RegisterWallBroken() {
        GameManager.obj.RegisterEvent(_wallBroken);
    }
}
