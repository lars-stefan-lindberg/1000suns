using System.Collections;
using System.Collections.Generic;
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
            StartCoroutine(EnableCollider());
        }
    }

    IEnumerator EnableCollider() {
        yield return new WaitForSeconds(0.2f);
        _blockPathCollider.enabled = true;
    }
}
