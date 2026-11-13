#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The four standard room layouts. The names of the backing prefabs start with "Cave",
/// but the layouts are used for every biome.
/// </summary>
public enum StandardRoomType
{
    /// <summary>1 x 1 screens (320x180 px). Prefab: CaveStandardRoom, camera: MainVCam static.</summary>
    Standard,
    /// <summary>2 x 1 screens. Prefab: CaveHorizontalRoom, camera: MainVCam horizontal.</summary>
    Horizontal,
    /// <summary>1 x 3 screens. Prefab: CaveVerticalRoom, camera: MainVCam vertical.</summary>
    Vertical,
    /// <summary>1.5 x 1.5 screens. Prefab: CaveBigRoom, camera: MainVCam big room.</summary>
    Big
}

/// <summary>
/// The side of the existing (neighbor) room that the new room is attached to.
/// </summary>
public enum RoomAlignmentSide
{
    Left,
    Right,
    Top,
    Bottom
}

/// <summary>
/// The outcome of a room generation.
/// </summary>
public class RoomGenerationResult
{
    public string ScenePath;
    public Vector2 RoomOrigin;
    public Rect RoomBounds;
    public string SpawnPointId;

    public override string ToString()
    {
        return $"Created '{ScenePath}'. Room origin: ({RoomOrigin.x}, {RoomOrigin.y}). " +
               $"Bounds: min ({RoomBounds.xMin}, {RoomBounds.yMin}) max ({RoomBounds.xMax}, {RoomBounds.yMax}). " +
               $"Spawn point ID: {SpawnPointId}";
    }
}

/// <summary>
/// Creates new room scenes from the standard room prefabs and places them flush against an existing room.
/// Does the same thing as dragging the standard prefabs into an empty scene by hand, plus wiring and pixel snapping.
/// </summary>
public static class RoomGenerator
{
    public const float PixelsPerUnit = 8f;
    public const float ScreenWidthInUnits = 40f;   // 320 px
    public const float ScreenHeightInUnits = 22.5f; // 180 px

    private const string TemplatePrefabFolder = "Assets/Prefabs/level/scene_template/";
    private const string ScenesFolder = "Assets/Scenes/";
    private const string SceneFileExtension = ".unity";
    private const string DefaultSceneNamePrefix = "Cave-";
    private const string InitRoomPrefabPath = TemplatePrefabFolder + "InitRoom.prefab";
    private const string LoadPrefabPath = TemplatePrefabFolder + "Load.prefab";
    private const string LevelEntryLeftPrefabPath = TemplatePrefabFolder + "LevelEntry left.prefab";
    private const string RoomTag = "Room";

    private const string RoomObjectsToLoadProperty = "roomObjectsToLoad";
    private const string DarknessLevelTypeProperty = "_darknessLevelType";
    private const string BackgroundSceneProperty = "backgroundScene";
    private const string WalkableSurfaceSceneProperty = "walkableSurfaceScene";
    private const string AdjacentRoomsProperty = "adjacentRooms";
    private const string DefaultSpawnPointProperty = "defaultSpawnPoint";
    private const string IsRetryableProperty = "isRetryable";
    private const string SceneFieldAssetProperty = "_sceneAsset";
    private const string SceneFieldNameProperty = "_sceneName";

    private static readonly Dictionary<StandardRoomType, string> RoomPrefabPaths = new()
    {
        { StandardRoomType.Standard, TemplatePrefabFolder + "CaveStandardRoom.prefab" },
        { StandardRoomType.Horizontal, TemplatePrefabFolder + "CaveHorizontalRoom.prefab" },
        { StandardRoomType.Vertical, TemplatePrefabFolder + "CaveVerticalRoom.prefab" },
        { StandardRoomType.Big, TemplatePrefabFolder + "CaveBigRoom.prefab" },
    };

    private static readonly Dictionary<StandardRoomType, string> CameraPrefabPaths = new()
    {
        { StandardRoomType.Standard, TemplatePrefabFolder + "MainVCam static.prefab" },
        { StandardRoomType.Horizontal, TemplatePrefabFolder + "MainVCam horizontal.prefab" },
        { StandardRoomType.Vertical, TemplatePrefabFolder + "MainVCam vertical.prefab" },
        { StandardRoomType.Big, TemplatePrefabFolder + "MainVCam big room.prefab" },
    };

    /// <summary>
    /// Creates a new room scene of the given type, placed flush against one side of an existing room.
    /// </summary>
    /// <param name="roomType">Layout of the new room.</param>
    /// <param name="neighborSceneReference">Existing room scene: a name ("Cave-60") or an asset path.</param>
    /// <param name="side">Side of the existing room that the new room attaches to.</param>
    /// <param name="offsetInScreens">
    /// Shift along the shared edge, in screens. For Left/Right it is measured from the neighbor's bottom edge to the
    /// new room's bottom edge (screen height units). For Top/Bottom it is measured from the neighbor's left edge to the
    /// new room's left edge (screen width units). 0 aligns the bottom (or left) edges.
    /// </param>
    /// <param name="newSceneName">Name of the new scene. When empty, the next free "Cave-N" name is used.</param>
    public static RoomGenerationResult GenerateAlignedRoom(
        StandardRoomType roomType,
        string neighborSceneReference,
        RoomAlignmentSide side,
        float offsetInScreens = 0f,
        string newSceneName = null)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Room generation is not available in Play Mode.");

        string neighborScenePath = ResolveExistingScenePath(neighborSceneReference);
        string sceneName = string.IsNullOrWhiteSpace(newSceneName) ? GetNextFreeSceneName() : newSceneName.Trim();
        string newScenePath = ValidateNewScenePath(sceneName);

        GameObject roomPrefab = LoadPrefab(RoomPrefabPaths[roomType]);
        GameObject cameraPrefab = LoadPrefab(CameraPrefabPaths[roomType]);
        GameObject initRoomPrefab = LoadPrefab(InitRoomPrefabPath);
        GameObject loadPrefab = LoadPrefab(LoadPrefabPath);
        GameObject levelEntryPrefab = LoadPrefab(LevelEntryLeftPrefabPath);

        Scene neighborScene = OpenSceneAdditively(neighborScenePath);
        GameObject neighborRoom = FindRoomRoot(neighborScene)
            ?? throw new InvalidOperationException($"No root object tagged '{RoomTag}' with a PolygonCollider2D in '{neighborScenePath}'.");
        Rect neighborBounds = GetWorldBounds(neighborRoom.GetComponent<PolygonCollider2D>());
        InitRoom neighborInitRoom = FindRootComponent<InitRoom>(neighborScene);
        RoomMgr neighborRoomManager = neighborRoom.GetComponent<RoomMgr>();

        Rect newRoomLocalBounds = GetLocalBounds(roomPrefab.GetComponent<PolygonCollider2D>()
            ?? throw new InvalidOperationException($"Room prefab '{RoomPrefabPaths[roomType]}' has no PolygonCollider2D."));
        Vector2 roomOrigin = CalculateAlignedOrigin(neighborBounds, newRoomLocalBounds, side, offsetInScreens);

        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        if (!EditorSceneManager.SaveScene(newScene, newScenePath))
            throw new IOException($"Could not save the new scene to '{newScenePath}'.");

        // Same order as hand-built rooms: InitRoom, room, camera, Load, LevelEntry.
        GameObject initRoomInstance = InstantiatePrefabAt(initRoomPrefab, newScene, roomOrigin);
        GameObject roomInstance = InstantiatePrefabAt(roomPrefab, newScene, roomOrigin);
        InstantiatePrefabAt(cameraPrefab, newScene, new Vector3(roomOrigin.x, roomOrigin.y, cameraPrefab.transform.position.z));
        GameObject loadInstance = InstantiatePrefabAt(loadPrefab, newScene, roomOrigin);
        Vector2 levelEntryPosition = CalculateLeftLevelEntryPosition(levelEntryPrefab, roomOrigin, newRoomLocalBounds);
        GameObject levelEntryInstance = InstantiatePrefabAt(levelEntryPrefab, newScene, levelEntryPosition);

        SpawnPoint spawnPoint = levelEntryInstance.GetComponentInChildren<SpawnPoint>(true)
            ?? throw new InvalidOperationException($"'{LevelEntryLeftPrefabPath}' has no SpawnPoint child.");
        spawnPoint.ClearSpawnPointId();
        spawnPoint.GenerateID();
        PrefabUtility.RecordPrefabInstancePropertyModifications(spawnPoint);

        ConfigureRoomManager(roomInstance.GetComponent<RoomMgr>(), loadInstance, neighborRoomManager);
        ConfigureInitRoom(initRoomInstance.GetComponent<InitRoom>(), neighborInitRoom, neighborScenePath, spawnPoint.gameObject);
        AddAdjacentRoom(neighborInitRoom, newScenePath);

        EditorSceneManager.MarkSceneDirty(newScene);
        EditorSceneManager.SaveScene(newScene);
        if (neighborInitRoom != null)
            EditorSceneManager.SaveScene(neighborScene);

        AddSceneToBuildSettings(newScenePath);

        return new RoomGenerationResult
        {
            ScenePath = newScenePath,
            RoomOrigin = roomOrigin,
            RoomBounds = new Rect(roomOrigin + newRoomLocalBounds.min, newRoomLocalBounds.size),
            SpawnPointId = spawnPoint.SpawnPointID
        };
    }

    /// <summary>
    /// Text-based version of <see cref="GenerateAlignedRoom(StandardRoomType, string, RoomAlignmentSide, float, string)"/>.
    /// Used by tools that cannot reference this assembly directly, such as the optional Bezi actions, which call it by reflection.
    /// Keep the signature stable. Returns a human-readable summary of the generated room.
    /// </summary>
    public static string GenerateAlignedRoomFromText(string roomType, string neighborSceneReference, string side, float offsetInScreens, string newSceneName)
    {
        var parsedRoomType = ParseEnum<StandardRoomType>(roomType, nameof(roomType));
        var parsedSide = ParseEnum<RoomAlignmentSide>(side, nameof(side));
        return GenerateAlignedRoom(parsedRoomType, neighborSceneReference, parsedSide, offsetInScreens, newSceneName).ToString();
    }

    /// <summary>
    /// Generates IDs for every SpawnPoint in the scene that does not have one yet, then saves the scene.
    /// Returns the number of IDs generated.
    /// </summary>
    public static int GenerateMissingSpawnPointIds(string sceneReference)
    {
        string scenePath = ResolveExistingScenePath(sceneReference);
        Scene scene = OpenSceneAdditively(scenePath);
        int generatedCount = 0;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (SpawnPoint spawnPoint in rootObject.GetComponentsInChildren<SpawnPoint>(true))
            {
                if (spawnPoint.HasID)
                    continue;

                spawnPoint.GenerateID();
                EditorUtility.SetDirty(spawnPoint);
                PrefabUtility.RecordPrefabInstancePropertyModifications(spawnPoint);
                generatedCount++;
            }
        }

        if (generatedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        return generatedCount;
    }

    /// <summary>
    /// Returns the next unused "Cave-N" scene name in the scenes folder.
    /// </summary>
    public static string GetNextFreeSceneName()
    {
        var numberPattern = new Regex($"^{Regex.Escape(DefaultSceneNamePrefix)}(\\d+)$");
        int highestNumber = AssetDatabase.FindAssets("t:Scene", new[] { ScenesFolder.TrimEnd('/') })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(Path.GetFileNameWithoutExtension)
            .Select(name => numberPattern.Match(name))
            .Where(match => match.Success)
            .Select(match => int.Parse(match.Groups[1].Value))
            .DefaultIfEmpty(0)
            .Max();
        return $"{DefaultSceneNamePrefix}{highestNumber + 1}";
    }

    /// <summary>
    /// Rounds a value to the nearest pixel (1 / PixelsPerUnit units).
    /// </summary>
    public static float SnapToPixelGrid(float value)
    {
        return Mathf.Round(value * PixelsPerUnit) / PixelsPerUnit;
    }

    /// <summary>
    /// Rounds the x and y components to the nearest pixel. Z is left unchanged.
    /// </summary>
    public static Vector3 SnapToPixelGrid(Vector3 position)
    {
        return new Vector3(SnapToPixelGrid(position.x), SnapToPixelGrid(position.y), position.z);
    }

    private static TEnum ParseEnum<TEnum>(string value, string parameterName) where TEnum : struct, Enum
    {
        if (Enum.TryParse(value?.Trim(), true, out TEnum parsed) && Enum.IsDefined(typeof(TEnum), parsed))
            return parsed;
        throw new ArgumentException($"Invalid {parameterName} '{value}'. Expected one of: {string.Join(", ", Enum.GetNames(typeof(TEnum)))}.");
    }

    private static Vector2 CalculateAlignedOrigin(Rect neighborBounds, Rect newRoomLocalBounds, RoomAlignmentSide side, float offsetInScreens)
    {
        float verticalOffset = offsetInScreens * ScreenHeightInUnits;
        float horizontalOffset = offsetInScreens * ScreenWidthInUnits;
        Vector2 newRoomMin = side switch
        {
            RoomAlignmentSide.Right => new Vector2(neighborBounds.xMax, neighborBounds.yMin + verticalOffset),
            RoomAlignmentSide.Left => new Vector2(neighborBounds.xMin - newRoomLocalBounds.width, neighborBounds.yMin + verticalOffset),
            RoomAlignmentSide.Top => new Vector2(neighborBounds.xMin + horizontalOffset, neighborBounds.yMax),
            RoomAlignmentSide.Bottom => new Vector2(neighborBounds.xMin + horizontalOffset, neighborBounds.yMin - newRoomLocalBounds.height),
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
        Vector2 origin = newRoomMin - newRoomLocalBounds.min;
        return new Vector2(SnapToPixelGrid(origin.x), SnapToPixelGrid(origin.y));
    }

    private static Vector2 CalculateLeftLevelEntryPosition(GameObject levelEntryPrefab, Vector2 roomOrigin, Rect newRoomLocalBounds)
    {
        var trigger = levelEntryPrefab.GetComponent<BoxCollider2D>();
        float halfTriggerWidth = trigger != null ? trigger.size.x * 0.5f : 0f;
        float triggerOffsetX = trigger != null ? trigger.offset.x : 0f;
        float leftEdge = roomOrigin.x + newRoomLocalBounds.xMin;
        // The trigger sits just inside the left edge, vertically centered on the room origin's screen.
        return new Vector2(leftEdge + halfTriggerWidth - triggerOffsetX, roomOrigin.y);
    }

    private static void ConfigureRoomManager(RoomMgr roomManager, GameObject loadInstance, RoomMgr neighborRoomManager)
    {
        if (roomManager == null)
            throw new InvalidOperationException("The room prefab has no RoomMgr component.");

        var serializedRoomManager = new SerializedObject(roomManager);
        SerializedProperty objectsToLoad = serializedRoomManager.FindProperty(RoomObjectsToLoadProperty);
        objectsToLoad.arraySize = 1;
        objectsToLoad.GetArrayElementAtIndex(0).objectReferenceValue = loadInstance;

        if (neighborRoomManager != null)
        {
            var serializedNeighbor = new SerializedObject(neighborRoomManager);
            serializedRoomManager.FindProperty(DarknessLevelTypeProperty).enumValueIndex =
                serializedNeighbor.FindProperty(DarknessLevelTypeProperty).enumValueIndex;
        }

        serializedRoomManager.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigureInitRoom(InitRoom initRoom, InitRoom neighborInitRoom, string neighborScenePath, GameObject defaultSpawnPoint)
    {
        if (initRoom == null)
            throw new InvalidOperationException("The InitRoom prefab has no InitRoom component.");

        var serializedInitRoom = new SerializedObject(initRoom);

        if (neighborInitRoom != null)
        {
            var serializedNeighbor = new SerializedObject(neighborInitRoom);
            CopySceneField(serializedNeighbor.FindProperty(BackgroundSceneProperty), serializedInitRoom.FindProperty(BackgroundSceneProperty));
            CopySceneField(serializedNeighbor.FindProperty(WalkableSurfaceSceneProperty), serializedInitRoom.FindProperty(WalkableSurfaceSceneProperty));
            serializedInitRoom.FindProperty(IsRetryableProperty).boolValue = serializedNeighbor.FindProperty(IsRetryableProperty).boolValue;
        }

        SerializedProperty adjacentRooms = serializedInitRoom.FindProperty(AdjacentRoomsProperty);
        adjacentRooms.arraySize = 1;
        SetSceneField(adjacentRooms.GetArrayElementAtIndex(0), neighborScenePath);

        serializedInitRoom.FindProperty(DefaultSpawnPointProperty).objectReferenceValue = defaultSpawnPoint;
        serializedInitRoom.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddAdjacentRoom(InitRoom initRoom, string adjacentScenePath)
    {
        if (initRoom == null)
        {
            Debug.LogWarning("The neighbor room has no InitRoom, so the new room was not added to its adjacent rooms.");
            return;
        }

        var serializedInitRoom = new SerializedObject(initRoom);
        SerializedProperty adjacentRooms = serializedInitRoom.FindProperty(AdjacentRoomsProperty);
        string adjacentSceneName = Path.GetFileNameWithoutExtension(adjacentScenePath);

        for (int index = 0; index < adjacentRooms.arraySize; index++)
        {
            if (adjacentRooms.GetArrayElementAtIndex(index).FindPropertyRelative(SceneFieldNameProperty).stringValue == adjacentSceneName)
                return;
        }

        adjacentRooms.arraySize++;
        SetSceneField(adjacentRooms.GetArrayElementAtIndex(adjacentRooms.arraySize - 1), adjacentScenePath);
        serializedInitRoom.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(initRoom.gameObject.scene);
    }

    private static void CopySceneField(SerializedProperty source, SerializedProperty destination)
    {
        destination.FindPropertyRelative(SceneFieldAssetProperty).objectReferenceValue =
            source.FindPropertyRelative(SceneFieldAssetProperty).objectReferenceValue;
        destination.FindPropertyRelative(SceneFieldNameProperty).stringValue =
            source.FindPropertyRelative(SceneFieldNameProperty).stringValue;
    }

    private static void SetSceneField(SerializedProperty sceneField, string scenePath)
    {
        sceneField.FindPropertyRelative(SceneFieldAssetProperty).objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        sceneField.FindPropertyRelative(SceneFieldNameProperty).stringValue = Path.GetFileNameWithoutExtension(scenePath);
    }

    private static GameObject InstantiatePrefabAt(GameObject prefab, Scene scene, Vector3 position)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.transform.position = SnapToPixelGrid(position);
        PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
        return instance;
    }

    private static Rect GetWorldBounds(PolygonCollider2D collider)
    {
        IEnumerable<Vector2> worldPoints = GetColliderPoints(collider)
            .Select(point => (Vector2)collider.transform.TransformPoint(point + collider.offset));
        return SnappedRectFromPoints(worldPoints);
    }

    private static Rect GetLocalBounds(PolygonCollider2D collider)
    {
        IEnumerable<Vector2> localPoints = GetColliderPoints(collider)
            .Select(point => Vector2.Scale(point + collider.offset, collider.transform.localScale));
        return SnappedRectFromPoints(localPoints);
    }

    private static IEnumerable<Vector2> GetColliderPoints(PolygonCollider2D collider)
    {
        for (int pathIndex = 0; pathIndex < collider.pathCount; pathIndex++)
        {
            foreach (Vector2 point in collider.GetPath(pathIndex))
                yield return point;
        }
    }

    // The room colliders are padded by ~0.01 units, so the bounds are snapped to the pixel grid before alignment.
    private static Rect SnappedRectFromPoints(IEnumerable<Vector2> points)
    {
        List<Vector2> pointList = points.ToList();
        if (pointList.Count == 0)
            throw new InvalidOperationException("The room collider has no points.");

        float minX = SnapToPixelGrid(pointList.Min(point => point.x));
        float minY = SnapToPixelGrid(pointList.Min(point => point.y));
        float maxX = SnapToPixelGrid(pointList.Max(point => point.x));
        float maxY = SnapToPixelGrid(pointList.Max(point => point.y));
        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    private static GameObject FindRoomRoot(Scene scene)
    {
        return scene.GetRootGameObjects()
            .FirstOrDefault(rootObject => rootObject.CompareTag(RoomTag) && rootObject.GetComponent<PolygonCollider2D>() != null);
    }

    private static T FindRootComponent<T>(Scene scene) where T : Component
    {
        return scene.GetRootGameObjects()
            .Select(rootObject => rootObject.GetComponent<T>())
            .FirstOrDefault(component => component != null);
    }

    private static Scene OpenSceneAdditively(string scenePath)
    {
        Scene scene = SceneManager.GetSceneByPath(scenePath);
        return scene.isLoaded ? scene : EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
    }

    private static GameObject LoadPrefab(string prefabPath)
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
            ?? throw new FileNotFoundException($"Standard room prefab not found at '{prefabPath}'.");
    }

    private static string ResolveExistingScenePath(string sceneReference)
    {
        if (string.IsNullOrWhiteSpace(sceneReference))
            throw new ArgumentException("A scene name or path is required.");

        string reference = sceneReference.Trim().Replace('\\', '/');
        int assetsIndex = reference.IndexOf("Assets/", StringComparison.Ordinal);
        string scenePath = assetsIndex >= 0
            ? reference.Substring(assetsIndex)
            : ScenesFolder + reference;
        if (!scenePath.EndsWith(SceneFileExtension, StringComparison.OrdinalIgnoreCase))
            scenePath += SceneFileExtension;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            throw new FileNotFoundException($"Scene not found: '{scenePath}'.");
        return scenePath;
    }

    private static string ValidateNewScenePath(string sceneName)
    {
        if (sceneName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException($"'{sceneName}' is not a valid scene name.");

        string scenePath = ScenesFolder + sceneName + SceneFileExtension;
        if (File.Exists(scenePath) || AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) != null)
            throw new IOException($"A scene already exists at '{scenePath}'.");
        return scenePath;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
        if (buildScenes.Any(buildScene => buildScene.path == scenePath))
            return;

        buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = buildScenes.ToArray();
    }
}
#endif
