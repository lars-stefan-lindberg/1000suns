using System.Collections.Generic;
using UnityEngine;

public class InitRoom : MonoBehaviour
{
    public SceneField backgroundScene;
    public SceneField walkableSurfaceScene;
    public List<SceneField> adjacentRooms;
    public GameObject defaultSpawnPoint;

    void Start()
    {
        StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(walkableSurfaceScene));
    }
}
