#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(SpawnPoint))]
public class SpawnPointEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var spawnPoint = (SpawnPoint)target;

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(spawnPoint.HasID))
        {
            if (GUILayout.Button("Generate Spawn point ID"))
            {
                spawnPoint.GenerateID();
                EditorUtility.SetDirty(spawnPoint);
                EditorSceneManager.MarkSceneDirty(spawnPoint.gameObject.scene);
            }
        }

        if (spawnPoint.HasID)
        {
            EditorGUILayout.HelpBox(
                $"Spawn point ID:\n{spawnPoint.SpawnPointID}",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "No Spawn point ID assigned.\nThis SpawnPoint cannot be used for saving/loading.",
                MessageType.Warning
            );
        }
    }
}
#endif
