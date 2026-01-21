using UnityEngine;

public class PowerUpRoomsCompletedWallContainer : MonoBehaviour
{
    void Awake() {
        if(GameManager.obj.AfterPowerUpRoomsCompletedWallBreak) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}
