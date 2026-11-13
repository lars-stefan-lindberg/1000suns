#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Bezi;

/// <summary>
/// Bezi actions that let agents generate standard rooms and spawn point IDs.
/// This file is in its own assembly (Rooms.BeziActions), which Unity only compiles when the Bezi package
/// (com.bezi.sidekick) is installed, so the project still compiles without Bezi.
/// Asmdef assemblies cannot reference Assembly-CSharp-Editor, where RoomGenerator lives, so RoomGenerator is called by reflection.
/// </summary>
public static class RoomBeziActions
{
    private const string RoomGeneratorTypeName = "RoomGenerator, Assembly-CSharp-Editor";
    private const string GenerateAlignedRoomMethodName = "GenerateAlignedRoomFromText";
    private const string GenerateMissingSpawnPointIdsMethodName = "GenerateMissingSpawnPointIds";

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
        return (string)InvokeRoomGenerator(GenerateAlignedRoomMethodName, roomType, neighborScene, side, offsetInScreens, newSceneName);
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
        return (int)InvokeRoomGenerator(GenerateMissingSpawnPointIdsMethodName, sceneName);
    }

    private static object InvokeRoomGenerator(string methodName, params object[] arguments)
    {
        Type roomGeneratorType = Type.GetType(RoomGeneratorTypeName)
            ?? throw new InvalidOperationException($"Could not find '{RoomGeneratorTypeName}'. Make sure the editor scripts compile.");
        MethodInfo method = roomGeneratorType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMethodException(roomGeneratorType.Name, methodName);

        try
        {
            return method.Invoke(null, arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            // Surface the generator's own error message instead of the reflection wrapper.
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}
#endif
