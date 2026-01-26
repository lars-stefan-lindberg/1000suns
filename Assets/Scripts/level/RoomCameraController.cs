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
    public float topLockScreenYSmoothTime = 0.2f;
    public float unlockYDampingBoost = 2f;
    public float unlockYDampingBoostDuration = 0.25f;

    private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer framing;
    private CinemachineConfiner2D vcamConfiner;
    private Transform player;
    private bool _yLockedToTop = false;
    private float _topY;
    private Coroutine _screenYSmoothCoroutine;
    private bool _justActivated = false;
    private Coroutine _unlockYDampingCoroutine;
    private float _unlockYDampingPreviousValue;

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
        vcam.Follow = player;
        vcamConfiner.m_BoundingShape2D = confiner;
        CacheConfinerBounds();
        vcamConfiner.InvalidateCache();

        transform.position = new Vector3(spawnPosition.x, spawnPosition.y, transform.position.z);

        ConfigureForRoomType();
        _justActivated = true;
        vcam.enabled = true;
    }

    public void Deactivate() {
        vcam.Follow = null;
        vcam.enabled = false;

        StopUnlockYDampingBoostAndRestore();

        gameObject.SetActive(false);
        _yLockedToTop = false;
    }

    public bool IsRoomCameraActivated() {
        return vcam.enabled;
    }

    void ConfigureForRoomType()
    {
        framing.m_LookaheadTime = 0;
        framing.m_ScreenX = 0.5f;
        framing.m_ScreenY = 0.5f;
        framing.m_TrackedObjectOffset = Vector3.zero;

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

    void LateUpdate()
    {
        if (roomType == RoomCameraType.Vertical || roomType == RoomCameraType.HorizontalAndVertical)
        {
            HandleVerticalTopLock();

            _justActivated = false;
        }
    }

    void HandleVerticalTopLock()
    {
        if (player == null || framing == null)
            return;

        float playerY = player.position.y;
        float distanceToTop = _topY - playerY;

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

    private void LockCameraToTopImmediate()
    {
        _yLockedToTop = true;

        StopUnlockYDampingBoostAndRestore();

        framing.m_DeadZoneHeight = 999f;

        if (_screenYSmoothCoroutine != null)
        {
            StopCoroutine(_screenYSmoothCoroutine);
            _screenYSmoothCoroutine = null;
        }

        framing.m_ScreenY = 1.5f;

        // Force Cinemachine to forget previous state
        vcam.OnTargetObjectWarped(player, Vector3.zero);
    }

    private void LockCameraToTopSmooth()
    {
        _yLockedToTop = true;

        StopUnlockYDampingBoostAndRestore();

        framing.m_DeadZoneHeight = 999f;

        if (_screenYSmoothCoroutine != null)
        {
            StopCoroutine(_screenYSmoothCoroutine);
            _screenYSmoothCoroutine = null;
        }

        _screenYSmoothCoroutine = StartCoroutine(SmoothScreenYTo(1.5f, topLockScreenYSmoothTime));
    }

    void UnlockCameraY()
    {
        _yLockedToTop = false;

        if (_screenYSmoothCoroutine != null)
        {
            StopCoroutine(_screenYSmoothCoroutine);
            _screenYSmoothCoroutine = null;
        }

        framing.m_DeadZoneHeight = 0f;
        framing.m_ScreenY = 0.5f;
        framing.m_ScreenX = 0.5f;

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

    IEnumerator SmoothScreenYTo(float targetScreenY, float duration)
    {
        if (framing == null)
        {
            _screenYSmoothCoroutine = null;
            yield break;
        }

        float start = framing.m_ScreenY;

        if (duration <= 0f)
        {
            framing.m_ScreenY = targetScreenY;
            _screenYSmoothCoroutine = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / duration);
            framing.m_ScreenY = Mathf.Lerp(start, targetScreenY, alpha);
            yield return null;
        }

        framing.m_ScreenY = targetScreenY;
        _screenYSmoothCoroutine = null;
    }
}
