using UnityEngine;

public class C1Dash12Trigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col) {
        if(col.CompareTag("Player")) {
            PlayerPowersManager.obj.EliCanShadowDash = true;
            Destroy(gameObject);
        }
    }
}
