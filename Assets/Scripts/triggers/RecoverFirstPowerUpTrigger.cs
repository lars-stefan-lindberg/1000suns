using UnityEngine;

public class RecoverFirstPowerUpTrigger : MonoBehaviour
{
    [SerializeField] private PowerUpRoomCutScene _powerUpRoomCutScene;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            if(GameEventManager.obj.FirstPowerUpPicked)
                _powerUpRoomCutScene.SetFirstPowerUpRecovered();
        }
    }
}
