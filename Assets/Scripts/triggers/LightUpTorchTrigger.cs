using UnityEngine;

public class LightUpTorchTrigger : MonoBehaviour
{
    [SerializeField] private Torch _torch;
    private BoxCollider2D _collider;

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            _torch.LightUp();
            _collider.enabled = false;
        }
    }
}
