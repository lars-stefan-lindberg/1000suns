using System.Collections.Generic;
using UnityEngine;

public class InitRoom : MonoBehaviour
{
    public SceneField backgroundScene;
    public SceneField walkableSurfaceScene;
    public List<SceneField> adjacentRooms;
    public GameObject defaultSpawnPoint;
    public bool isRetryable = true;

    void Start()
    {
        StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(walkableSurfaceScene));
    }

    public bool IsRetryable()
    {
        IRetryable retryable = GetComponent<IRetryable>();
        if (retryable != null)
            return retryable.IsRetryable();
        
        return isRetryable;
    }
}
