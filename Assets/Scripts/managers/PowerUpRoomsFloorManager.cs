using UnityEngine;

public class PowerUpRoomsFloorManager : MonoBehaviour
{
    [SerializeField] private GameObject _floor;
    [SerializeField] private GameEventId _cave33FloorBroken;

    void Awake() {
        if(GameManager.obj.HasEvent(_cave33FloorBroken))
            _floor.SetActive(false);
    }
}
