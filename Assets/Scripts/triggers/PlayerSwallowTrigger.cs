using UnityEngine;

public class PlayerSwallowTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            EndRoomBackgroundBlobManager.obj.StartCutscene();
        }
    }
}
