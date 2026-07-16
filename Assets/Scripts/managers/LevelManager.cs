using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FunkyCode;

public class LevelManager : MonoBehaviour
{
    public static LevelManager obj;
    public bool isRunningAfterSceneLoaded = false;
    [SerializeField] private SceneField _titleScreen;
    [SerializeField] private float _reloadSceneDelayTime = 0.4f;
    private SharedCharacterAudio _sharedCharacterAudio;

    private Dictionary<string, bool> levelCompletionMap = new Dictionary<string, bool>();

    private static readonly string[] LevelSceneNamePrefixes =
    {
        "Forest-",
        "Cave-",
        "Tree-",
        "Ocean-",
        "Underworld-"
    };

    // Event ID constants for cave avatar positioning
    private const string SOOT_FREED_EVENT = "cave-3.soot-freed";
    private const string BEFORE_SHADOW_JUMP_EVENT = "Cave-33.first-eli-soot-conversation-completed";
    private const string AFTER_SHADOW_JUMP_EVENT = "Cave-33.after-shadow-jump-conversation-completed";
    private const string SOOT_BETRAYAL_EVENT = "Cave-47.cutscene-completed";
    private const string CAVE_52_CONVERSATION_EVENT = "Cave-52.cutscene-completed";
    private const string CAVE_3_DEE_CONVERSATION_EVENT = "Cave-3.dee.cutscene-completed";


    void Awake() {
        obj = this;
        _sharedCharacterAudio = GetComponent<SharedCharacterAudio>();
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
        GameObject[] rootObjects = scene.GetRootGameObjects();
        GameObject initRoomObject = null;
        foreach (var go in rootObjects) {
            if (go.CompareTag("InitRoom")) {
                initRoomObject = go;
                break;
            }
        }
        if(initRoomObject == null) {
            Debug.LogWarning("Scene " + scene.name + " does not have any InitRoom data.");
            return null;
        }
        return initRoomObject.GetComponent<InitRoom>();
    }

    private InitRoom GetInitRoomData(Scene scene, GameObject[] rootObjects) {
        GameObject initRoomObject = null;
        foreach (var go in rootObjects) {
            if (go.CompareTag("InitRoom")) {
                initRoomObject = go;
                break;
            }
        }
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

    private IEnumerator LoadWalkableSurface(InitRoom initRoom) {       
        SceneField walkableSurfaceSceneField = initRoom.walkableSurfaceScene;
        yield return StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(walkableSurfaceSceneField));
    }

    private IEnumerator LoadScene(Scene scene) {
        GameObject[] sceneRootObjects = scene.GetRootGameObjects();
        InitRoom initRoom = GetInitRoomData(scene, sceneRootObjects);
        yield return StartCoroutine(LoadWalkableSurface(initRoom));
        
        if(AudioStateManager.obj != null) {
            ReverbZone reverbZone = ReverbZone.Forest;
            if(scene.name.StartsWith("Cave")) {
                reverbZone = ReverbZone.Cave;
            } else if(scene.name.StartsWith("Forest")) {
                reverbZone = ReverbZone.Forest;
            }
            AudioStateManager.obj.SetReverbZone(reverbZone);
        }
        if(!BackgroundLoaderManager.obj.IsBackgroundLayersLoaded(initRoom.backgroundScene)) {
            yield return StartCoroutine(BackgroundLoaderManager.obj.LoadAndSetBackground(initRoom.backgroundScene));
        }

        if(SaveManager.obj.RestoreBlobOnNextScene) {
            SaveManager.obj.ConsumeRestoreBlobFlag();
            PlayerMovement.obj.ToBlob();
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
        PlayerManager.PlayerType playerType = PlayerManager.obj.GetActivePlayerType();
        if(caveTimelineId == CaveTimelineId.Id.Eli) {
            if(Player.obj != null && playerType == PlayerManager.PlayerType.HUMAN)
                Player.obj.transform.position = playerSpawnPointCollider.transform.position;
            else if(PlayerBlob.obj != null && playerType == PlayerManager.PlayerType.BLOB)
                PlayerBlob.obj.transform.position = playerSpawnPointCollider.transform.position - new Vector3(0, 0.5f, 0);
        } else if(caveTimelineId == CaveTimelineId.Id.Dee) {
            if(Player.obj != null)
                Player.obj.gameObject.SetActive(false);
            if(PlayerBlob.obj != null)
                PlayerBlob.obj.gameObject.SetActive(false);
            if(ShadowTwinPlayer.obj != null) {
                ShadowTwinPlayer.obj.transform.position = playerSpawnPointCollider.transform.position;
            }
        } else if(caveTimelineId == CaveTimelineId.Id.Both) {
            //TODO
        }

        AdjustSpawnFaceDirection(Camera.main.transform.position.x, playerSpawnPoint.transform.position.x, playerType);        

        if(CaveAvatar.obj != null && CaveAvatar.obj.gameObject.activeSelf) {
            SetCaveAvatarPosition(scene, caveTimelineId);
        }

        if(Player.obj != null)
            Player.obj.SetHasPowerUp(false);
        PlayerPowersManager.obj.EliCanForcePushJump = false;
        
        if(PlayerManager.obj.IsSeparated) {
            PlayerManager.obj.EnableAllPlayers();
            SetPlayersStartingState();
            if(PlayerManager.obj.IsCoopActive) {
                LobbyManager.obj.SetPlayerInputs();
            }
        } else {
            PlayerManager.obj.EnablePlayerGameObject(playerType);
            SetPlayersStartingState();
        }

        GameObject mainCamera = null;
        GameObject room = null;
        foreach (var go in sceneRootObjects) {
            if (go.CompareTag("MainCamera")) {
                mainCamera = go;
            } else if (go.CompareTag("Room")) {
                room = go;
            }
            if (mainCamera != null && room != null) {
                break;
            }
        }
        RoomCameraController cameraController = mainCamera.GetComponent<RoomCameraController>();
        Collider2D roomCollider = room.GetComponent<Collider2D>();
        CameraManager.obj.EnterRoom(cameraController, roomCollider, PlayerManager.obj.GetPlayerTransform(PlayerManager.obj.GetActivePlayerType()), playerSpawnPointCollider.transform.position);  

        Reaper.obj.playerKilled = false;

        StartCoroutine(LoadAdjacentRoomsPrivate(initRoom));

        // If we just loaded a game from a save, restore audio state (music + ambience)
        if (SaveManager.obj != null && SaveManager.obj.RestoreAudioOnNextScene) {
            var data = SaveManager.obj.LastLoadedSaveData;
            if (data != null) {
                if (!string.IsNullOrEmpty(data.currentMusicId) && MusicManager.obj != null) {
                    MusicManager.obj.PlayById(data.currentMusicId);
                }
                if (data.currentAmbienceIds != null && data.currentAmbienceIds.Count > 0 && AmbienceManager.obj != null) {
                    foreach (string ambienceId in data.currentAmbienceIds) {
                        AmbienceManager.obj.PlayById(ambienceId);
                    }
                }
            }
            SaveManager.obj.ConsumeRestoreAudioFlag();
        }

        //Give parallax bg a moment to settle before fading in
        yield return StartCoroutine(DelayedSceneFadeIn(playerSpawnPointCollider.transform, playerType));

        yield return null;
    }

    private GameObject FindSceneSpawnPoint(string spawnPointId) {
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
        foreach (var sp in spawnPoints) {
            if (sp.SpawnPointID == spawnPointId) {
                return sp.gameObject;
            }
        }
        return null;
    }

    public void LoadAdjacentRooms(InitRoom initRoomData) {
        StartCoroutine(LoadAdjacentRoomsPrivate(initRoomData));
    }

    private IEnumerator LoadAdjacentRoomsPrivate(InitRoom initRoomData) {
        List<SceneField> adjacentRooms = initRoomData.adjacentRooms;
        foreach(SceneField room in adjacentRooms) {
            Scene sceneToLoad = SceneManager.GetSceneByName(room.SceneName);
            if(!sceneToLoad.isLoaded) {
                SceneManager.LoadSceneAsync(room.SceneName, LoadSceneMode.Additive);
            }
        }
        yield return null;
    }

    public void UnloadNonAdjacentRooms(Scene currentScene, InitRoom initRoomData) {
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

            bool isLevelScene = false;
            foreach (var prefix in LevelSceneNamePrefixes)
            {
                if (scene.name.StartsWith(prefix))
                {
                    isLevelScene = true;
                    break;
                }
            }
            
            if (isLevelScene)
            {
                scenesToUnload.Add(scene);
            }
        }

        foreach (Scene scene in scenesToUnload)
        {
            SceneManager.UnloadSceneAsync(scene);
        }
    }

    private IEnumerator DelayedSceneFadeIn(Transform playerSpawnPoint, PlayerManager.PlayerType playerType) {
        yield return new WaitForSeconds(_reloadSceneDelayTime);
        SceneFadeManager.obj.StartFadeIn();
        PlayerStatsManager.obj.ResumeTimer();
        PlayerManager.obj.PlaySpawn(playerType);
        //Need to play a slightly delayed spawn sound due to when loading a game the sound is broken at the beginning. No idea why!
        StartCoroutine(DelayedSpawnSfx(playerSpawnPoint));
    }

    private void SetPlayersStartingState() {
        if(PlayerBlobMovement.obj != null && PlayerBlobMovement.obj.gameObject.activeSelf) {
            PlayerBlobMovement.obj.SetStartingOnGround();
            PlayerBlobMovement.obj.isGrounded = true;
            PlayerBlobMovement.obj.CancelJumping();
            PlayerBlob.obj.FadeInPlayerLight();
        }

        if(PlayerMovement.obj != null && PlayerMovement.obj.gameObject.activeSelf) {
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

        if(ShadowTwinMovement.obj != null && ShadowTwinMovement.obj.gameObject.activeSelf) {
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
        _sharedCharacterAudio.PlaySpawn(transform);
    }

    private void AdjustSpawnFaceDirection(float sceneLoadTriggerPosition, float playerSpawnPointPosition, PlayerManager.PlayerType playerType) {
        bool isLeftSideOfScreen = sceneLoadTriggerPosition - playerSpawnPointPosition >= 0;
        if(isLeftSideOfScreen && PlayerManager.obj.IsPlayerFacingLeft(playerType))
            PlayerManager.obj.FlipPlayer(playerType);
        else if(!isLeftSideOfScreen && !PlayerManager.obj.IsPlayerFacingLeft(playerType))
            PlayerManager.obj.FlipPlayer(playerType);
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
        List<string> completedLevels = new List<string>();
        foreach (var kvp in levelCompletionMap)
        {
            if (kvp.Value)
            {
                completedLevels.Add(kvp.Key);
            }
        }
        return completedLevels;
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

    private void SetCaveAvatarPosition(Scene scene, CaveTimelineId.Id caveTimelineId) {
        if(caveTimelineId == CaveTimelineId.Id.Eli) {
            if (scene.name == "Cave-56") {
                CaveAvatar.obj.SetStartingPositionInRoom35();
            } else if(scene.name == "Cave-55") {
                if(IsLevelCompleted("Cave-55"))
                    CaveAvatar.obj.SetStartingPositionInRoom35();
                else
                    CaveAvatar.obj.SetStartingPositionInRoom34();
            } else if(scene.name == "Cave-54") {
                if(IsLevelCompleted("Cave-54"))
                    CaveAvatar.obj.SetStartingPositionInRoom34();
                else
                    CaveAvatar.obj.SetStartingPositionInRoom33();
            } else if(scene.name == "Cave-53") {
                if(IsLevelCompleted("Cave-53"))
                    CaveAvatar.obj.SetStartingPositionInRoom33();
                else
                    CaveAvatar.obj.SetStartingPositionInRoom32();
            } else if(scene.name == "Cave-52") {
                if(IsLevelCompleted("Cave-52"))
                    CaveAvatar.obj.SetStartingPositionInRoom32();
                else {
                    if(GameManager.obj.HasEvent(CAVE_52_CONVERSATION_EVENT)) {
                        CaveAvatar.obj.SetStartingPositionInRoom52AfterConversation();
                    } else {
                        CaveAvatar.obj.SetStartingPositionInRoom52BeforeConversation();
                    }    
                }
            } else if(GameManager.obj.HasEvent(SOOT_BETRAYAL_EVENT)) {
                //Let room managers handle Soot's position
            } else if(GameManager.obj.HasEvent(BEFORE_SHADOW_JUMP_EVENT) && !GameManager.obj.HasEvent(AFTER_SHADOW_JUMP_EVENT)) {
                CaveAvatar.obj.SetStartingPositionInCaveRoom33();   
            } else if(!GameManager.obj.HasEvent(SOOT_FREED_EVENT) && !GameManager.obj.isDevMode) {
                CaveAvatar.obj.SetStartingPositionInRoom1();
            } else {
                CaveAvatar.obj.SetFollowPlayerStartingPosition();
                CaveAvatar.obj.FollowPlayer();
            }
        } else if(caveTimelineId == CaveTimelineId.Id.Dee) {
            if(!ShadowTwinPlayer.obj.GetHasCrown()) {
                if(GameManager.obj.HasEvent(CAVE_3_DEE_CONVERSATION_EVENT)) {
                    CaveAvatar.obj.SetStartingPositionInRoom1();
                } else {
                    CaveAvatar.obj.SetFollowPlayerStartingPosition();
                    CaveAvatar.obj.FollowPlayer();    
                }
            } else if(ShadowTwinPlayer.obj.GetHasCrown()) {
                if(CaveAvatar.obj != null && CaveAvatar.obj.gameObject.activeSelf) {
                    CaveAvatar.obj.gameObject.SetActive(false);
                }
            }
        }
    }

    void OnDestroy()
    {
        obj = null;
    }
}
