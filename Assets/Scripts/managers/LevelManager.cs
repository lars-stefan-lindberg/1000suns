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

    private Dictionary<string, bool> levelCompletionMap = new Dictionary<string, bool>();


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

                GameObject playerSpawnPoint;
                if(IsLevelCompleted(scene.name)) {
                    Debug.Log("Level has been completed. Scene: " + scene.name + ". Loading alternative spawn point.");
                    playerSpawnPoint = sceneGameObjects.First(gameObject => gameObject.CompareTag("AlternatePlayerSpawnPoint"));
                } else {
                    playerSpawnPoint = sceneGameObjects.First(gameObject => gameObject.CompareTag("PlayerSpawnPoint"));
                }
                _playerSpawningCollider = playerSpawnPoint.GetComponent<Collider2D>();
                
                //If we have multiple cameras in room, activate the first/default one when loading the room. This will also
                //do an early enough reset of any parallax backgrounds.
                GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
                CameraManager cameraManager = cameras.GetComponent<CameraManager>();
                cameraManager.ActivateMainCamera(PlayerMovement.PlayerDirection.NO_DIRECTION);

                Player.obj.transform.position = _playerSpawningCollider.transform.position;
                AdjustSpawnFaceDirection(Camera.main.transform.position.x, playerSpawnPoint.transform.position.x);

                CaveAvatar.obj.SetStartingPosition();
                //If there are collectibles following, set start positions for them
                if(CollectibleManager.obj.GetNumberOfCreaturesFollowingPlayer() > 0) {
                    CaveCollectibleCreature previousCollectible = null;
                    foreach(CaveCollectibleCreature collectible in CollectibleManager.obj.GetFollowingCaveCollectibleCreatures()) {
                        if(previousCollectible == null) {
                            collectible.SetStartingPosition(CaveAvatar.obj.GetHeadTransform().position);
                        } else {
                            collectible.SetStartingPosition(previousCollectible.GetHeadTransform().position);
                        }
                        previousCollectible = collectible;
                    }
                }

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
                SoundFXManager.obj.PlayPlayerShadowSpawn(Player.obj.transform);
                
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

    private bool IsLevelCompleted(string levelId)
    {
        if (levelCompletionMap.TryGetValue(levelId, out bool isCompleted))
        {
            return isCompleted;
        }
        else
        {
            //Debug.LogWarning($"Level {levelId} not found!");
            return false;
        }
    }

    public void SetLevelCompleted(string levelId)
    {
        if (levelCompletionMap.ContainsKey(levelId))
        {
            levelCompletionMap[levelId] = true;
        }
        else
        {
            Debug.LogWarning($"Level {levelId} not found! Adding it as completed.");
            levelCompletionMap[levelId] = true; // Adds the level if not found
        }
    }

    public void ResetLevels() {
        levelCompletionMap.Clear();
    }
}
