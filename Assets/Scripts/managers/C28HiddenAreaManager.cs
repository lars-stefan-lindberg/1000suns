using UnityEngine;

public class C28HiddenAreaManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _hiddenAreas;

    void Awake() {
        if(PlayerPowersManager.obj.BlobCanExtraJump)
        {
            foreach(GameObject area in _hiddenAreas)
            {
                Destroy(area);
            }

        }
    }
}
