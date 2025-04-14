using UnityEngine;

public class C27FloorManager : MonoBehaviour
{
    [SerializeField] private GameObject _floor;

    void Awake() {
        if(GameEventManager.obj.C275FloorBroken)
            _floor.SetActive(false);
    }
}
