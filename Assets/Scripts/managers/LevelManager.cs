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
    [SerializeField] private float _reloadSceneDelayTime = 0.4f;

    private Dictionary<string, bool> levelCompletionMap = new Dictionary<string, bool>();

    private static readonly string[] LevelSceneNamePrefixes =
    {
        "Forest-",
        "Cave-",
        "Tree-",
        "Ocean-",
        "Underworld-"
    };


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

    public InitRoom GetActiveSceneInitRoomData() {
        Scene activeScene = SceneManager.GetActiveScene();
        return GetInitRoomData(activeScene);
    }

    public InitRoom GetInitRoomData(Scene scene) {
        GameObject initRoomObject = scene.GetRootGameObjects().FirstOrDefault(gameObject => gameObject.CompareTag("InitRoom"));
        if(initRoomObject == null) {
            Debug.LogWarning("Scene " + scene.name + " does not have any InitRoom data.");
            return null;
        }
        return initRoomObject.GetComponent<InitRoom>();
    }

    private IEnumerator LoadSceneDelayedCoroutine(string sceneName) {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        isRunningAfterSceneLoaded = true;
        if(mode == LoadSceneMode.Single) {
            if(scene.name != _titleScreen.SceneName) {
                StartCoroutine(LoadScene(scene));
            }
        }
        isRunningAfterSceneLoaded = false;
    }

    private IEnumerator LoadWalkableSurface(Scene scene) {
        GameObject initRoomObject = scene.GetRootGameObjects().FirstOrDefault(gameObject => gameObject.CompareTag("InitRoom"));
        InitRoom initRoom = initRoomObject?.GetComponent<InitRoom>();
        SceneField walkableSurfaceSceneField = initRoom.walkableSurfaceScene;
        yield return StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(walkableSurfaceSceneField));
    }

    private IEnumerator LoadScene(Scene scene) {
        InitRoom initRoom = GetInitRoomData(scene);
        yield return StartCoroutine(LoadWalkableSurface(scene));
        if(!BackgroundLoaderManager.obj.IsBackgroundLayersLoaded(initRoom.backgroundScene)) {
            yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground(initRoom.backgroundScene));
        }

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
        GameObject playerSpawnPoint;
        playerSpawnPoint = FindSceneSpawnPoint(GameManager.obj.GetCurrentSpawnPointId());
        if (playerSpawnPoint == null)
            playerSpawnPoint = initRoom.defaultSpawnPoint;
        playerSpawnPointCollider = playerSpawnPoint.GetComponent<Collider2D>();
        //Set correct character active
        CaveTimelineId.Id caveTimelineId = GameManager.obj.GetCaveTimeline().GetCaveTimelineId();
        if(caveTimelineId == CaveTimelineId.Id.Eli || caveTimelineId == CaveTimelineId.Id.Both) {
            if(ShadowTwinPlayer.obj != null)
                ShadowTwinPlayer.obj.gameObject.SetActive(false);
            PlayerManager.PlayerType playerType = PlayerManager.obj.GetLastActivePlayerType();
            if(Player.obj != null && playerType == PlayerManager.PlayerType.HUMAN)
                Player.obj.transform.position = playerSpawnPointCollider.transform.position;
            else if(PlayerBlob.obj != null && playerType == PlayerManager.PlayerType.BLOB)
                PlayerBlob.obj.transform.position = playerSpawnPointCollider.transform.position - new Vector3(0, 0.5f, 0);
        } else if(caveTimelineId == CaveTimelineId.Id.Dee) {
            Player.obj.gameObject.SetActive(false);
            PlayerBlob.obj.gameObject.SetActive(true);
            if(ShadowTwinPlayer.obj != null) {
                ShadowTwinPlayer.obj.transform.position = playerSpawnPointCollider.transform.position;
            }
        }
        AdjustSpawnFaceDirection(activeCameraObject.transform.position.x, playerSpawnPoint.transform.position.x);        

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
        PlayerPowersManager.obj.EliCanForcePushJump = false;
        //MothsManager.obj.DestroyMoths();
        
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

        StartCoroutine(LoadAdjacentRoomsPrivate(scene));

        //Load any potential collectibles
        CollectibleManager.obj.MaybeLoadCollectible(scene.name);

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

        //Give parallax bg a moment to settle before fading in
        yield return StartCoroutine(DelayedSceneFadeIn(playerSpawnPointCollider.transform));

        yield return null;
    }

    private GameObject FindSceneSpawnPoint(string spawnPointId) {
        SpawnPoint spawnPoint = FindObjectsOfType<SpawnPoint>().FirstOrDefault(spawnPoint => spawnPoint.SpawnPointID == spawnPointId);
        if(spawnPoint == null) {
            return null;
        }
        return spawnPoint.gameObject;
    }

    public void LoadAdjacentRooms(Scene scene) {
        StartCoroutine(LoadAdjacentRoomsPrivate(scene));
    }

    private IEnumerator LoadAdjacentRoomsPrivate(Scene scene) {
        InitRoom initRoomData = GetInitRoomData(scene);
        List<SceneField> adjacentRooms = initRoomData.adjacentRooms;
        foreach(SceneField room in adjacentRooms) {
            Scene sceneToLoad = SceneManager.GetSceneByName(room.SceneName);
            if(!sceneToLoad.isLoaded) {
                SceneManager.LoadSceneAsync(room.SceneName, LoadSceneMode.Additive);
            }
            CollectibleManager.obj.MaybeLoadCollectible(room.SceneName);
        }
        yield return null;
    }

    public void UnloadNonAdjacentRooms(Scene currentScene) {
        InitRoom initRoomData = GetInitRoomData(currentScene);
        List<SceneField> adjacentRooms = initRoomData.adjacentRooms;

        HashSet<string> excluded = new()
        {
            currentScene.name
        };
        foreach(SceneField sceneField in adjacentRooms) {
            excluded.Add(sceneField.SceneName);
        }

        List<Scene> scenesToUnload = new List<Scene>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!scene.isLoaded)
                continue;

            if (excluded.Contains(scene.name))
                continue;

            if (LevelSceneNamePrefixes.Any(prefix => scene.name.StartsWith(prefix)))
            {
                scenesToUnload.Add(scene);
            }
        }

        foreach (Scene scene in scenesToUnload)
        {
            SceneManager.UnloadSceneAsync(scene);
        }
    }

    private IEnumerator DelayedSceneFadeIn(Transform playerSpawnPoint) {
        yield return new WaitForSeconds(_reloadSceneDelayTime);
        SceneFadeManager.obj.StartFadeIn();
        PlayerStatsManager.obj.ResumeTimer();

        if(PlayerManager.obj.IsSeparated) {
            PlayerManager.obj.PlaySpawn(PlayerManager.PlayerType.HUMAN);
            PlayerManager.obj.PlaySpawn(PlayerManager.PlayerType.SHADOW_TWIN);
        } else {
            PlayerManager.obj.PlaySpawn();
        }
        //Need to play a slightly delayed spawn sound due to when loading a game the sound is broken at the beginning. No idea why!
        StartCoroutine(DelayedSpawnSfx(playerSpawnPoint));
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
                if(Player.obj.GetHasCape() || GameManager.obj.isDevMode) {
                    Player.obj.SetAnimatorLayerAndHasCape(true);
                } else {
                    Player.obj.SetAnimatorLayerAndHasCape(false);
                }
            }
        }

        if(ShadowTwinMovement.obj != null) {
            ShadowTwinMovement.obj.SetStartingOnGround();
            ShadowTwinMovement.obj.isGrounded = true;
            ShadowTwinMovement.obj.isTransforming = false;
            ShadowTwinMovement.obj.CancelJumping();
            ShadowTwinPlayer.obj.FadeInPlayerLight();

            if(ShadowTwinPlayer.obj != null) {
                if(ShadowTwinPlayer.obj.GetHasCrown() || GameManager.obj.isDevMode) {
                    ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(true);
                } else {
                    ShadowTwinPlayer.obj.SetAnimatorLayerAndHasCrown(false);
                }
            }
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
        GameEventId sootFreed = ScriptableObject.CreateInstance<GameEventId>();
        sootFreed.id = "cave-3.soot-freed";
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
            if(GameManager.obj.C30CutsceneCompleted)
                CaveAvatar.obj.SetStartingPositionInRoom31();
            else
                CaveAvatar.obj.SetStartingPositionInRoom30();
        } else if(GameManager.obj.C27CutsceneCompleted) {
            CaveAvatar.obj.SetStartingPositionInRoom30();
        } else if(GameManager.obj.C26CutsceneCompleted) {
            CaveAvatar.obj.SetStartingPositionInRoom27();
        } else if(!GameManager.obj.Progress.HasEvent(sootFreed) && !GameManager.obj.isDevMode) {
            CaveAvatar.obj.SetStartingPositionInRoom1();
        } else {
            CaveAvatar.obj.SetFollowPlayerStartingPosition();
            CaveAvatar.obj.FollowPlayer();
        }
        Destroy(sootFreed);
    }

    void OnDestroy()
    {
        obj = null;
    }
}
