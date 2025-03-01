using System.Collections;
using UnityEngine;

public class CaveCollectiblePortalTrigger : MonoBehaviour
{
    private bool _isTriggered = false;
    [SerializeField] private Transform _portal;
    [SerializeField] private Animator _portalAnimator;

    void Awake()
    {
        //TODO if room is completed, destroy self
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(_isTriggered)
            return;
        if(collision.gameObject.CompareTag("Player")) {
            _isTriggered = true;
            StartCoroutine(Cutscene());
        }
    }

    private IEnumerator Cutscene() {
        PlayerMovement.obj.Freeze();
        yield return new WaitForSeconds(1f);
        var collectibles = CollectibleManager.obj.GetFollowingCaveCollectibleCreatures();
        foreach(var collectible in collectibles) {
            collectible.portal = _portal;
            collectible.IsPermantentlyCollected = true;
            yield return new WaitUntil(() => collectible.IsDespawned);
        }
        
        _portalAnimator.SetTrigger("despawn");
        PlayerMovement.obj.UnFreeze();
        CollectibleManager.obj.ClearFollowingCaveCollectibles();
    }
}
