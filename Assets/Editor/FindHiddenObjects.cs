using UnityEngine;
using UnityEditor;

public class FindHiddenObjects : EditorWindow
{
    [MenuItem("Tools/Find Hidden GameObjects")]
    static void FindHidden()
    {
        // FindObjectsByType includes hidden objects when using the right overload
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go.hideFlags != HideFlags.None)
            {
                Debug.Log($"Hidden Object: {go.name} | HideFlags: {go.hideFlags} | Scene: {go.scene.name}");
            }
        }
    }
}