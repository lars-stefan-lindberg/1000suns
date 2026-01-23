using UnityEngine;

public class C28HiddenAreaManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _hiddenAreas;

    void Awake() {
        if(PlayerPowersManager.obj.EliBlobCanExtraJump)
        {
            foreach(GameObject area in _hiddenAreas)
            {
                Destroy(area);
            }

        }
    }
}
