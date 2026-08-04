using UnityEngine;

public class Cave23RetryRoomManager : MonoBehaviour, IRetryable
{
    [SerializeField] private GameEventId _dreamSequenceCompleted;
    [SerializeField] private GameEventId _postDreamSequenceCompleted;

    public bool IsRetryable()
    {
        bool notRetryable = GameManager.obj.GetCaveTimeline().GetCaveTimelineId() == CaveTimelineId.Id.Eli 
            && GameManager.obj.HasEvent(_dreamSequenceCompleted) 
            && !GameManager.obj.HasEvent(_postDreamSequenceCompleted);

        return !notRetryable;
    }
}
