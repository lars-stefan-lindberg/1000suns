using UnityEngine;

public class FadeOutTilemap : MonoBehaviour
{
    [SerializeField] private Animator _visibleLayerAnimator;
    private BoxCollider2D _collider;

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            _visibleLayerAnimator.SetTrigger("reveal");
            _collider.enabled = false;
            Destroy(gameObject, 5);
        }
    }
}
