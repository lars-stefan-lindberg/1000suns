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
            
            GameObject sceneLoadTriggerGameObject = sceneGameObjects.First(gameObject => gameObject.CompareTag("SceneLoadTrigger"));
            SceneLoadTrigger sceneLoadTrigger = sceneLoadTriggerGameObject.GetComponent<SceneLoadTrigger>();

            Player.obj.transform.position = _playerSpawningCollider.transform.position;
            AdjustSpawnFaceDirection(sceneLoadTrigger.transform.position.x, playerSpawnPoint.transform.position.x);
            Player.obj.hasPowerUp = false;
            Player.obj.gameObject.SetActive(true);
            Player.obj.PlaySpawn();
            if(Player.obj.hasCape)
                Player.obj.SetHasCape();
            PlayerMovement.obj.SetStartingOnGround();
            PlayerMovement.obj.isGrounded = true;
            PlayerMovement.obj.isForcePushJumping = false;
            PlayerMovement.obj.jumpedWhileForcePushJumping = false;


            sceneLoadTrigger.LoadScenes();
        }
    }

    private void AdjustSpawnFaceDirection(float sceneLoadTriggerPosition, float playerSpawnPointPosition) {
        bool isLeftSideOfScreen = sceneLoadTriggerPosition - playerSpawnPointPosition >= 0;
            if(isLeftSideOfScreen && PlayerMovement.obj.isFacingLeft())
                PlayerMovement.obj.FlipPlayer();
            else if(!isLeftSideOfScreen && !PlayerMovement.obj.isFacingLeft())
                PlayerMovement.obj.FlipPlayer();
    }
}
