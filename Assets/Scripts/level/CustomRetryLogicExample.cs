using UnityEngine;

public class CustomRetryLogicExample : MonoBehaviour, IRetryable
{
    [SerializeField] private GameEventId _roomCompletedEventId;
    
    public bool IsRetryable()
    {
        return !GameManager.obj.Progress.HasEvent(_roomCompletedEventId);
    }
}
