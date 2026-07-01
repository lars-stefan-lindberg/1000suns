using UnityEngine;
using UnityEditor;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts")]
    static void ShowWindow()
    {
        GetWindow<FindMissingScripts>("Find Missing Scripts");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in Scene"))
        {
            FindInScene();
        }

        if (GUILayout.Button("Find Missing Scripts in Selected"))
        {
            FindInSelected();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Find Hidden GameObjects"))
        {
            FindHiddenObjects();
        }

        if (GUILayout.Button("Reveal All Hidden GameObjects"))
        {
            RevealAllHiddenObjects();
        }

        if (GUILayout.Button("Hide Selected GameObjects"))
        {
            HideSelectedObjects();
        }
    }

    private static void FindInScene()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int missingCount = 0;

        foreach (GameObject go in allObjects)
        {
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    missingCount++;
                    Debug.LogError($"Missing script on GameObject: {GetFullPath(go)} (Component index: {i})", go);
                }
            }
        }

        if (missingCount == 0)
        {
            Debug.Log("No missing scripts found in scene!");
        }
        else
        {
            Debug.LogWarning($"Found {missingCount} missing script(s) in scene. Check the console for details.");
        }
    }

    private static void FindInSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected!");
            return;
        }

        int missingCount = 0;

        foreach (GameObject go in selectedObjects)
        {
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    missingCount++;
                    Debug.LogError($"Missing script on GameObject: {GetFullPath(go)} (Component index: {i})", go);
                }
            }
        }

        if (missingCount == 0)
        {
            Debug.Log("No missing scripts found in selection!");
        }
        else
        {
            Debug.LogWarning($"Found {missingCount} missing script(s) in selection. Check the console for details.");
        }
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform parent = go.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    private static void FindHiddenObjects()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int hiddenCount = 0;

        foreach (GameObject go in allObjects)
        {
            if (go.hideFlags != HideFlags.None && !EditorUtility.IsPersistent(go))
            {
                hiddenCount++;
                Debug.LogWarning($"Hidden GameObject found: {GetFullPath(go)} (HideFlags: {go.hideFlags})", go);
            }
        }

        if (hiddenCount == 0)
        {
            Debug.Log("No hidden GameObjects found in scene!");
        }
        else
        {
            Debug.LogWarning($"Found {hiddenCount} hidden GameObject(s). Check the console for details.");
        }
    }

    private static void RevealAllHiddenObjects()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int revealedCount = 0;

        foreach (GameObject go in allObjects)
        {
            if (go.hideFlags != HideFlags.None && !EditorUtility.IsPersistent(go))
            {
                Undo.RecordObject(go, "Reveal Hidden GameObject");
                go.hideFlags = HideFlags.None;
                revealedCount++;
                Debug.Log($"Revealed GameObject: {GetFullPath(go)}", go);
            }
        }

        if (revealedCount == 0)
        {
            Debug.Log("No hidden GameObjects to reveal!");
        }
        else
        {
            Debug.Log($"Revealed {revealedCount} hidden GameObject(s). Check the hierarchy.");
            EditorApplication.RepaintHierarchyWindow();
        }
    }

    private static void HideSelectedObjects()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected to hide!");
            return;
        }

        foreach (GameObject go in selectedObjects)
        {
            Undo.RecordObject(go, "Hide GameObject");
            go.hideFlags = HideFlags.HideInHierarchy;
            Debug.Log($"Hidden GameObject: {GetFullPath(go)}", go);
        }

        Debug.Log($"Hidden {selectedObjects.Length} GameObject(s) from hierarchy.");
        EditorApplication.RepaintHierarchyWindow();
    }
}
