using UnityEngine;

public class Cave36RoomManager : MonoBehaviour
{
    [SerializeField] private GameObject _musicTrigger;
    [SerializeField] private GameEventId _hasShadowJump;

    void Start()
    {
        CaveTimelineId.Id id = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(id != CaveTimelineId.Id.Eli || !GameManager.obj.HasEvent(_hasShadowJump))
            _musicTrigger.SetActive(false);
    }
}
