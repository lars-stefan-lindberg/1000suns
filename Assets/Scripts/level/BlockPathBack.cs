using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlockPathBack : MonoBehaviour
{
    [SerializeField] private SceneField _scene;
    [SerializeField] private BoxCollider2D _blockPathCollider;

    void Start() {
        Scene activeScene = SceneManager.GetActiveScene();
        if(activeScene.name == _scene.SceneName) {
            _blockPathCollider.enabled = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) {
            //Check if player is exiting to the right of the trigger, only enable solid collider in this case
            if(collision.transform.position.x > transform.position.x) {
                StartCoroutine(EnableCollider());
            }
        }
    }

    IEnumerator EnableCollider() {
        yield return new WaitForSeconds(0.2f);
        _blockPathCollider.enabled = true;
    }
}
