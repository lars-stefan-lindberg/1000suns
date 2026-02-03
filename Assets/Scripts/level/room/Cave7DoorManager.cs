using UnityEngine;

public class Cave7DoorManager : MonoBehaviour
{
    
    [SerializeField] private GameEventId _doorClosed;
    [SerializeField] private PillarDoor _door;

    void Awake() {
        if(!GameManager.obj.HasEvent(_doorClosed)) {
            _door.SetFullyOpenImmediate();
        }
    }

    public void CloseDoor() {
        if(!GameManager.obj.HasEvent(_doorClosed)) {
            _door.Close();
            GameManager.obj.RegisterEvent(_doorClosed);
        }
    }
}
