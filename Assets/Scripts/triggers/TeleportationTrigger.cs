using System.Collections;
using UnityEngine;

public class TeleportationTrigger : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _spawnPoint;
    [SerializeField] private GameObject _soul;

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
        DustParticleMgr.obj.Enabled = false;  //Prevent any dust from spawning

        GameObject soul = Instantiate(_soul, Player.obj.transform.position, Player.obj.transform.rotation);
        SoundFXManager.obj.PlayPlayerTeleportStart(soul.transform);
        PrisonerSoul prisonerSoul = soul.GetComponent<PrisonerSoul>();
        prisonerSoul.Target = _spawnPoint.transform;
        while (!prisonerSoul.IsTargetReached) {
            Player.obj.transform.position = prisonerSoul.transform.position;
            yield return null;
        }
        SoundFXManager.obj.PlayPlayerTeleportEnd(Player.obj.transform);
        Destroy(prisonerSoul.gameObject);

        PlayerMovement.obj.spriteRenderer.enabled = true;
        PlayerMovement.obj.UnFreeze();
        DustParticleMgr.obj.Enabled = true;
        yield return null;
    }
}
