using UnityEngine;

public class C1Dash12Trigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col) {
        if(col.CompareTag("Player")) {
            PlayerPowersManager.obj.CanShadowDash = true;
            SoundFXManager.obj.PlayCrystalRoomRumble();
            CameraShakeManager.obj.ShakeCamera(1.94f, 1.84f, 4.9f);
            transform.GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
