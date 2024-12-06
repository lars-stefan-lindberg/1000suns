using UnityEngine;

public class PowerUpRoomsFloorManager : MonoBehaviour
{
    [SerializeField] private GameObject _floor;

    void Awake() {
        if(GameEventManager.obj.PowerUpRoomsFloorBroken)
            _floor.SetActive(false);
    }
}
