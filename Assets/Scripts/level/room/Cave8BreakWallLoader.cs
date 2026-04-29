using UnityEngine;

public class Cave8BreakWallLoader : MonoBehaviour
{
    [SerializeField] private GameEventId _wallBroken;

    void Awake()
    {
        if(GameManager.obj.GetCaveTimeline().GetCaveTimelineId() == CaveTimelineId.Id.Dee)
            return;
        
        if(GameManager.obj.GetCaveTimeline().GetCaveTimelineId() == CaveTimelineId.Id.Eli) {
            if(GameManager.obj.HasEvent(_wallBroken)) {
                gameObject.SetActive(false);
            }
        }

        if(GameManager.obj.GetCaveTimeline().GetCaveTimelineId() == CaveTimelineId.Id.Both) {
            gameObject.SetActive(false);
        }
    }
}
