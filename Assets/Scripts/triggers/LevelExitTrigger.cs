using UnityEngine;

public class LevelExitTrigger : MonoBehaviour
{
    [SerializeField] private CaveCollectibleCreature _collectible;
    [SerializeField] private GameObject _portal;

    void Awake()
    {
        if(_collectible == null && CollectibleManager.obj.GetNumberOfCreaturesFollowingPlayer() == 0)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            if(_collectible != null && _collectible.IsPicked) {
                _collectible.SetSaved();
                CollectibleManager.obj.CollectiblePickedPermanently(_collectible);
                if(_portal != null) {
                    _portal.SetActive(true);
                    foreach (Transform child in _portal.transform) {
                        child.gameObject.SetActive(true);
                    }
                    _portal.GetComponentInChildren<BlackHole>().FadeInLight();
                }
            } else if(CollectibleManager.obj.GetNumberOfCreaturesFollowingPlayer() > 0) {
                if(_portal != null) {
                    _portal.SetActive(true);
                    foreach (Transform child in _portal.transform) {
                        child.gameObject.SetActive(true);
                    }
                    _portal.GetComponentInChildren<BlackHole>().FadeInLight();
                }
            }
        }
    }
}
