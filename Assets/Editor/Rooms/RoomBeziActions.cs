#if UNITY_EDITOR
using System;
using Bezi;

/// <summary>
/// Bezi actions that let agents generate standard rooms and spawn point IDs.
/// </summary>
public static class RoomBeziActions
{
    /// <summary>
    /// Creates a new room scene of a standard type, aligned flush against one side of an existing room scene.
    /// </summary>
    [BeziAction(
        "Generate a new room scene from the standard room prefabs, placed flush against one side of an existing room. " +
        "roomType: Standard | Horizontal | Vertical | Big. " +
        "neighborScene: existing room scene name (e.g. 'Cave-60') or asset path. " +
        "side: Left | Right | Top | Bottom (side of the existing room). " +
        "offsetInScreens: shift along the shared edge in screens (Left/Right: from the neighbor's bottom edge, 1 = 22.5 units; " +
        "Top/Bottom: from the neighbor's left edge, 1 = 40 units). 0 aligns the bottom/left edges. " +
        "newSceneName: optional; empty uses the next free 'Cave-N'. Returns the new scene path, origin, bounds and spawn point ID."
    )]
    public static string GenerateAlignedRoom(string roomType, string neighborScene, string side, float offsetInScreens = 0f, string newSceneName = "")
    {
        var parsedRoomType = ParseEnum<StandardRoomType>(roomType, nameof(roomType));
        var parsedSide = ParseEnum<RoomAlignmentSide>(side, nameof(side));
        return RoomGenerator.GenerateAlignedRoom(parsedRoomType, neighborScene, parsedSide, offsetInScreens, newSceneName).ToString();
    }

    /// <summary>
    /// Generates IDs for all SpawnPoints without one in the given scene, and saves it.
    /// </summary>
    [BeziAction(
        "Generate spawn point IDs for every SpawnPoint in a scene that does not have one yet (same as the inspector " +
        "'Generate Spawn point ID' button), then save the scene. sceneName: scene name (e.g. 'Cave-60') or asset path. " +
        "Returns the number of IDs generated."
    )]
    public static int GenerateMissingSpawnPointIds(string sceneName)
    {
        return RoomGenerator.GenerateMissingSpawnPointIds(sceneName);
    }

    private static TEnum ParseEnum<TEnum>(string value, string parameterName) where TEnum : struct, Enum
    {
        if (Enum.TryParse(value?.Trim(), true, out TEnum parsed) && Enum.IsDefined(typeof(TEnum), parsed))
            return parsed;
        throw new ArgumentException($"Invalid {parameterName} '{value}'. Expected one of: {string.Join(", ", Enum.GetNames(typeof(TEnum)))}.");
    }
}
#endif
