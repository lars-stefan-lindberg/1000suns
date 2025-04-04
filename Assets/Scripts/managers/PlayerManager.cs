using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager obj;

    public enum PlayerDirection {
        RIGHT,
        LEFT,
        UP,
        DOWN,
        NO_DIRECTION
    }

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

    public void SetTransitioningBetweenLevels() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            PlayerMovement.obj.SetTransitioningBetweenLevels();
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            PlayerBlobMovement.obj.SetTransitioningBetweenLevels();
    }

    public void TransitionToNextRoom(PlayerManager.PlayerDirection direction) {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            PlayerMovement.obj.TransitionToNextRoom(direction);
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            PlayerBlobMovement.obj.TransitionToNextRoom(direction);
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
