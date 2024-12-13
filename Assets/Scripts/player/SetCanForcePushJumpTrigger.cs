using UnityEngine;

public class SetCanForcePushJumpTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            Player.obj.CanForcePushJump = true;
            GameEventManager.obj.AfterPowerUpRoomsCompletedWallBreak = true;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
