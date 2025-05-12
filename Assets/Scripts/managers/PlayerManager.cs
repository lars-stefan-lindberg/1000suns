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

    private enum PlayerType {
        HUMAN,
        BLOB
    }

    private PlayerType _lastPlayerType = PlayerType.HUMAN;

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

    public void KillPlayerGeneric(float genericDeathAnimationTime) {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
            PlayerMovement.obj.Freeze(genericDeathAnimationTime);
            Player.obj.PlayGenericDeathAnimation();
        }
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
            PlayerBlobMovement.obj.Freeze(genericDeathAnimationTime);
            PlayerBlob.obj.PlayGenericDeathAnimation();
        }
    }

    public void KillPlayerShadow(float shadowDeathAnimationTime) {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
            PlayerMovement.obj.Freeze(shadowDeathAnimationTime);
            Player.obj.PlayShadowDeathAnimation();
        }
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
            PlayerBlobMovement.obj.Freeze(shadowDeathAnimationTime);
            PlayerBlob.obj.PlayGenericDeathAnimation();
        }
    }

    public void SetPlayerGameObjectInactive() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
            Player.obj.gameObject.SetActive(false);
            _lastPlayerType = PlayerType.HUMAN;
        }
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
            PlayerBlob.obj.gameObject.SetActive(false);
            _lastPlayerType = PlayerType.BLOB;
        }
    }

    public void EnableLastActivePlayerGameObject() {
        if(_lastPlayerType == PlayerType.HUMAN) {
            Player.obj.gameObject.SetActive(true);
        }
        else if(_lastPlayerType == PlayerType.BLOB) {
            PlayerBlob.obj.gameObject.SetActive(true);
        }
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

    public float GetPlayerVerticalVelocity() {
        if(Player.obj != null && Player.obj.gameObject.activeSelf)
            return Player.obj.rigidBody.velocity.y;
        else if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
            return PlayerBlob.obj.rigidBody.velocity.y;
        return 0f;    
    }

    public Transform GetPlayerTransform() {
        if(Player.obj != null && Player.obj.gameObject.activeSelf)
            return Player.obj.transform;
        else if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
            return PlayerBlob.obj.transform;
        return null;
    }

    public bool IsPlayerGrounded() {
        if(Player.obj != null && Player.obj.gameObject.activeSelf)
            return PlayerMovement.obj.isGrounded;
        else if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
            return PlayerBlobMovement.obj.isGrounded;
        return false;
    }

    //Only use in LevelManager!
    public bool IsPlayerFacingLeft() {
        if(Player.obj != null && _lastPlayerType == PlayerType.HUMAN)
            return PlayerMovement.obj.isFacingLeft();
        else if(PlayerBlob.obj != null && _lastPlayerType == PlayerType.BLOB)
            return PlayerBlobMovement.obj.IsFacingLeft();
        return false;
    }

    //Only use in LevelManager!
    public void FlipPlayer() {
        if(Player.obj != null && _lastPlayerType == PlayerType.HUMAN)
            PlayerMovement.obj.FlipPlayer();
        else if(PlayerBlob.obj != null && _lastPlayerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.FlipPlayer();
    }

    public void PlaySpawn() {
        if(Player.obj != null && Player.obj.gameObject.activeSelf)
            Player.obj.PlaySpawn();
        else if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
            PlayerBlob.obj.PlaySpawn();
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
