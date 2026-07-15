using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave3RoomManager : MonoBehaviour
{
    [SerializeField] private GameEventId _cutsceneCompleted;
    
    public void StartCutscene()
    {
        if(GameManager.obj.HasEvent(_cutsceneCompleted)) {
            return;
        }
        CaveAvatar.obj.IsFollowingPlayer = false;
        GameManager.obj.RegisterEvent(_cutsceneCompleted);
    }
}
