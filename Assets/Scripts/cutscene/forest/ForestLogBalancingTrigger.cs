using UnityEngine;

public class ForestLogBalancingTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider) {
        if(!collider.CompareTag("Player")) {
            return;
        }
        PlayerMovement.obj.SetIsBalancing(true);
    }

    void OnTriggerExit2D(Collider2D collider) {
        if(!collider.CompareTag("Player")) {
            return;
        }
        PlayerMovement.obj.SetIsBalancing(false);
    }
}
