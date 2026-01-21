using UnityEngine;

public class SetCaveAvatarTarget : MonoBehaviour
{
    void Awake()
    {
        CaveAvatar.obj.gameObject.SetActive(true);
        CaveAvatar.obj.SetFloatingEnabled(false);
        CaveAvatar.obj.SetStartingPositionInRoom1();
        CaveAvatar.obj.SetFlipX(true);
    }
}
