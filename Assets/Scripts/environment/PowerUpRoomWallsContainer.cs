using UnityEngine;

public class PowerUpRoomWallsContainer : MonoBehaviour
{
    void Awake() {
        if(GameManager.obj.PowerUpRoomCompletedWallBreak) {
            gameObject.SetActive(false);
            Destroy(gameObject, 3);
        }
    }
}

