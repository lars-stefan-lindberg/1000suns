using System.Collections;
using UnityEngine;

public class CaveCollectiblePortalTrigger : MonoBehaviour
{
    private bool _isTriggered = false;
    [SerializeField] private Transform _portal;
    [SerializeField] private Animator _portalAnimator;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player")) {
            if(_portal.gameObject.activeSelf)
                StartCoroutine(Cutscene());
        }
    }

    private IEnumerator Cutscene() {
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(1f);
        var collectibles = CollectibleManager.obj.GetFollowingCaveCollectibleCreatures();
        foreach(var collectible in collectibles) {
            collectible.IsPermantentlyCollected = true;
            collectible.portal = _portal;
            yield return new WaitUntil(() => collectible.IsDespawned);
        }
        
        _portalAnimator.SetTrigger("despawn");
        PlayerMovement.obj.UnFreeze();
        CollectibleManager.obj.ClearFollowingCaveCollectibles();
    }
}
