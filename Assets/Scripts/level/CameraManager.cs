using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager obj;

    private RoomCameraController currentRoomCameraController;
    private static PolygonCollider2D currentRoomCollider;

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
        
        currentRoomCollider = confiner as PolygonCollider2D;
    }

    public static PolygonCollider2D GetCurrentRoomCollider()
    {
        return currentRoomCollider;
    }

    void OnDestroy()
    {
        obj = null;
    }
}
