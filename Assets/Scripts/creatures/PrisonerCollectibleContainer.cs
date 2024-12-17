using UnityEngine;

public class PrisonerCollectibleContainer : MonoBehaviour
{
    [SerializeField] private CaveAvatar _caveAvatar;

    [SerializeField] private string id;

    private BoxCollider2D _collider;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            _collider.enabled = false;
            _caveAvatar.IsCollected = true;
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
