using UnityEngine;

public class PowerUpRoomsCompletedWallContainer : MonoBehaviour
{
    void Awake() {
        if(GameEventManager.obj.AfterPowerUpRoomsCompletedWallBreak) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}
