using UnityEngine;
using UnityEngine.SceneManagement;

public class SetCanForcePushJumpTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            PlayerPowersManager.obj.CanForcePushJump = true;
            GameEventManager.obj.AfterPowerUpRoomsCompletedWallBreak = true;
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
