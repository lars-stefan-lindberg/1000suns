using UnityEngine;

public class MothDeactivationCrystal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            //Deprecated
            //SoundFXManager.obj.PlayPlayerTeleportEnd(transform);
            MothsManager.obj.SendActiveToCrystal(transform.position);
        }
    }
}
