using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager obj;

    public void DisablePlayerMovement() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            PlayerMovement.obj.DisablePlayerMovement();
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            PlayerBlobMovement.obj.DisablePlayerMovement();
    }

    public void EnablePlayerMovement() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            PlayerMovement.obj.EnablePlayerMovement();
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            PlayerBlobMovement.obj.EnablePlayerMovement();
    }

    public bool IsFrozen() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            return PlayerMovement.obj.IsFrozen();
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            return PlayerBlobMovement.obj.IsFrozen();
        return false;
    }

    public void KillPlayerGeneric() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            Reaper.obj.KillPlayerGeneric();
    }

    void Awake()
    {
        obj = this;
    }

    void OnDestroy()
    {
        obj = null;
    }
}
