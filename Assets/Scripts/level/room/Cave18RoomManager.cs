using System.Collections;
using UnityEngine;

public class Cave18RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _secretRevealed;
    [SerializeField] private GameObject _secretWall;

    void Start()
    {
        CaveTimelineId.Id caveTimeline = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(GameManager.obj.HasEvent(_secretRevealed) && caveTimeline == CaveTimelineId.Id.Eli) {
            _secretWall.SetActive(false);
        }
    }

    public void SetSecretRevealed() {
        GameManager.obj.RegisterEvent(_secretRevealed);
    }
}
