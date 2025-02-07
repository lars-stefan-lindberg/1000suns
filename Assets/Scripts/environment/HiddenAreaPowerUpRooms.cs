using UnityEngine;

public class HiddenAreaPowerUpRooms : MonoBehaviour
{
    private bool _isDestroyed = false;
    void Awake() {
        if(GameEventManager.obj.FirstPowerUpPicked) {
            _isDestroyed = true;
            Destroy(gameObject);
        }
    }

    void FixedUpdate() {
        if(GameEventManager.obj.FirstPowerUpPicked && !_isDestroyed) {
            Destroy(gameObject);
            _isDestroyed = true;
        }
    }
}
