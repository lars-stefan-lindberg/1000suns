using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(BoxCollider2D))]
public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnPointID;
    public string SpawnPointID => spawnPointID;

    private BoxCollider2D _collider;

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
    }

    public BoxCollider2D GetCollider() {
        return _collider;
    }

    #if UNITY_EDITOR

    void OnValidate()
    {
        AssignIDIfNeeded();
    }
    void AssignIDIfNeeded()
    {
        // Never generate IDs on prefab assets
        if (PrefabUtility.IsPartOfPrefabAsset(this))
            return;

        if (string.IsNullOrEmpty(spawnPointID))
        {
            var room = gameObject.scene;
            if (room == null) return;

            spawnPointID = $"{room.name}_{System.Guid.NewGuid().ToString()}";
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            EditorUtility.SetDirty(this);
        }
    }
    #endif
}
