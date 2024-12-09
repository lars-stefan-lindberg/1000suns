using UnityEngine;

public class RecoverFirstPowerUpTrigger : MonoBehaviour
{
    [SerializeField] private PowerUpRoomCutScene _powerUpRoomCutScene;
    private BoxCollider2D _collider;

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            _powerUpRoomCutScene.SetFirstPowerUpRecovered();
            _collider.enabled = false;
        }
    }
}
