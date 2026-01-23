using UnityEngine;

public class Cave3CustomLoader : MonoBehaviour
{
    public GameObject _rootsManager;
    public GameEventId eventId;

    public void UnloadRoots() {
        if(!GameManager.obj.Progress.HasEvent(eventId)) {
            _rootsManager.GetComponent<CaveAvatarRootsManager>().Stop();
        }
    }
}
