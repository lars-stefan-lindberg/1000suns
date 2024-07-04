using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager obj;

    private Collider2D _playerSpawningCollider;

    void Awake() {
        obj = this;
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ReloadCurrentScene() {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if(mode == LoadSceneMode.Single) {
            SceneFadeManager.obj.StartFadeIn();

            GameObject[] sceneGameObjects = scene.GetRootGameObjects();
            GameObject playerSpawnPoint = sceneGameObjects.First(gameObject => gameObject.CompareTag("PlayerSpawnPoint"));
            _playerSpawningCollider = playerSpawnPoint.GetComponent<Collider2D>();

            Player.obj.transform.position = _playerSpawningCollider.transform.position;
            Player.obj.gameObject.SetActive(true);
            Player.obj.PlaySpawn();
            PlayerMovement.obj.SetStartingOnGround();

            GameObject sceneLoadTriggerGameObject = sceneGameObjects.First(gameObject => gameObject.CompareTag("SceneLoadTrigger"));
            SceneLoadTrigger sceneLoadTrigger = sceneLoadTriggerGameObject.GetComponent<SceneLoadTrigger>();
            sceneLoadTrigger.LoadScenes();
        }
    }
}
