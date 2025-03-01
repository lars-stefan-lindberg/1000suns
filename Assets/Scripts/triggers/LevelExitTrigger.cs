using UnityEngine;

public class LevelExitTrigger : MonoBehaviour
{
    [SerializeField] private CaveCollectibleCreature _collectible;
    [SerializeField] private GameObject _portal;
    private bool _isTriggered = false;

    void Awake()
    {
        //TODO if collectible in this room is already saved, and there's no collectibles following player, disable this game object
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isTriggered)
            return;
        if (other.CompareTag("Player")) {
            //TODO, also consider if there's collectibles from other rooms following player, and activate portal in that case as well
            //TODO, also need to consider the case where player dies/retries the room with collectibles following, and enable the black hole in that case
            if(_collectible != null && _collectible.IsPicked) {
                _isTriggered = true;
                _collectible.SetSaved();
                CollectibleManager.obj.CollectiblePickedPermanently(_collectible);
                if(_portal != null) {
                    _portal.SetActive(true);
                }
            }
        }
    }
}
