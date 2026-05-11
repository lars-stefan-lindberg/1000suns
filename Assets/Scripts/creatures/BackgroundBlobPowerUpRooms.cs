using UnityEngine;

public class BackgroundBlobPowerUpRooms : MonoBehaviour
{
    [SerializeField] private GameEventId _shadowJumpReceived;

    private bool _isDestroyed = false;
    void Awake() {
        if(GameManager.obj.HasEvent(_shadowJumpReceived)) {
            _isDestroyed = true;
            Destroy(gameObject);
        }
    }

    void FixedUpdate() {
        if(GameManager.obj.HasEvent(_shadowJumpReceived) && !_isDestroyed) {
            Destroy(gameObject);
            _isDestroyed = true;
        }
    }
}
