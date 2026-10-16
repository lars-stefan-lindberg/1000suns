using Cinemachine;
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

    /// <summary>
    /// Retargets the camera(s) following <paramref name="from"/> to <paramref name="to"/>,
    /// e.g. when the player switches character. Keeps the room camera controller in sync.
    /// </summary>
    public static void SwitchFollowTarget(Transform from, Transform to)
    {
        if (obj != null && obj.currentRoomCameraController != null)
            obj.currentRoomCameraController.SwitchFollowTarget(from, to);

        if (CinemachineCore.Instance.BrainCount == 0)
            return;

        ICinemachineCamera activeVirtualCamera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera;
        if (activeVirtualCamera != null && activeVirtualCamera.Follow == from)
            activeVirtualCamera.Follow = to;
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
