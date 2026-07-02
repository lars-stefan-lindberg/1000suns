using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CaveCollectiblePortalTrigger : MonoBehaviour
{
    [SerializeField] private Transform _portal;
    [SerializeField] private Animator _portalAnimator;
    [SerializeField] private CaveCollectibleCreature _collectible;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player")) {
            if(_portal.gameObject.activeSelf) {
                GetComponent<BoxCollider2D>().enabled = false;
                PlayerManager.PlayerType playerType = PlayerManager.obj.GetPlayerTypeFromCollider(collision);
                StartCoroutine(Cutscene(playerType));
            }
        }
    }

    private IEnumerator Cutscene(PlayerManager.PlayerType playerType) {
        PlayerManager.obj.FreezePlayer(playerType);
        yield return new WaitForSeconds(1f);
        _collectible.IsPermanentlyCollected = true;
        yield return new WaitUntil(() => _collectible.IsDespawned);
        
        _portalAnimator.SetTrigger("despawn");
        CollectibleManager.obj.CollectiblePickedPermanently(_collectible);
        SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        PlayerManager.obj.UnfreezePlayer(playerType);
        Destroy(this, 5);
    }
}
