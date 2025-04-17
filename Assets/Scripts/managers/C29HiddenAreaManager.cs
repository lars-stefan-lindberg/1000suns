using UnityEngine;

public class C29HiddenAreaManager : MonoBehaviour
{
    [SerializeField] private GameObject _hiddenArea;

    void Awake() {
        if(PlayerPowersManager.obj.BlobCanExtraJump)
            _hiddenArea.SetActive(false);
    }
}
