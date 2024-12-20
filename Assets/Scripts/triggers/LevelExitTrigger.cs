using UnityEngine;

public class LevelExitTrigger : MonoBehaviour
{
    [SerializeField] private CaveCollectibleCreature _collectible;
    [SerializeField] private string _id;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            if(_collectible.IsCollected) {
                _collectible.IsPermantentlyCollected = true;
                CollectibleManager.obj.CollectiblePickedPermanently(_id);
            }
        }
    }
}
