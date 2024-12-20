using UnityEngine;

public class CaveCollectibleCreatureContainer : MonoBehaviour
{
    [SerializeField] private CaveCollectibleCreature _collectible;

    [SerializeField] private string id;

    private BoxCollider2D _collider;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            _collider.enabled = false;
            _collectible.IsCollected = true;
        }
    }

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
        if(CollectibleManager.obj.IsCollectiblePicked(id)) {
            gameObject.SetActive(true);
            Destroy(gameObject);
        }
    }
}
