using UnityEngine;

public class RoomState : MonoBehaviour
{
    public GameEventId eventId;
    void Start()
    {
        if(GameManager.obj.Progress.HasEvent(eventId))
            gameObject.SetActive(false);
    }
}
