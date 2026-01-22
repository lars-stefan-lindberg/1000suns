using UnityEngine;

public class InitRoom : MonoBehaviour
{
    public SceneField backgroundScene;
    public SceneField walkableSurfaceScene;

    void Start()
    {
        StartCoroutine(WalkableSurfacesManager.obj.AddWalkableSurface(walkableSurfaceScene));
    }
}
