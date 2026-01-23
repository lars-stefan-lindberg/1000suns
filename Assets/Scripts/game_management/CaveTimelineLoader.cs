using UnityEngine;

public class CaveTimelineLoader : MonoBehaviour
{
    [SerializeField] private CaveTimelineId timelineId;
    
    void Start()
    {
        if(GameManager.obj.GetCaveTimeline().IsTimeline(timelineId))
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }
}
