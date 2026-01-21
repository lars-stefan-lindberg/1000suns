using UnityEngine;

public class BackgroundBlobPowerUpRooms : MonoBehaviour
{
    private bool _isDestroyed = false;
    void Awake() {
        if(GameManager.obj.FirstPowerUpPicked) {
            _isDestroyed = true;
            Destroy(gameObject);
        }
    }

    void FixedUpdate() {
        if(GameManager.obj.FirstPowerUpPicked && !_isDestroyed) {
            Destroy(gameObject);
            _isDestroyed = true;
        }
    }
}
