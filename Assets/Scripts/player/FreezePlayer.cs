using UnityEngine;

public class FreezePlayer : MonoBehaviour
{
    private bool _hasBeenTriggered = false;

    void Awake() {
        if(GameEventManager.obj.FirstPowerUpPicked) {
            _hasBeenTriggered = true;
            Destroy(gameObject, 3);
        }
    }

     void OnTriggerEnter2D(Collider2D other) {
        if(_hasBeenTriggered)
            return;
        if(other.CompareTag("Player")) {
            PlayerMovement.obj.Freeze(4f);
            _hasBeenTriggered = true;
        }
    }
}
