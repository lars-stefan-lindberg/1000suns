using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompletedTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.CompareTag("Player")) {
            Scene scene = SceneManager.GetActiveScene();
            LevelManager.obj.SetLevelCompleted(scene.name);
        }
    }
}
