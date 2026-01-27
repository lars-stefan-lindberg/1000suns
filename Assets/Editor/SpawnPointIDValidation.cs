#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class SpawnPointIDValidation
{
    [MenuItem("Tools/Spawn Points/Validate Duplicate IDs")]
    public static void ValidateDuplicateIDs()
    {
        var points = Object.FindObjectsOfType<SpawnPoint>(true);

        var duplicates = points
            .Where(p => !string.IsNullOrEmpty(p.SpawnPointID))
            .GroupBy(p => p.SpawnPointID)
            .Where(g => g.Count() > 1);

        bool found = false;

        foreach (var dup in duplicates)
        {
            found = true;
            Debug.LogError(
                $"Duplicate Spawn point ID: {dup.Key}",
                dup.First()
            );
        }

        if (!found)
            Debug.Log("No duplicate Spawn point IDs found.");
    }

    [MenuItem("Tools/Spawn Points/Find Missing IDs")]
    public static void FindMissingIDs()
    {
        var points = Object.FindObjectsOfType<SpawnPoint>(true);

        var missing = points
            .Where(p => string.IsNullOrEmpty(p.SpawnPointID))
            .ToList();

        if (missing.Count == 0)
        {
            Debug.Log("All SpawnPoints have IDs 👍");
            return;
        }

        Debug.LogError(
            $"Found {missing.Count} SpawnPoint(s) with missing IDs."
        );

        foreach (var p in missing)
        {
            Debug.LogError(
                $"Missing Spawn ID on SpawnPoint: {p.gameObject.name}",
                p
            );
        }

        // Optional: select all missing ones
        Selection.objects = missing
            .Select(p => p.gameObject)
            .ToArray();
    }
}
#endif
