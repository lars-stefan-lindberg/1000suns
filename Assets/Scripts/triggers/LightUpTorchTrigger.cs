using UnityEngine;

public class LightUpTorchTrigger : MonoBehaviour
{
    [SerializeField] private Torch _torch;

    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            _torch.LightUp();
        }
    }
}
