#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameProgressDebugger))]
public class GameProgressDebuggerEditor : Editor
{
    Vector2 scroll;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var debugger = (GameProgressDebugger)target;
        var events = debugger.Events;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Completed Game Events", EditorStyles.boldLabel);

        if (events == null)
        {
            EditorGUILayout.LabelField("No data (not bound)");
            return;
        }

        if (events.Count == 0)
        {
            EditorGUILayout.LabelField("<none>");
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(
            scroll,
            GUILayout.Height(200)
        );

        foreach (var e in events)
        {
            EditorGUILayout.LabelField("• " + e);
        }

        EditorGUILayout.EndScrollView();
    }
}
#endif
