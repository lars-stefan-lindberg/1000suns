using Cinemachine;
using UnityEngine;

// Run before CinemachineBrain (default order 0) so the tracked offset set in LateUpdate
// is used by the camera in the same frame.
[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class RoomCameraController : MonoBehaviour
{
    public RoomCameraType roomType;
    [Header("Vertical Room - Top Lock")]
    public TopLockMode topLockMode = TopLockMode.Automatic;
    public float topLockDistance = 2.5f;
    public float unlockHysteresis = 0.5f;
    [Tooltip("Approximate time (seconds) for the camera to ease up into top lock")]
    public float lockSmoothTime = 0.6f;
    [Tooltip("Approximate time (seconds) for the camera to ease back down out of top lock")]
    public float unlockSmoothTime = 0.6f;

    private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer framing;
    private CinemachineConfiner2D vcamConfiner;
    private CameraLookaheadController lookaheadController;
    private Transform player;
    private bool _yLockedToTop = false;
    private float _topY;
    private bool _justActivated = false;
    private bool _requiresVerticalHandling;
    // 0 = camera follows the player vertically, 1 = camera is pinned to the top of the room
    private float _topLockBlend = 0f;
    private float _topLockBlendVelocity = 0f;

    public enum RoomCameraType
    {
        Static,
        Horizontal,
        Vertical,
        HorizontalAndVertical
    }

    public enum TopLockMode
    {
        Automatic,
        Manual
    }

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        framing = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        vcamConfiner = vcam.GetComponent<CinemachineConfiner2D>();
        lookaheadController = GetComponent<CameraLookaheadController>();
        _requiresVerticalHandling = (roomType == RoomCameraType.Vertical || roomType == RoomCameraType.HorizontalAndVertical);
    }

    void CacheConfinerBounds()
    {
        if (vcamConfiner != null && vcamConfiner.m_BoundingShape2D != null)
        {
            _topY = vcamConfiner.m_BoundingShape2D.bounds.max.y;
        }
    }

    public void Activate(Collider2D confiner, Transform playerTransform, Vector3 spawnPosition)
    {
        gameObject.SetActive(true);
        player = playerTransform;
        
        //Don't set confiner for static rooms, since camera shake won't work
        if(roomType != RoomCameraType.Static) {
            vcam.Follow = player;
            vcamConfiner.m_BoundingShape2D = confiner;
            CacheConfinerBounds();
            vcamConfiner.InvalidateCache();
            transform.position = new Vector3(spawnPosition.x, spawnPosition.y, transform.position.z);
        }

        ConfigureForRoomType();
        _justActivated = true;
        vcam.enabled = true;
        
        // Initialize lookahead controller
        if (lookaheadController != null && framing != null)
        {
            lookaheadController.Initialize(framing);
        }
    }

    public void Deactivate() {
        vcam.Follow = null;
        vcam.enabled = false;
        
        // Disable lookahead controller
        if (lookaheadController != null)
        {
            lookaheadController.Disable();
        }

        gameObject.SetActive(false);
        _yLockedToTop = false;
        _topLockBlend = 0f;
        _topLockBlendVelocity = 0f;
    }

    public bool IsRoomCameraActivated() {
        return vcam.enabled;
    }

    /// <summary>
    /// Manually locks the camera to the top. Use this when topLockMode is set to Manual.
    /// </summary>
    public void ManualTopLock()
    {
        if (!_requiresVerticalHandling || framing == null)
            return;

        if (!_yLockedToTop)
        {
            topLockMode = TopLockMode.Automatic;
            LockCameraToTopSmooth();
        }
    }

    /// <summary>
    /// Manually unlocks the camera from the top. Use this when topLockMode is set to Manual.
    /// </summary>
    public void ManualTopUnlock()
    {
        if (!_requiresVerticalHandling || framing == null)
            return;

        if (_yLockedToTop)
        {
            topLockMode = TopLockMode.Manual;
            UnlockCameraY();
        }
    }

    void ConfigureForRoomType()
    {
        if(framing != null) {
            framing.m_LookaheadTime = 0;
            framing.m_ScreenX = 0.5f;
            framing.m_ScreenY = 0.5f;
            framing.m_TrackedObjectOffset = Vector3.zero;
            
            // Update lookahead controller base screen Y
            if (lookaheadController != null)
            {
                lookaheadController.SetBaseScreenY(0.5f);
            }

            switch (roomType)
            {
                case RoomCameraType.Static:
                    vcam.Follow = null;
                    break;

                case RoomCameraType.Horizontal:
                    framing.m_DeadZoneHeight = 999;
                    framing.m_DeadZoneWidth = 0;
                    break;

                case RoomCameraType.Vertical:
                    framing.m_DeadZoneWidth = 999;
                    framing.m_DeadZoneHeight = 0;
                    break;

                case RoomCameraType.HorizontalAndVertical:
                    framing.m_DeadZoneWidth = 0;
                    framing.m_DeadZoneHeight = 0;
                    break;
            }
        }
    }

    void LateUpdate()
    {
        if (_requiresVerticalHandling)
        {
            HandleVerticalTopLock();
            _justActivated = false;
        }
        
        // Apply lookahead camera changes
        if (lookaheadController != null)
        {
            lookaheadController.UpdateLookahead();
        }
    }

    void HandleVerticalTopLock()
    {
        if (player == null || framing == null)
            return;

        // Update lookahead controller with current top lock state
        if (lookaheadController != null)
        {
            lookaheadController.SetInTopLock(_yLockedToTop);
        }

        // Read input first to determine if lookahead wants to be active
        // This doesn't apply the camera changes yet
        if (lookaheadController != null)
        {
            lookaheadController.ReadInput();
        }

        // Only run automatic lock/unlock logic if in Automatic mode
        if (topLockMode == TopLockMode.Automatic)
        {
            float playerY = player.position.y;
            float distanceToTop = _topY - playerY;

            // Normal lock/unlock logic
            if (!_yLockedToTop && distanceToTop <= topLockDistance)
            {
                if (_justActivated)
                    LockCameraToTopImmediate();
                else
                    LockCameraToTopSmooth();
            }
            else if (_yLockedToTop && distanceToTop > topLockDistance + unlockHysteresis)
            {
                UnlockCameraY();
            }
        }

        ApplyTopLockOffset();
    }

    private void LockCameraToTopImmediate()
    {
        _yLockedToTop = true;
        _topLockBlend = 1f;
        _topLockBlendVelocity = 0f;
        
        if (lookaheadController != null)
        {
            lookaheadController.ResetToBase();
        }

        // Force Cinemachine to forget previous state
        vcam.OnTargetObjectWarped(player, Vector3.zero);
    }

    private void LockCameraToTopSmooth()
    {
        _yLockedToTop = true;
    }

    void UnlockCameraY()
    {
        _yLockedToTop = false;
    }

    /// <summary>
    /// The camera keeps following the player normally; the top lock shifts the tracked point
    /// (via the tracked object offset) from the player towards the "pinned to top" Y,
    /// eased with a critically damped spring. The tracked point never goes past the confiner,
    /// so the full transition is visible instead of being clipped.
    /// </summary>
    void ApplyTopLockOffset()
    {
        float target = _yLockedToTop ? 1f : 0f;
        float smoothTime = Mathf.Max(0.0001f, _yLockedToTop ? lockSmoothTime : unlockSmoothTime);
        _topLockBlend = Mathf.Clamp01(Mathf.SmoothDamp(
            _topLockBlend, target, ref _topLockBlendVelocity, smoothTime, Mathf.Infinity, Time.deltaTime));

        // Camera center Y where the top of the view touches the top of the confiner
        float lockedY = _topY - vcam.m_Lens.OrthographicSize;
        Vector3 worldOffset = new Vector3(0f, (lockedY - player.position.y) * _topLockBlend, 0f);
        // The framing transposer applies the offset in the follow target's local space
        framing.m_TrackedObjectOffset = Quaternion.Inverse(player.rotation) * worldOffset;
    }
}
