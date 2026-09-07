using Cinemachine;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class RoomCameraController : MonoBehaviour
{
    public RoomCameraType roomType;
    [Header("Vertical Room - Top Lock")]
    public float topLockDistance = 2.5f;
    public float unlockHysteresis = 0.5f;
    public float lockYDampingBoost = 2f;
    public float lockYDampingBoostDuration = 0.25f;
    public float unlockYDampingBoost = 2f;
    public float unlockYDampingBoostDuration = 0.25f;

    private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer framing;
    private CinemachineConfiner2D vcamConfiner;
    private CameraLookaheadController lookaheadController;
    private Transform player;
    private bool _yLockedToTop = false;
    private float _topY;
    private bool _justActivated = false;
    private Coroutine _unlockYDampingCoroutine;
    private float _unlockYDampingPreviousValue;
    private Coroutine _lockYDampingCoroutine;
    private float _lockYDampingPreviousValue;
    private bool _isSmoothLockingToTop = false;
    private bool _requiresVerticalHandling;

    public enum RoomCameraType
    {
        Static,
        Horizontal,
        Vertical,
        HorizontalAndVertical
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

        StopUnlockYDampingBoostAndRestore();
        
        // Disable lookahead controller
        if (lookaheadController != null)
        {
            lookaheadController.Disable();
        }

        gameObject.SetActive(false);
        _yLockedToTop = false;
    }

    public bool IsRoomCameraActivated() {
        return vcam.enabled;
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
        else if (_yLockedToTop)
        {
            // We're locked and should stay locked - ensure dead zone is set
            // But don't override it if we're currently in a smooth transition
            if (!_isSmoothLockingToTop && framing.m_DeadZoneHeight != 999f)
            {
                framing.m_DeadZoneHeight = 999f;
            }
        }
    }

    private void LockCameraToTopImmediate()
    {
        _yLockedToTop = true;

        StopUnlockYDampingBoostAndRestore();

        framing.m_DeadZoneHeight = 999f;

        framing.m_ScreenY = 1.5f;
        
        // Update lookahead controller base screen Y
        if (lookaheadController != null)
        {
            lookaheadController.SetBaseScreenY(1.5f);
            lookaheadController.ResetToBase();
        }

        // Force Cinemachine to forget previous state
        vcam.OnTargetObjectWarped(player, Vector3.zero);
    }

    private void LockCameraToTopSmooth()
    {
        _yLockedToTop = true;
        _isSmoothLockingToTop = true;

        StopUnlockYDampingBoostAndRestore();

        // Keep dead zone at 0 during transition, will be set to 999 after damping completes
        framing.m_DeadZoneHeight = 0f;
        
        // Set Screen Y to target and use damping to smooth the transition
        framing.m_ScreenY = 1.5f;
        
        // Apply damping boost for smooth transition
        if (_lockYDampingCoroutine != null)
        {
            StopCoroutine(_lockYDampingCoroutine);
        }
        _lockYDampingCoroutine = StartCoroutine(ApplyLockYDampingBoost());
        
        // Update lookahead controller base screen Y
        if (lookaheadController != null)
        {
            lookaheadController.SetBaseScreenY(1.5f);
        }
    }

    void UnlockCameraY()
    {
        _yLockedToTop = false;
        _isSmoothLockingToTop = false;

        StopLockYDampingBoostAndRestore();

        framing.m_DeadZoneHeight = 0f;
        framing.m_ScreenY = 0.5f;
        framing.m_ScreenX = 0.5f;
        
        // Update lookahead controller base screen Y
        if (lookaheadController != null)
        {
            lookaheadController.SetBaseScreenY(0.5f);
            lookaheadController.ResetToBase();
        }

        StartUnlockYDampingBoost();
    }

    private void StartUnlockYDampingBoost()
    {
        if (framing == null)
            return;

        StopUnlockYDampingBoostAndRestore();

        _unlockYDampingPreviousValue = framing.m_YDamping;
        _unlockYDampingCoroutine = StartCoroutine(UnlockYDampingBoostRoutine());
    }

    private void StopUnlockYDampingBoostAndRestore()
    {
        if (_unlockYDampingCoroutine == null)
            return;

        StopCoroutine(_unlockYDampingCoroutine);
        _unlockYDampingCoroutine = null;

        if (framing != null)
            framing.m_YDamping = _unlockYDampingPreviousValue;
    }

    private void StopLockYDampingBoostAndRestore()
    {
        if (_lockYDampingCoroutine == null)
            return;

        StopCoroutine(_lockYDampingCoroutine);
        _lockYDampingCoroutine = null;

        if (framing != null)
            framing.m_YDamping = _lockYDampingPreviousValue;
    }

    private IEnumerator UnlockYDampingBoostRoutine()
    {
        if (framing == null)
        {
            _unlockYDampingCoroutine = null;
            yield break;
        }

        framing.m_YDamping = unlockYDampingBoost;

        float t = 0f;
        while (t < unlockYDampingBoostDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (framing != null)
        {
            framing.m_YDamping = _unlockYDampingPreviousValue;
        }

        _unlockYDampingCoroutine = null;
    }

    IEnumerator ApplyLockYDampingBoost()
    {
        if (framing == null)
        {
            _lockYDampingCoroutine = null;
            yield break;
        }

        _lockYDampingPreviousValue = framing.m_YDamping;
        framing.m_YDamping = lockYDampingBoost;

        float t = 0f;
        while (t < lockYDampingBoostDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (framing != null)
        {
            framing.m_YDamping = _lockYDampingPreviousValue;
            
            // Set dead zone to 999 after transition completes to lock the camera
            if (_yLockedToTop)
            {
                framing.m_DeadZoneHeight = 999f;
                _isSmoothLockingToTop = false;
            }
        }

        _lockYDampingCoroutine = null;
    }

}
