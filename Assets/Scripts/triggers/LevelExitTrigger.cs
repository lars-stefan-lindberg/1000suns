using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExitTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _portal;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            bool updated = CollectibleManager.obj.SaveFollowingCollectibles();
            if(updated) {
                SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
            }
            if(CollectibleManager.obj.GetNumberOfCreaturesFollowingPlayer() > 0) {
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
