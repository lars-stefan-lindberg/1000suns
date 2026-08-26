using System.Collections;
using FMODUnity;
using UnityEngine;

public class TeleportationTrigger : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _spawnPoint;
    [SerializeField] private GameObject _soul;
    [SerializeField] private EventReference _teleportationStartSfx;
    [SerializeField] private EventReference _teleportationEndSfx;
    private bool _isDee = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isDee = PlayerManager.obj.GetPlayerTypeFromCollider(collision) == PlayerManager.PlayerType.SHADOW_TWIN;
            StartCoroutine(TeleportPlayer());
        }
    }

    private IEnumerator TeleportPlayer() {
        Transform playerTransform;
        GameObject playerGameObject;
        if(!_isDee) {
            PlayerMovement.obj.Freeze();
            PlayerMovement.obj.spriteRenderer.enabled = false;
            PlayerMovement.obj.DisableCollider();
            playerTransform = Player.obj.transform;
            playerGameObject = Player.obj.gameObject;
        } else {
            ShadowTwinMovement.obj.Freeze();
            ShadowTwinMovement.obj.spriteRenderer.enabled = false;
            ShadowTwinMovement.obj.DisableCollider();
            playerTransform = ShadowTwinPlayer.obj.transform;
            playerGameObject = ShadowTwinPlayer.obj.gameObject;
        }
        DustParticleMgr.obj.Enabled = false;  //Prevent any dust from spawning

        GameObject soul = Instantiate(_soul, playerTransform.position, playerTransform.rotation);
        
        SoundFXManager.obj.PlayAtGameObject(_teleportationStartSfx, soul);
        PrisonerSoul prisonerSoul = soul.GetComponent<PrisonerSoul>();
        prisonerSoul.Target = _spawnPoint.transform.position;
        while (!prisonerSoul.IsTargetReached) {
            playerTransform.position = prisonerSoul.transform.position;
            yield return null;
        }
        SoundFXManager.obj.PlayAtGameObject(_teleportationEndSfx, playerGameObject);
        Destroy(prisonerSoul.gameObject);

        playerTransform.position = _spawnPoint.transform.position;
        if(!_isDee) {
            PlayerMovement.obj.EnableCollider();
            PlayerMovement.obj.spriteRenderer.enabled = true;
            PlayerMovement.obj.UnFreeze();
        } else {
            ShadowTwinMovement.obj.EnableCollider();
            ShadowTwinMovement.obj.spriteRenderer.enabled = true;
            ShadowTwinMovement.obj.UnFreeze();
        }
        DustParticleMgr.obj.Enabled = true;
        yield return null;
    }
}
