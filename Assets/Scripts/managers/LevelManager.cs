using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager obj;
    public bool isRunningAfterSceneLoaded = false;
    [SerializeField] private SceneField _titleScreen;

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
        isRunningAfterSceneLoaded = true;
        if(mode == LoadSceneMode.Single) {
            if(scene.name != _titleScreen.SceneName) {
                GameObject[] sceneGameObjects = scene.GetRootGameObjects();
                GameObject playerSpawnPoint = sceneGameObjects.First(gameObject => gameObject.CompareTag("PlayerSpawnPoint"));
                _playerSpawningCollider = playerSpawnPoint.GetComponent<Collider2D>();
                
                //If we have multiple cameras in room, activate the first/default one when loading the room. This will also
                //do an early enough reset of any parallax backgrounds.
                GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
                CameraManager cameraManager = cameras.GetComponent<CameraManager>();
                cameraManager.ActivateMainCamera(PlayerMovement.PlayerDirection.NO_DIRECTION);

                Player.obj.transform.position = _playerSpawningCollider.transform.position;
                AdjustSpawnFaceDirection(Camera.main.transform.position.x, playerSpawnPoint.transform.position.x);
                Player.obj.SetHasPowerUp(false);
                Player.obj.gameObject.SetActive(true);
                PlayerMovement.obj.SetStartingOnGround();
                PlayerMovement.obj.isGrounded = true;
                PlayerMovement.obj.isForcePushJumping = false;
                PlayerMovement.obj.jumpedWhileForcePushJumping = false;
                PlayerMovement.obj.CancelJumping();

                Reaper.obj.playerKilled = false;
                if(Player.obj.hasCape)
                    Player.obj.SetHasCape(true);
                Player.obj.PlaySpawn();
                
                IEnumerable<GameObject> levelSwitchers = sceneGameObjects.Where(gameObject => gameObject.CompareTag("LevelSwitcher"));
                foreach(GameObject levelSwitcherGameObject in levelSwitchers) {
                    LevelSwitcher levelSwitcher = levelSwitcherGameObject.GetComponent<LevelSwitcher>();
                    levelSwitcher.LoadNextRoom();
                }
            }
            SceneFadeManager.obj.StartFadeIn();
        }
        isRunningAfterSceneLoaded = false;
    }

    private void AdjustSpawnFaceDirection(float sceneLoadTriggerPosition, float playerSpawnPointPosition) {
        bool isLeftSideOfScreen = sceneLoadTriggerPosition - playerSpawnPointPosition >= 0;
            if(isLeftSideOfScreen && PlayerMovement.obj.isFacingLeft())
                PlayerMovement.obj.FlipPlayer();
            else if(!isLeftSideOfScreen && !PlayerMovement.obj.isFacingLeft())
                PlayerMovement.obj.FlipPlayer();
    }
}
