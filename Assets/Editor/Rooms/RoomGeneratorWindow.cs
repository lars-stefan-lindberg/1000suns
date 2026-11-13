#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor window for generating a new standard room next to an existing room.
/// </summary>
public class RoomGeneratorWindow : EditorWindow
{
    private const string WindowTitle = "Room Generator";
    private const string MenuPath = "Tools/Rooms/Generate Aligned Room...";

    private StandardRoomType _roomType = StandardRoomType.Standard;
    private SceneAsset _neighborScene;
    private RoomAlignmentSide _side = RoomAlignmentSide.Right;
    private float _offsetInScreens;
    private string _newSceneName = "";

    /// <summary>
    /// Opens the room generator window. The currently active scene is used as the neighbor room.
    /// </summary>
    [MenuItem(MenuPath)]
    public static void Open()
    {
        var window = GetWindow<RoomGeneratorWindow>(WindowTitle);
        window._neighborScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorSceneManager.GetActiveScene().path);
        window._newSceneName = RoomGenerator.GetNextFreeSceneName();
    }

    private void OnGUI()
    {
        _roomType = (StandardRoomType)EditorGUILayout.EnumPopup("Room type", _roomType);
        _neighborScene = (SceneAsset)EditorGUILayout.ObjectField("Align to room", _neighborScene, typeof(SceneAsset), false);
        _side = (RoomAlignmentSide)EditorGUILayout.EnumPopup("Side of that room", _side);
        _offsetInScreens = EditorGUILayout.FloatField(new GUIContent("Offset (screens)",
            "Shift along the shared edge. Left/Right: from the neighbor's bottom edge. Top/Bottom: from the neighbor's left edge."), _offsetInScreens);
        _newSceneName = EditorGUILayout.TextField(new GUIContent("New scene name", "Leave empty for the next free Cave-N."), _newSceneName);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(_neighborScene == null))
        {
            if (GUILayout.Button("Generate room"))
                Generate();
        }
    }

    private void Generate()
    {
        try
        {
            RoomGenerationResult result = RoomGenerator.GenerateAlignedRoom(
                _roomType, AssetDatabase.GetAssetPath(_neighborScene), _side, _offsetInScreens, _newSceneName);
            Debug.Log(result.ToString());
            _newSceneName = RoomGenerator.GetNextFreeSceneName();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog(WindowTitle, exception.Message, "OK");
        }
    }
}
#endif
