using UnityEngine;

public class CaveCollectiblePickedHandler : MonoBehaviour
{
    [SerializeField] private AudioClip _pickupSound;

    private CaveCollectibleCreature _collectible;

    void Start()
    {
        _collectible = GetComponent<CaveCollectibleCreature>();
        _collectible.OnPicked += PlayPickupSound;
        _collectible.OnPermanentlyCollected += PlayPickupSound;
    }

    private void PlayPickupSound() {
        SoundFXManager.obj.PlaySound(_pickupSound, transform, 1f);
    }

    private void OnDestroy() {
        if(_collectible != null) {
            _collectible.OnPicked -= PlayPickupSound;
            _collectible.OnPermanentlyCollected -= PlayPickupSound;
        }
    }
}
