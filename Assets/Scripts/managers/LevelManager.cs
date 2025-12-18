using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using FunkyCode;

public class LevelManager : MonoBehaviour
{
    public static LevelManager obj;
    public bool isRunningAfterSceneLoaded = false;
    [SerializeField] private SceneField _titleScreen;

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

    public void LoadSceneDelayed(string sceneName) {
        StartCoroutine(LoadSceneDelayedCoroutine(sceneName));
    }

    private IEnumerator LoadSceneDelayedCoroutine(string sceneName) {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        isRunningAfterSceneLoaded = true;
        if(mode == LoadSceneMode.Single) {
            if(scene.name != _titleScreen.SceneName) {
                GameObject[] sceneGameObjects = scene.GetRootGameObjects();
                
                //If we have multiple cameras in room, activate the first/default one when loading the room. This will also
                //do an early enough reset of any parallax backgrounds.
                GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
                CameraManager cameraManager = cameras.GetComponent<CameraManager>();
                GameObject activeCameraObject;
                if(IsLevelCompleted(scene.name)) {
                    activeCameraObject = cameraManager.ActivateAlternativeCamera();
                } else {
                    activeCameraObject = cameraManager.ActivateMainCamera();
                }

                if(SaveManager.obj.RestoreBlobOnNextScene) {
                    SaveManager.obj.ConsumeRestoreBlobFlag();
                    PlayerMovement.obj.ToBlob();
                    PlayerManager.obj.SetLastActivePlayerType(PlayerManager.PlayerType.BLOB);
                    PlayerSwitcher.obj.SwitchToBlob();
                }
                
                Collider2D playerSpawnPointCollider;
                if(PlayerManager.obj.IsSeparated) {
                    GameObject eliSpawnPoint = GetPlayerSpawnPoint(PlayerManager.PlayerType.HUMAN, sceneGameObjects);
                    Collider2D eliSpawningCollider = eliSpawnPoint.GetComponent<Collider2D>();
                    playerSpawnPointCollider = eliSpawningCollider;
                    if(Player.obj != null)
                        Player.obj.transform.position = eliSpawningCollider.transform.position;
                    else if(PlayerBlob.obj != null) {
                        PlayerBlob.obj.transform.position = eliSpawningCollider.transform.position - new Vector3(0, 0.5f, 0);;
                    }
                    GameObject shadowTwinSpawnPoint = GetPlayerSpawnPoint(PlayerManager.PlayerType.SHADOW_TWIN, sceneGameObjects);
                    Collider2D shadowTwinSpawningCollider = shadowTwinSpawnPoint.GetComponent<Collider2D>();
                    if(ShadowTwinPlayer.obj != null)
                        ShadowTwinPlayer.obj.transform.position = shadowTwinSpawningCollider.transform.position;

                    AdjustSpawnFaceDirectionIsSeparated(activeCameraObject.transform.position.x, eliSpawningCollider.transform.position.x, PlayerManager.PlayerType.HUMAN);
                    AdjustSpawnFaceDirectionIsSeparated(activeCameraObject.transform.position.x, shadowTwinSpawningCollider.transform.position.x, PlayerManager.PlayerType.SHADOW_TWIN);
                } else {
                    GameObject playerSpawnPoint;
                    if(IsLevelCompleted(scene.name)) {
                        Debug.Log("Level has been completed. Scene: " + scene.name + ". Loading alternative spawn point.");
                        playerSpawnPoint = sceneGameObjects.First(gameObject => gameObject.CompareTag("AlternatePlayerSpawnPoint"));
                    } else {
                        playerSpawnPoint = sceneGameObjects.First(gameObject => gameObject.CompareTag("PlayerSpawnPoint"));
                    }
                    playerSpawnPointCollider = playerSpawnPoint.GetComponent<Collider2D>();
                    if(Player.obj != null)
                        Player.obj.transform.position = playerSpawnPointCollider.transform.position;
                    if(ShadowTwinPlayer.obj != null) {
                        ShadowTwinPlayer.obj.transform.position = playerSpawnPointCollider.transform.position;
                    }
                    if(PlayerBlob.obj != null)
                        PlayerBlob.obj.transform.position = playerSpawnPointCollider.transform.position - new Vector3(0, 0.5f, 0);
                    AdjustSpawnFaceDirection(activeCameraObject.transform.position.x, playerSpawnPoint.transform.position.x);
                }

                if(CaveAvatar.obj != null && CaveAvatar.obj.gameObject.activeSelf) {
                    SetCaveAvatarPosition(scene);
                }

                if(SaveManager.obj != null && SaveManager.obj.RestoreFollowingCreaturesOnNextScene) {
                    var data = SaveManager.obj.LastLoadedSaveData;
                    if (data != null) {
                        CollectibleManager.obj.ImportFollowingCollectibles(data.followingCollectibles);
                    }
                    SaveManager.obj.ConsumeRestoreFollowingCreaturesFlag();
                }
                //If there are collectibles following, set start positions for them
                if(CollectibleManager.obj.GetNumberOfCreaturesFollowingPlayer() > 0) {
                    CaveCollectibleCreature previousCollectible = null;
                    foreach(CaveCollectibleCreature collectible in CollectibleManager.obj.GetFollowingCaveCollectibleCreatures()) {
                        if(previousCollectible == null) {
                            collectible.SetStartingPosition(CaveAvatar.obj.GetHeadTransform().position);
                            collectible.SetTarget(CaveAvatar.obj.GetHeadTransform());
                        } else {
                            collectible.SetStartingPosition(previousCollectible.GetHeadTransform().position);
                            collectible.SetTarget(previousCollectible.GetHeadTransform());
                        }
                        previousCollectible = collectible;
                    }
                }

                if(Player.obj != null)
                    Player.obj.SetHasPowerUp(false);
                PlayerPowersManager.obj.CanForcePushJump = false;
                MothsManager.obj.DestroyMoths();
                
                if(PlayerManager.obj.IsSeparated) {
                    PlayerManager.obj.EnableAllPlayers();
                    SetPlayersStartingState();
                    if(PlayerManager.obj.IsCoopActive) {
                        LobbyManager.obj.SetPlayerInputs();
                    }
                } else {
                    PlayerManager.obj.EnableLastActivePlayerGameObject();
                    SetPlayersStartingState();
                }

                Reaper.obj.playerKilled = false;

                if(PlayerManager.obj.IsSeparated) {
                    PlayerManager.obj.PlaySpawn(PlayerManager.PlayerType.HUMAN);
                    PlayerManager.obj.PlaySpawn(PlayerManager.PlayerType.SHADOW_TWIN);
                } else {
                    PlayerManager.obj.PlaySpawn();
                }
                //Need to play a slightly delayed spawn sound due to when loading a game the sound is broken at the beginning. No idea why!
                StartCoroutine(DelayedSpawnSfx(playerSpawnPointCollider.transform));
                
                IEnumerable<GameObject> levelSwitchers = sceneGameObjects.Where(gameObject => gameObject.CompareTag("LevelSwitcher"));
                foreach(GameObject levelSwitcherGameObject in levelSwitchers) {
                    LevelSwitcher levelSwitcher = levelSwitcherGameObject.GetComponent<LevelSwitcher>();
                    levelSwitcher.LoadNextRoom();
                }
            }
            

            // If we just loaded a game from a save, restore global lighting darkness color
            if (SaveManager.obj != null && SaveManager.obj.RestoreLightingOnNextScene) {
                var data = SaveManager.obj.LastLoadedSaveData;
                if (data != null && !string.IsNullOrEmpty(data.darknessColorHex) && LightingManager2D.Get() != null && LightingManager2D.Get().profile != null) {
                    if (ColorUtility.TryParseHtmlString(data.darknessColorHex, out var parsed)) {
                        LightingManager2D.Get().profile.DarknessColor = parsed;
                    }
                }
                SaveManager.obj.ConsumeRestoreLightingFlag();
            }

            //Load any potential collectibles
            CollectibleManager.obj.MaybeLoadCollectible(scene.name);

            SceneFadeManager.obj.StartFadeIn();
            PlayerStatsManager.obj.ResumeTimer();

            // If we just loaded a game from a save, restore audio state (music + ambience)
            if (SaveManager.obj != null && SaveManager.obj.RestoreAudioOnNextScene) {
                var data = SaveManager.obj.LastLoadedSaveData;
                if (data != null) {
                    if (!string.IsNullOrEmpty(data.currentMusicId) && MusicManager.obj != null) {
                        MusicManager.obj.PlayById(data.currentMusicId);
                    }
                    if (!string.IsNullOrEmpty(data.currentAmbienceId) && AmbienceManager.obj != null) {
                        AmbienceManager.obj.PlayById(data.currentAmbienceId);
                    }
                }
                SaveManager.obj.ConsumeRestoreAudioFlag();
            }
        }
        isRunningAfterSceneLoaded = false;
    }

    private void SetPlayersStartingState() {
        if(PlayerBlobMovement.obj != null) {
            PlayerBlobMovement.obj.SetStartingOnGround();
            PlayerBlobMovement.obj.isGrounded = true;
            PlayerBlobMovement.obj.CancelJumping();
            PlayerBlob.obj.FadeInPlayerLight();
        }

        if(PlayerMovement.obj != null) {
            PlayerMovement.obj.SetStartingOnGround();
            PlayerMovement.obj.isGrounded = true;
            PlayerMovement.obj.isForcePushJumping = false;
            PlayerMovement.obj.jumpedWhileForcePushJumping = false;
            PlayerMovement.obj.isTransformingToBlob = false;
            PlayerMovement.obj.isTransformingToTwin = false;
            PlayerMovement.obj.CancelJumping();
            Player.obj.FadeInPlayerLight();

            if(Player.obj != null) {
                if(Player.obj.hasCape || PlayerMovement.obj.isDevMode) {
                    Player.obj.SetHasCape(true);
                } else {
                    Player.obj.SetHasCape(false);
                }
            }
        }

        if(ShadowTwinMovement.obj != null) {
            ShadowTwinMovement.obj.SetStartingOnGround();
            ShadowTwinMovement.obj.isGrounded = true;
            ShadowTwinMovement.obj.isTransforming = false;
            ShadowTwinMovement.obj.CancelJumping();
            ShadowTwinPlayer.obj.FadeInPlayerLight();
        }
    }

    public void SpawnPlayer(PlayerManager.PlayerType playerType) {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] sceneGameObjects = scene.GetRootGameObjects();

        GameObject playerSpawnPoint;
        if(IsLevelCompleted(scene.name)) {
            Debug.Log("Level has been completed. Scene: " + scene.name + ". Loading alternative spawn point.");
            playerSpawnPoint = sceneGameObjects.First(gameObject => gameObject.CompareTag("AlternatePlayerSpawnPoint"));
        } else {
            playerSpawnPoint = GetPlayerSpawnPoint(playerType, sceneGameObjects);
        }
        Collider2D playerSpawningCollider = playerSpawnPoint.GetComponent<Collider2D>();

        GameObject cameras = sceneGameObjects.First(gameObject => gameObject.CompareTag("Cameras"));
        CameraManager cameraManager = cameras.GetComponent<CameraManager>();
        GameObject activeCameraObject = cameraManager.ActivateMainCamera();
        
        if(playerType == PlayerManager.PlayerType.HUMAN) {
            if(Player.obj != null)
                Player.obj.transform.position = playerSpawningCollider.transform.position;
            if(PlayerBlob.obj != null)
                PlayerBlob.obj.transform.position = playerSpawningCollider.transform.position - new Vector3(0, 0.5f, 0);
        } else if(playerType == PlayerManager.PlayerType.SHADOW_TWIN) {
            ShadowTwinPlayer.obj.transform.position = playerSpawningCollider.transform.position;
        }
        AdjustSpawnFaceDirection(activeCameraObject.transform.position.x, playerSpawnPoint.transform.position.x);
        
        PlayerManager.obj.EnableLastActivePlayerGameObject();
        
        if(playerType == PlayerManager.PlayerType.HUMAN) {
            if(PlayerBlobMovement.obj != null) {
                PlayerBlobMovement.obj.SetStartingOnGround();
                PlayerBlobMovement.obj.isGrounded = true;
                PlayerBlobMovement.obj.CancelJumping();
                PlayerBlob.obj.FadeInPlayerLight();
            }

            if(PlayerMovement.obj != null) {
                PlayerMovement.obj.SetStartingOnGround();
                PlayerMovement.obj.isGrounded = true;
                PlayerMovement.obj.isForcePushJumping = false;
                PlayerMovement.obj.jumpedWhileForcePushJumping = false;
                PlayerMovement.obj.isTransformingToBlob = false;
                PlayerMovement.obj.isTransformingToTwin = false;
                PlayerMovement.obj.CancelJumping();
                Player.obj.FadeInPlayerLight();
            }
        } else if(playerType == PlayerManager.PlayerType.SHADOW_TWIN) {
            if(ShadowTwinMovement.obj != null) {
                ShadowTwinMovement.obj.SetStartingOnGround();
                ShadowTwinMovement.obj.isGrounded = true;
                ShadowTwinMovement.obj.isTransforming = false;
                ShadowTwinMovement.obj.CancelJumping();
                ShadowTwinPlayer.obj.FadeInPlayerLight();
            }
        }

        PlayerManager.obj.PlaySpawn(playerType);
        //Need to play a slightly delayed spawn sound due to when loading a game the sound is broken at the beginning. No idea why!
        StartCoroutine(DelayedSpawnSfx(playerSpawningCollider.transform));
    }   

    private GameObject GetPlayerSpawnPoint(PlayerManager.PlayerType playerType, GameObject[] sceneGameObjects) {
        if(playerType == PlayerManager.PlayerType.HUMAN)
         return sceneGameObjects.First(gameObject => gameObject.CompareTag("PlayerSpawnPoint") && !gameObject.GetComponent<PlayerSpawnPointManager>().IsSecondPlayerSpawnPoint);
        else if(playerType == PlayerManager.PlayerType.SHADOW_TWIN)
         return sceneGameObjects.First(gameObject => gameObject.CompareTag("PlayerSpawnPoint") && gameObject.GetComponent<PlayerSpawnPointManager>().IsSecondPlayerSpawnPoint);
        else {
            Debug.Log("Unknown player type: " + playerType);
            return null;
        }
    }

    private IEnumerator DelayedSpawnSfx(Transform transform) {
        yield return new WaitForSeconds(0.01f);
        SoundFXManager.obj.PlayPlayerShadowSpawn(transform);
    }

    private void AdjustSpawnFaceDirection(float sceneLoadTriggerPosition, float playerSpawnPointPosition) {
        bool isLeftSideOfScreen = sceneLoadTriggerPosition - playerSpawnPointPosition >= 0;
        if(isLeftSideOfScreen && PlayerManager.obj.IsPlayerFacingLeft())
            PlayerManager.obj.FlipPlayer();
        else if(!isLeftSideOfScreen && !PlayerManager.obj.IsPlayerFacingLeft())
            PlayerManager.obj.FlipPlayer();
    }

    private void AdjustSpawnFaceDirectionIsSeparated(float sceneLoadTriggerPosition, float playerSpawnPointPosition, PlayerManager.PlayerType playerType) {
        bool isLeftSideOfScreen = sceneLoadTriggerPosition - playerSpawnPointPosition >= 0;
        if(isLeftSideOfScreen && PlayerManager.obj.IsPlayerFacingLeft(playerType))
            PlayerManager.obj.FlipPlayer(playerType);
        else if(!isLeftSideOfScreen && !PlayerManager.obj.IsPlayerFacingLeft(playerType))
            PlayerManager.obj.FlipPlayer(playerType);
    }

    public bool IsLevelCompleted(string levelId)
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

    // Export/import helpers for persistence
    // Returns a list of level ids that are marked as completed
    public List<string> ExportCompletedLevels()
    {
        // Only include entries explicitly marked true
        return levelCompletionMap.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
    }

    // Rebuilds the completion map from a list of completed level ids
    public void ImportCompletedLevels(List<string> completedLevels)
    {
        levelCompletionMap.Clear();
        if (completedLevels == null) return;
        foreach (var id in completedLevels)
        {
            if (!string.IsNullOrEmpty(id))
            {
                levelCompletionMap[id] = true;
            }
        }
    }

    private void SetCaveAvatarPosition(Scene scene) {
        if (scene.name == "C35") {
            CaveAvatar.obj.SetStartingPositionInRoom35();
        } else if(scene.name == "C34") {
            if(IsLevelCompleted("C34"))
                CaveAvatar.obj.SetStartingPositionInRoom35();
            else
                CaveAvatar.obj.SetStartingPositionInRoom34();
        } else if(scene.name == "C33") {
            if(IsLevelCompleted("C33"))
                CaveAvatar.obj.SetStartingPositionInRoom34();
            else
                CaveAvatar.obj.SetStartingPositionInRoom33();
        } else if(scene.name == "C32") {
            if(IsLevelCompleted("C32"))
                CaveAvatar.obj.SetStartingPositionInRoom33();
            else
                CaveAvatar.obj.SetStartingPositionInRoom32();
        } else if(scene.name == "C31") {
            if(IsLevelCompleted("C31"))
                CaveAvatar.obj.SetStartingPositionInRoom32();
            else
                CaveAvatar.obj.SetStartingPositionInRoom31();
        } else if(scene.name == "C30") {
            if(GameEventManager.obj.C30CutsceneCompleted)
                CaveAvatar.obj.SetStartingPositionInRoom31();
            else
                CaveAvatar.obj.SetStartingPositionInRoom30();
        } else if(GameEventManager.obj.C27CutsceneCompleted) {
            CaveAvatar.obj.SetStartingPositionInRoom30();
        } else if(GameEventManager.obj.C26CutsceneCompleted) {
            CaveAvatar.obj.SetStartingPositionInRoom27();
        } else if(!GameEventManager.obj.CaveAvatarFreed && !PlayerMovement.obj.isDevMode) {
            CaveAvatar.obj.SetStartingPositionInRoom1();
        } else {
            CaveAvatar.obj.SetFollowPlayerStartingPosition();
            CaveAvatar.obj.FollowPlayer();
        }
    }
}
