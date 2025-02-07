using UnityEngine;

public class PowerUpRoomWallsContainer : MonoBehaviour
{
    void Awake() {
        if(GameEventManager.obj.PowerUpRoomCompletedWallBreak) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}

