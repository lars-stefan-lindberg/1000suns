using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SpawnPoint : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private string spawnPointID;
    public string SpawnPointID => spawnPointID;

    private BoxCollider2D _collider;

    void Awake() {
        _collider = GetComponent<BoxCollider2D>();
    }

    public BoxCollider2D GetCollider() {
        return _collider;
    }

    #if UNITY_EDITOR

    public bool HasID => !string.IsNullOrEmpty(spawnPointID);

    public void GenerateID()
    {
        if (HasID)
            return;

        var room = gameObject.scene;
        if (room == null) return;
        spawnPointID = $"{room.name}_{System.Guid.NewGuid().ToString()}";
    }
    #endif
}
