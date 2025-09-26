using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompletedTrigger : MonoBehaviour
{
    private BoxCollider2D _collider;
    
    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
    }
    
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            Scene scene = SceneManager.GetActiveScene();
            LevelManager.obj.SetLevelCompleted(scene.name);
            SaveManager.obj.SaveGame(scene.name);
            _collider.enabled = false;
        }
    }
}
