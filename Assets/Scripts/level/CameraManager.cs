using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager obj;

    private RoomCameraController currentRoomCameraController;

    void Awake()
    {
        obj = this;
    }

    public bool IsRoomCameraActivated() {
        if(currentRoomCameraController == null)
            return false;
        return currentRoomCameraController.IsRoomCameraActivated();
    }

    public void EnterRoom(
        RoomCameraController room,
        Collider2D confiner,
        Transform player,
        Vector3 spawnPosition)
    {
        if (currentRoomCameraController != null)
            currentRoomCameraController.Deactivate();

        currentRoomCameraController = room;
        currentRoomCameraController.Activate(confiner, player, spawnPosition);
    }

    void OnDestroy()
    {
        obj = null;
    }
}
