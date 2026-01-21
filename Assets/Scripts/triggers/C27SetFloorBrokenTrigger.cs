using UnityEngine;
using UnityEngine.SceneManagement;

public class C27SetFloorBrokenTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) {
            GameManager.obj.C275FloorBroken = true;  
            SaveManager.obj.SaveGame(SceneManager.GetActiveScene().name);
        }
    }
}
