using UnityEngine;

public class FreezePlayer : MonoBehaviour
{
    private bool _hasBeenTriggered = false;
     void OnTriggerEnter2D(Collider2D other) {
        if(_hasBeenTriggered)
            return;
        if(other.CompareTag("Player")) {
            PlayerMovement.obj.Freeze(4f);
            _hasBeenTriggered = true;
        }
    }
}
