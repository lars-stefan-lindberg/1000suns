using UnityEngine;

public class TempBgUnloaderTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        LevelManager.obj.RemoveBackgroundLayers();
    }
}
