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
        SHADOW_TWIN, 
        NONE
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
        } else if(playerIdentity.id == 3) {
            return PlayerType.BLOB;
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
        } else if(playerIdentity.id == 3) {
            return PlayerType.BLOB;
        } else {
            Debug.Log("Unknown player identity id: " + playerIdentity.id);
            return PlayerType.HUMAN;
        }
    }
    
    public bool IsEliInBlobForm() {
        return elisLastForm == PlayerType.BLOB;
    }
    
    public void DisablePlayerMovement(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            PlayerMovement.obj.DisablePlayerMovement();
        else if(playerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.DisablePlayerMovement();
        else if(playerType == PlayerType.SHADOW_TWIN)
            ShadowTwinMovement.obj.DisablePlayerMovement();
    }

    public void EnablePlayerMovement(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            PlayerMovement.obj.EnablePlayerMovement();
        else if(playerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.EnablePlayerMovement();
        else if(playerType == PlayerType.SHADOW_TWIN)
            ShadowTwinMovement.obj.EnablePlayerMovement();
    }

    public bool IsFrozen(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            return PlayerMovement.obj.IsFrozen();
        else if(playerType == PlayerType.BLOB)
            return PlayerBlobMovement.obj.IsFrozen();
        else if(playerType == PlayerType.SHADOW_TWIN)
            return ShadowTwinMovement.obj.IsFrozen();
        return false;
    }

    public void KillPlayerGeneric(PlayerType playerType, float genericDeathAnimationTime) {
        if(playerType == PlayerType.HUMAN) {
            PlayerMovement.obj.Freeze(genericDeathAnimationTime);
            Player.obj.PlayGenericDeathAnimation();
        }
        else if(playerType == PlayerType.BLOB) {
            PlayerBlobMovement.obj.Freeze(genericDeathAnimationTime);
            PlayerBlob.obj.PlayGenericDeathAnimation();
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinMovement.obj.Freeze(genericDeathAnimationTime);
            ShadowTwinPlayer.obj.PlayGenericDeathAnimation();
        }
    }

    public void KillAllPlayersGeneric(float genericDeathAnimationTime) {
        if(Player.obj != null && Player.obj.gameObject.activeSelf) {
            PlayerPush.obj.ResetBuiltUpPower();
            PlayerMovement.obj.Freeze(genericDeathAnimationTime);
            Player.obj.FadeOutPlayerLight();
            Player.obj.PlayGenericDeathAnimation();
        }
        if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf) {
            PlayerBlobMovement.obj.Freeze(genericDeathAnimationTime);
            PlayerBlob.obj.FadeOutPlayerLight();
            PlayerBlob.obj.PlayGenericDeathAnimation();
        }
        if(ShadowTwinPlayer.obj != null && ShadowTwinPlayer.obj.gameObject.activeSelf) {
            ShadowTwinPull.obj.CancelPulling();
            ShadowTwinMovement.obj.Freeze(genericDeathAnimationTime);
            ShadowTwinPlayer.obj.FadeOutPlayerLight();
            ShadowTwinPlayer.obj.PlayGenericDeathAnimation();
        }
    }

    public void KillPlayerShadow(PlayerType playerType, float shadowDeathAnimationTime) {
        if(playerType == PlayerType.HUMAN) {
            PlayerPush.obj.ResetBuiltUpPower();
            PlayerMovement.obj.Freeze(shadowDeathAnimationTime);
            Player.obj.FadeOutPlayerLight();
            Player.obj.PlayShadowDeathAnimation();
        }
        else if(playerType == PlayerType.BLOB) {
            PlayerBlobMovement.obj.Freeze(shadowDeathAnimationTime);
            PlayerBlob.obj.FadeOutPlayerLight();
            PlayerBlob.obj.PlayGenericDeathAnimation();
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinPull.obj.CancelPulling();
            ShadowTwinMovement.obj.Freeze(shadowDeathAnimationTime);
            ShadowTwinPlayer.obj.FadeOutPlayerLight();
            ShadowTwinPlayer.obj.PlayShadowDeathAnimation();
        }
    }

    public void SetPlayerGameObjectInactive(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            Player.obj.gameObject.SetActive(false);
        }
        else if(playerType == PlayerType.BLOB) {
            PlayerBlob.obj.gameObject.SetActive(false);
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinPlayer.obj.gameObject.SetActive(false);
        }
    }

    public void SetAllPlayerGameObjectsInactive() {
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

    public void EnableAllPlayers() {
        //TODO might need to handle Eli/blob case here
        Player.obj.gameObject.SetActive(true);
        ShadowTwinPlayer.obj.gameObject.SetActive(true);
    }

    public void EnablePlayerGameObject(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            Player.obj.gameObject.SetActive(true);
        }
        else if(playerType == PlayerType.BLOB) {
            PlayerBlob.obj.gameObject.SetActive(true);
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinPlayer.obj.gameObject.SetActive(true);
        }
    }

    public PlayerType GetActivePlayerType() {
        if(PlayerSwitcher.obj.IsEliActive())
            return PlayerType.HUMAN;
        else if(PlayerSwitcher.obj.IsDeeActive())
            return PlayerType.SHADOW_TWIN;
        else if(PlayerSwitcher.obj.IsBlobActive())
            return PlayerType.BLOB;
        Debug.LogWarning("No active player type.");
        return PlayerType.NONE;
    }

    public void SetTransitioningBetweenLevels(PlayerType playerType) {
        switch(playerType)
        {
            case PlayerType.HUMAN: 
                PlayerMovement.obj.SetTransitioningBetweenLevels();
                break;
            case PlayerType.BLOB:
                PlayerBlobMovement.obj.SetTransitioningBetweenLevels();
                break;
            case PlayerType.SHADOW_TWIN:
                ShadowTwinMovement.obj.SetTransitioningBetweenLevels();
                break;
        }
    }

    public void TransitionToNextRoom(PlayerDirection direction, PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            PlayerMovement.obj.TransitionToNextRoom(direction);
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinMovement.obj.TransitionToNextRoom(direction);
        } else if(playerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.TransitionToNextRoom(direction);
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

    public float GetPlayerVerticalVelocity(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            if(Player.obj != null && Player.obj.gameObject.activeSelf)
                return Player.obj.rigidBody.velocity.y;
        }
        else if(playerType == PlayerType.BLOB) {
            if(PlayerBlob.obj != null && PlayerBlob.obj.gameObject.activeSelf)
                return PlayerBlob.obj.rigidBody.velocity.y;
        }
        else if(playerType == PlayerType.SHADOW_TWIN) {
            if(ShadowTwinPlayer.obj != null && ShadowTwinPlayer.obj.gameObject.activeSelf)
                return ShadowTwinPlayer.obj.rigidBody.velocity.y;
        }
        return 0f;    
    }

    public Transform GetPlayerTransform(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            return Player.obj.transform;
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            return ShadowTwinPlayer.obj.transform;
        } else if(playerType == PlayerType.BLOB) {
            return PlayerBlob.obj.transform;
        } else {
            Debug.Log("Unknown player type: " + playerType);
            return null;
        }
    }

    public bool IsPlayerGrounded(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            return PlayerMovement.obj.isGrounded;
        } else if(playerType == PlayerType.BLOB) {
            return PlayerBlobMovement.obj.isGrounded;
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            return ShadowTwinMovement.obj.isGrounded;
        } else {
            Debug.Log("Unknown player type: " + playerType);
            return false;
        }
    }

    public bool IsPlayerFacingLeft(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            return PlayerMovement.obj.IsFacingLeft();
        } else if(playerType == PlayerType.BLOB) {
            return PlayerBlobMovement.obj.IsFacingLeft();
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            return ShadowTwinMovement.obj.isFacingLeft();
        }
        return false;
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

    public void PlaySpawn(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN) {
            Player.obj.PlaySpawn();
        } else if(playerType == PlayerType.BLOB) {
            PlayerBlob.obj.PlaySpawn();
        } else if(playerType == PlayerType.SHADOW_TWIN) {
            ShadowTwinPlayer.obj.PlaySpawn();
        }
    }

    public Transform GetLeftAvatarTarget(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            return Player.obj.GetLeftAvatarTarget();
        else if(playerType == PlayerType.SHADOW_TWIN)
            return ShadowTwinPlayer.obj.GetLeftAvatarTarget();
        else if(playerType == PlayerType.BLOB)
            return PlayerBlob.obj.GetLeftAvatarTarget();
        return null;
    }

    public Transform GetRightAvatarTarget(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            return Player.obj.GetRightAvatarTarget();
        else if(playerType == PlayerType.SHADOW_TWIN)
            return ShadowTwinPlayer.obj.GetRightAvatarTarget();
        else if(playerType == PlayerType.BLOB)
            return PlayerBlob.obj.GetRightAvatarTarget();
        return null;
    }

    public void FreezePlayer(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            PlayerMovement.obj.Freeze();
        else if(playerType == PlayerType.SHADOW_TWIN)
            ShadowTwinMovement.obj.Freeze();
        else if(playerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.Freeze();
    }

    public void UnfreezePlayer(PlayerType playerType) {
        if(playerType == PlayerType.HUMAN)
            PlayerMovement.obj.UnFreeze();
        else if(playerType == PlayerType.SHADOW_TWIN)
            ShadowTwinMovement.obj.UnFreeze();
        else if(playerType == PlayerType.BLOB)
            PlayerBlobMovement.obj.UnFreeze();
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
