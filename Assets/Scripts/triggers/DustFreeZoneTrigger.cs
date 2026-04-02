using UnityEngine;

public class DustFreeZoneTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider) {
        if(!collider.CompareTag("Player"))
            return;

        DustParticleMgr.obj.Enabled = false;
    }

    void OnTriggerExit2D(Collider2D collider) {
        if(!collider.CompareTag("Player"))
            return;

        DustParticleMgr.obj.Enabled = true;
    }
}
