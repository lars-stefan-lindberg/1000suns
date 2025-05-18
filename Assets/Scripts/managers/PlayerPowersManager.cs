using UnityEngine;

public class PlayerPowersManager : MonoBehaviour
{
    public static PlayerPowersManager obj;

    public bool CanFallDash {get; set;}
    public bool CanForcePushJump {get; set;}
    public bool CanTurnFromHumanToBlob { get; set; }
    public bool CanTurnFromBlobToHuman { get; set; }
    public bool BlobCanJump { get; set; }
    public bool BlobCanExtraJump { get; set; }

    void Awake() {
        obj = this;
        ResetGameEvents();
        if(PlayerMovement.obj.isDevMode) {
            CanFallDash = true;
            CanForcePushJump = true;
            CanTurnFromBlobToHuman = true;
            CanTurnFromHumanToBlob = true;
            BlobCanJump = true;
            BlobCanExtraJump = true;
        }     
    }

    void OnEnable()
    {
        if(PlayerMovement.obj.isDevMode) {
            CanFallDash = true;
            CanForcePushJump = true;
            CanTurnFromBlobToHuman = true;
            CanTurnFromHumanToBlob = true;
            BlobCanJump = true;
            BlobCanExtraJump = true;
        }     
    }

    public void ResetGameEvents() {
        CanFallDash = false;
        CanForcePushJump = false;
        CanTurnFromHumanToBlob = false;
        CanTurnFromBlobToHuman = false;
        BlobCanJump = false;
        BlobCanExtraJump = false;
    }

    void OnDestroy() {
        obj = null;
    }
}
