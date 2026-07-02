using System.Collections;
using FMODUnity;
using UnityEngine;

public class TeleportationTrigger : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _spawnPoint;
    [SerializeField] private GameObject _soul;
    [SerializeField] private EventReference _teleportationStartSfx;
    [SerializeField] private EventReference _teleportationEndSfx;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(TeleportPlayer());
        }
    }

    private IEnumerator TeleportPlayer() {
        PlayerMovement.obj.Freeze();
        PlayerMovement.obj.spriteRenderer.enabled = false;
        PlayerMovement.obj.DisableCollider();
        DustParticleMgr.obj.Enabled = false;  //Prevent any dust from spawning

        GameObject soul = Instantiate(_soul, Player.obj.transform.position, Player.obj.transform.rotation);
        SoundFXManager.obj.PlayAtGameObject(_teleportationStartSfx, soul);
        PrisonerSoul prisonerSoul = soul.GetComponent<PrisonerSoul>();
        prisonerSoul.Target = _spawnPoint.transform.position;
        while (!prisonerSoul.IsTargetReached) {
            Player.obj.transform.position = prisonerSoul.transform.position;
            yield return null;
        }
        SoundFXManager.obj.PlayAtGameObject(_teleportationEndSfx, Player.obj.gameObject);
        Destroy(prisonerSoul.gameObject);

        Player.obj.transform.position = _spawnPoint.transform.position;
        PlayerMovement.obj.EnableCollider();
        PlayerMovement.obj.spriteRenderer.enabled = true;
        PlayerMovement.obj.UnFreeze();
        DustParticleMgr.obj.Enabled = true;
        yield return null;
    }
}
