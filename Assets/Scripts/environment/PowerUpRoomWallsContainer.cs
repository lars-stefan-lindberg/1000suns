using UnityEngine;

public class PowerUpRoomWallsContainer : MonoBehaviour
{
    void Awake() {
        if(GameEventManager.obj.FirstPowerUpPicked) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}

