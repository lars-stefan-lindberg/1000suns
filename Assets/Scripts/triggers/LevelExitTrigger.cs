using UnityEngine;

public class LevelExitTrigger : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
    if (other.CompareTag("Player"))
        CollectibleManager.Instance.CollectiblePickedPermanent();
    }
}
