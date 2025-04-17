using UnityEngine;

public class PlayerPowersManager : MonoBehaviour
{
    public static PlayerPowersManager obj;

    public bool CanTurnFromBlobToHuman { get; set; }
    public bool BlobCanJump { get; set; }
    public bool BlobCanExtraJump { get; set; }

    void Awake() {
        obj = this;
        ResetGameEvents();
        if(PlayerMovement.obj.isDevMode) {
            CanTurnFromBlobToHuman = true;
            BlobCanJump = true;
            BlobCanExtraJump = true;
        }
    }

    public void ResetGameEvents() {
        CanTurnFromBlobToHuman = false;
        BlobCanJump = false;
        BlobCanExtraJump = false;
    }

    void OnDestroy() {
        obj = null;
    }
}
