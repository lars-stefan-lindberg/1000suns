using UnityEngine;

public class SetCaveAvatarTarget : MonoBehaviour
{
    public GameEventId sootFreedEvent;
    void Awake()
    {
        if(!GameManager.obj.HasEvent(sootFreedEvent)) {    
            CaveAvatar.obj.gameObject.SetActive(true);
            CaveAvatar.obj.SetFloatingEnabled(false);
            CaveAvatar.obj.SetStartingPositionInRoom1();
            CaveAvatar.obj.SetFlipX(true);
        }
    }
}
