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

    public enum PlayerType {
        HUMAN,
        BLOB,
        SHADOW_TWIN
    }

    public bool IsSeparated = false;
    public bool IsCoopActive = false;

    private PlayerType _lastPlayerType = PlayerType.HUMAN;
    public PlayerType elisLastForm = PlayerType.HUMAN;

    public PlayerType GetPlayerTypeFromCollision(Collision2D collision) {
        PlayerIdentity playerIdentity = collision.gameObject.GetComponent<PlayerIdentity>();
        if(playerIdentity.id == 1) {
            return PlayerType.HUMAN;
        } else if(playerIdentity.id == 2) {
            return PlayerType.SHADOW_TWIN;
        } else {
            Debug.Log("Unknown player identity id: " + playerIdentity.id);
            return PlayerType.HUMAN;
        }
    }

    public PlayerType GetPlayerTypeFromCollider(Collider2D collider) {
        PlayerIdentity playerIdentity = collider.gameObject.GetComponent<PlayerIdentity>();
        if(playerIdentity.id == 1) {
            return PlayerType.HUMAN;
        } else if(playerIdentity.id == 2) {
            return PlayerType.SHADOW_TWIN;
        } else {
            Debug.Log("Unknown player identity id: " + playerIdentity.id);
            return PlayerType.HUMAN;
        }
    }
    
    public bool IsEliInBlobForm() {
        return elisLastForm == PlayerType.BLOB;
    }
    
    public void DisablePlayerMovement() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            PlayerMovement.obj.DisablePlayerMovement();
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            PlayerBlobMovement.obj.DisablePlayerMovement();
        else if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf)
            ShadowTwinMovement.obj.DisablePlayerMovement();
    }

    public void EnablePlayerMovement() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            PlayerMovement.obj.EnablePlayerMovement();
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            PlayerBlobMovement.obj.EnablePlayerMovement();
        else if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf)
            ShadowTwinMovement.obj.EnablePlayerMovement();
    }

    public bool IsFrozen() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            return PlayerMovement.obj.IsFrozen();
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            return PlayerBlobMovement.obj.IsFrozen();
        else if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf)
            return ShadowTwinMovement.obj.IsFrozen();
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
        else if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf) {
            ShadowTwinMovement.obj.Freeze(genericDeathAnimationTime);
            ShadowTwinPlayer.obj.PlayGenericDeathAnimation();
        }
    }

    public void KillPlayerGeneric(PlayerType playerType, float genericDeathAnimationTime) {
        if(playerType == PlayerType.HUMAN) {
            if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
                PlayerPush.obj.ResetBuiltUpPower();
                PlayerMovement.obj.Freeze(genericDeathAnimationTime);
                Player.obj.FadeOutPlayerLight();
                Player.obj.PlayGenericDeathAnimation();
            }
            else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
                PlayerBlobMovement.obj.Freeze(genericDeathAnimationTime);
                PlayerBlob.obj.FadeOutPlayerLight();
                PlayerBlob.obj.PlayGenericDeathAnimation();
            }
        }
        else {
            ShadowTwinPull.obj.CancelPulling();
            ShadowTwinMovement.obj.Freeze(genericDeathAnimationTime);
            ShadowTwinPlayer.obj.FadeOutPlayerLight();
            ShadowTwinPlayer.obj.PlayGenericDeathAnimation();
        }        
    }

    public void KillPlayerShadow(float shadowDeathAnimationTime) {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
            PlayerPush.obj.ResetBuiltUpPower();
            PlayerMovement.obj.Freeze(shadowDeathAnimationTime);
            Player.obj.FadeOutPlayerLight();
            Player.obj.PlayShadowDeathAnimation();
        }
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
            PlayerBlobMovement.obj.Freeze(shadowDeathAnimationTime);
            PlayerBlob.obj.FadeOutPlayerLight();
            PlayerBlob.obj.PlayGenericDeathAnimation();
        }
        else if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf) {
            ShadowTwinPull.obj.CancelPulling();
            ShadowTwinMovement.obj.Freeze(shadowDeathAnimationTime);
            ShadowTwinPlayer.obj.FadeOutPlayerLight();
            ShadowTwinPlayer.obj.PlayShadowDeathAnimation();
        }
    }

    public void KillPlayerShadow(PlayerType playerType, float shadowDeathAnimationTime) {
        if(playerType == PlayerType.HUMAN) {
            if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
                PlayerPush.obj.ResetBuiltUpPower();
                PlayerMovement.obj.Freeze(shadowDeathAnimationTime);
                Player.obj.FadeOutPlayerLight();
                Player.obj.PlayShadowDeathAnimation();
            }
            else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
                PlayerBlobMovement.obj.Freeze(shadowDeathAnimationTime);
                PlayerBlob.obj.FadeOutPlayerLight();
                PlayerBlob.obj.PlayGenericDeathAnimation();
            }
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf) {
                ShadowTwinPull.obj.CancelPulling();
                ShadowTwinMovement.obj.Freeze(shadowDeathAnimationTime);
                ShadowTwinPlayer.obj.FadeOutPlayerLight();
                ShadowTwinPlayer.obj.PlayShadowDeathAnimation();
            }
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
        else if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf) {
            ShadowTwinPlayer.obj.gameObject.SetActive(false);
            _lastPlayerType = PlayerType.SHADOW_TWIN;
        }
    }

    public void SetPlayerGameObjectsInactive() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
            Player.obj.gameObject.SetActive(false);
        }
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
            PlayerBlob.obj.gameObject.SetActive(false);
        }
        if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf) {
            ShadowTwinPlayer.obj.gameObject.SetActive(false);
        }
    }

    public void SetPlayerGameObjectInactive(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
                Player.obj.gameObject.SetActive(false);
                _lastPlayerType = PlayerType.HUMAN;
            }
            else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
                PlayerBlob.obj.gameObject.SetActive(false);
                _lastPlayerType = PlayerType.BLOB;
            }
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf) {
                ShadowTwinPlayer.obj.gameObject.SetActive(false);
                _lastPlayerType = PlayerType.SHADOW_TWIN;
            }
        }
    }

    public void EnableAllPlayers() {
        if(Player.obj != null) {
            Player.obj.gameObject.SetActive(true);
        } else if(PlayerBlob.obj != null) {
            PlayerBlob.obj.gameObject.SetActive(true);
        }
        ShadowTwinPlayer.obj.gameObject.SetActive(true);
    }

    public void EnableLastActivePlayerGameObject() {
        if(_lastPlayerType == PlayerType.HUMAN) {
            Player.obj.gameObject.SetActive(true);
        }
        else if(_lastPlayerType == PlayerType.BLOB) {
            PlayerBlob.obj.gameObject.SetActive(true);
        }
        else if(_lastPlayerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinPlayer.obj.gameObject.SetActive(true);
        }
    }

    public void SetLastActivePlayerType(PlayerType type) {
        _lastPlayerType = type;
    }

    public PlayerType GetActivePlayerType() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            return PlayerType.HUMAN;
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            return PlayerType.BLOB;
        else if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf)
            return PlayerType.SHADOW_TWIN;
        return PlayerType.HUMAN;
    }

    public PlayerType GetLastActivePlayerType() {
        return _lastPlayerType;
    }

    public void SetTransitioningBetweenLevels() {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            PlayerMovement.obj.SetTransitioningBetweenLevels();
        if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            PlayerBlobMovement.obj.SetTransitioningBetweenLevels();
        if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf)
            ShadowTwinMovement.obj.SetTransitioningBetweenLevels();
    }

    public void TransitionToNextRoom(PlayerDirection direction) {
        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
            PlayerMovement.obj.TransitionToNextRoom(direction);
        else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
            PlayerBlobMovement.obj.TransitionToNextRoom(direction);
        else if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf)
            ShadowTwinMovement.obj.TransitionToNextRoom(direction);
    }

    public void TransitionToNextRoom(PlayerDirection direction, PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf)
                PlayerMovement.obj.TransitionToNextRoom(direction);
            else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf)
                PlayerBlobMovement.obj.TransitionToNextRoom(direction);
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinMovement.obj.TransitionToNextRoom(direction);
        }
    }

    public void TeleportSecondPlayerToTarget(PlayerType playerType, Collider2D target) {
        if(playerType == PlayerType.SHADOW_TWIN) {
            if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
                PlayerMovement.obj.TeleportToNextRoom(target);
            }
            else if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
                PlayerBlobMovement.obj.TeleportToNextRoom(target);
            }
        } else if(playerType == PlayerType.HUMAN) {
            ShadowTwinMovement.obj.TeleportToNextRoom(target);
        }
    }

    public float GetPlayerVerticalVelocity() {
        if(Player.obj != null && Player.obj.gameObject.activeSelf)
            return Player.obj.rigidBody.velocity.y;
        else if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
            return PlayerBlob.obj.rigidBody.velocity.y;
        else if(ShadowTwinPlayer.obj != null && ShadowTwinPlayer.obj.gameObject.activeSelf)
            return ShadowTwinPlayer.obj.rigidBody.velocity.y;
        return 0f;    
    }

    public Transform GetPlayerTransform() {
        if(Player.obj != null && Player.obj.gameObject.activeSelf)
            return Player.obj.transform;
        else if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
            return PlayerBlob.obj.transform;
        else if(ShadowTwinPlayer.obj != null && ShadowTwinPlayer.obj.gameObject.activeSelf)
            return ShadowTwinPlayer.obj.transform;
        return null;
    }

    public Transform GetPlayerTransform(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            if(Player.obj != null && Player.obj.gameObject.activeSelf)
                return Player.obj.transform;
            else if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
                return PlayerBlob.obj.transform;
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            return ShadowTwinPlayer.obj.transform;
        } else {
            Debug.Log("Unknown player type: " + playerType);
            return null;
        }
        return null;
    }

    public bool IsPlayerGrounded(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            if(Player.obj != null && Player.obj.gameObject.activeSelf)
                return PlayerMovement.obj.isGrounded;
            else if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
                return PlayerBlobMovement.obj.isGrounded;
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            return ShadowTwinMovement.obj.isGrounded;
        } else {
            Debug.Log("Unknown player type: " + playerType);
            return false;
        }
        return false;
    }

    //Only use in LevelManager!
    public bool IsPlayerFacingLeft() {
        if(Player.obj != null && _lastPlayerType == PlayerType.HUMAN)
            return PlayerMovement.obj.isFacingLeft();
        else if(PlayerBlob.obj != null && _lastPlayerType == PlayerType.BLOB)
            return PlayerBlobMovement.obj.IsFacingLeft();
        else if(ShadowTwinPlayer.obj != null && _lastPlayerType == PlayerType.SHADOW_TWIN)
            return ShadowTwinMovement.obj.isFacingLeft();
        return false;
    }

    public bool IsPlayerFacingLeft(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            if(Player.obj != null && _lastPlayerType == PlayerType.HUMAN)
                return PlayerMovement.obj.isFacingLeft();
            else if(PlayerBlob.obj != null && _lastPlayerType == PlayerType.BLOB)
                return PlayerBlobMovement.obj.IsFacingLeft();
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            return ShadowTwinMovement.obj.isFacingLeft();
        }
        return false;
    }

    //Only use in LevelManager!
    public void FlipPlayer() {
        if(Player.obj != null && _lastPlayerType == PlayerType.HUMAN)
            PlayerMovement.obj.FlipPlayer();
        else if(PlayerBlob.obj != null && _lastPlayerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.FlipPlayer();
        else if(ShadowTwinPlayer.obj != null && _lastPlayerType == PlayerType.SHADOW_TWIN)
            ShadowTwinMovement.obj.FlipPlayer();
    }

    public void FlipPlayer(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            if(Player.obj != null && _lastPlayerType == PlayerType.HUMAN)
                PlayerMovement.obj.FlipPlayer();
            else if(PlayerBlob.obj != null && _lastPlayerType == PlayerType.BLOB)
                PlayerBlobMovement.obj.FlipPlayer();
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinMovement.obj.FlipPlayer();
        }
    }

    public void PlaySpawn() {
        if(Player.obj != null && Player.obj.gameObject.activeSelf)
            Player.obj.PlaySpawn();
        else if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
            PlayerBlob.obj.PlaySpawn();
        else if(ShadowTwinPlayer.obj != null && ShadowTwinPlayer.obj.gameObject.activeSelf)
            ShadowTwinPlayer.obj.PlaySpawn();
    }

    public void PlaySpawn(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            if(Player.obj != null && Player.obj.gameObject.activeSelf)
                Player.obj.PlaySpawn();
            else if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
                PlayerBlob.obj.PlaySpawn();
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            if(ShadowTwinPlayer.obj != null && ShadowTwinPlayer.obj.gameObject.activeSelf)
                ShadowTwinPlayer.obj.PlaySpawn();
        }
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
