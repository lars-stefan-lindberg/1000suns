using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLookaheadController : MonoBehaviour
{
    [Header("Vertical Lookahead Settings")]
    [SerializeField] private float lookUpOffset = 0.2f;       // How much to offset Screen Y when looking up
    [SerializeField] private float lookDownOffset = 0.2f;     // How much to offset Screen Y when looking down
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float inputDeadzone = 0.1f;
    
    private CinemachineFramingTransposer framing;
    private CinemachineVirtualCamera vcam;
    private Vector2 cameraLookInput;
    private float targetScreenY = 0.5f;
    private float currentScreenY = 0.5f;
    private float screenYVelocity = 0f;
    private float baseScreenY = 0.5f;
    private float lookaheadStartScreenY = 0.5f; // Where we start lookahead from
    private bool isEnabled = false;
    private InputAction cameraLookAction;
    private bool isLookaheadActive = false;
    private bool wasLookaheadActive = false;
    private float customSmoothTime = -1f; // -1 means use default smoothTime
    private bool isInTopLock = false;

    public void Initialize(CinemachineFramingTransposer framingTransposer)
    {
        framing = framingTransposer;
        vcam = GetComponent<CinemachineVirtualCamera>();
        
        if (framing != null)
        {
            baseScreenY = framing.m_ScreenY;
            currentScreenY = baseScreenY;
            targetScreenY = baseScreenY;
        }
        isEnabled = true;
        
        // Subscribe to the CameraLook input action from the active player
        SubscribeToCameraLookInput();
    }

    public void Disable()
    {
        isEnabled = false;
        cameraLookInput = Vector2.zero;
        isLookaheadActive = false;
        if (framing != null)
        {
            targetScreenY = baseScreenY;
        }
        
        // Unsubscribe from input
        UnsubscribeFromCameraLookInput();
    }

    private void SubscribeToCameraLookInput()
    {
        // Get the active player's input
        if (PlayerSwitcher.obj == null)
            return;
        
        PlayerInput activePlayerInput = PlayerSwitcher.obj.GetActivePlayerInput();
        if (activePlayerInput == null)
            return;
        
        if (activePlayerInput.currentActionMap == null)
            return;
        
        cameraLookAction = activePlayerInput.currentActionMap.FindAction("CameraLook");
    }

    private void UnsubscribeFromCameraLookInput()
    {
        cameraLookAction = null;
    }

    public bool IsLookaheadActive()
    {
        return isLookaheadActive;
    }

    void LateUpdate()
    {
        // Don't do anything in LateUpdate - let RoomCameraController call UpdateLookahead manually
        // This ensures proper execution order
    }

    public void ReadInput()
    {
        // Disable lookahead entirely when in top lock
        if (isInTopLock)
        {
            cameraLookInput = Vector2.zero;
            isLookaheadActive = false;
            return;
        }
        
        // Disable lookahead when player is frozen
        if (IsPlayerFrozen())
        {
            cameraLookInput = Vector2.zero;
            isLookaheadActive = false;
            return;
        }
        
        // Poll the input action directly for continuous input
        if (cameraLookAction != null)
        {
            cameraLookInput = cameraLookAction.ReadValue<Vector2>();
        }
        
        // Update lookahead active state based on input magnitude
        float verticalInput = cameraLookInput.y;
        isLookaheadActive = Mathf.Abs(verticalInput) > inputDeadzone;
    }

    public void UpdateLookahead()
    {
        if (!isEnabled || framing == null)
            return;

        // Read input first
        ReadInput();

        // Calculate target Screen Y based on right stick vertical input
        // Stick up (positive) = look up = higher Screen Y value
        // Stick down (negative) = look down = lower Screen Y value
        float verticalInput = cameraLookInput.y;
        
        // When lookahead first becomes active, calculate the effective Screen Y based on actual positions
        if (isLookaheadActive && !wasLookaheadActive)
        {
            // Calculate where the player actually appears on screen
            // This gives us the Screen Y value that represents the current camera-player relationship
            lookaheadStartScreenY = CalculateEffectiveScreenY();
            
            // Set currentScreenY to start from the actual position
            // This ensures smooth transition from where we are to where we want to go
            currentScreenY = lookaheadStartScreenY;
            
            // Base stays at 0.5 so when we release, it returns to center
            baseScreenY = 0.5f;
            
            // Disable player movement while lookahead is active
            if (PlayerManager.obj != null)
            {
                PlayerManager.PlayerType activePlayer = PlayerManager.obj.GetActivePlayerType();
                PlayerManager.obj.DisablePlayerMovement(activePlayer);
            }
        }
        
        // When lookahead stops, re-enable player movement
        if (!isLookaheadActive && wasLookaheadActive)
        {
            if (PlayerManager.obj != null)
            {
                PlayerManager.PlayerType activePlayer = PlayerManager.obj.GetActivePlayerType();
                PlayerManager.obj.EnablePlayerMovement(activePlayer);
            }
        }
        
        wasLookaheadActive = isLookaheadActive;
        
        // Only calculate new target if lookahead is active
        if (isLookaheadActive)
        {
            if (verticalInput > 0)
            {
                // Looking up - offset is always from base (0.5), not from current position
                if (isInTopLock)
                {
                    // Don't allow looking up when in top lock
                    targetScreenY = lookaheadStartScreenY;
                }
                else
                {
                    float offset = lookUpOffset * verticalInput;
                    targetScreenY = Mathf.Clamp(baseScreenY + offset, 0f, 1.5f);
                }
            }
            else if (verticalInput < 0)
            {
                // Looking down - offset is always from base (0.5), not from current position
                float offset = lookDownOffset * Mathf.Abs(verticalInput);
                targetScreenY = Mathf.Clamp(baseScreenY - offset, 0f, 1.5f);
            }
            else
            {
                // No input, return to base (0.5)
                targetScreenY = baseScreenY;
            }
        }
        else
        {
            // Lookahead not active - always target base (0.5)
            targetScreenY = baseScreenY;
        }

        // Smoothly interpolate to target using custom smooth time if set, otherwise use default
        float activeSmoothTime = customSmoothTime > 0 ? customSmoothTime : smoothTime;
        currentScreenY = Mathf.SmoothDamp(currentScreenY, targetScreenY, ref screenYVelocity, activeSmoothTime);
        
        // Reset custom smooth time once we've reached the target
        if (customSmoothTime > 0 && Mathf.Abs(currentScreenY - targetScreenY) < 0.01f)
        {
            customSmoothTime = -1f;
        }
        
        framing.m_ScreenY = currentScreenY;
    }

    public void SetBaseScreenY(float screenY, bool forceUpdateCurrent = false)
    {
        baseScreenY = screenY;
        if (!isEnabled || forceUpdateCurrent)
        {
            currentScreenY = screenY;
            targetScreenY = screenY;
            screenYVelocity = 0f;
        }
    }

    public void SetBaseScreenYSmooth(float screenY, float customSmoothTimeValue = -1f)
    {
        baseScreenY = screenY;
        targetScreenY = screenY;
        customSmoothTime = customSmoothTimeValue;
    }

    public void ResetToBase()
    {
        if (framing != null)
        {
            currentScreenY = baseScreenY;
            targetScreenY = baseScreenY;
            framing.m_ScreenY = baseScreenY;
            screenYVelocity = 0f;
        }
    }

    public void SetInTopLock(bool inTopLock)
    {
        isInTopLock = inTopLock;
    }

    private bool IsPlayerFrozen()
    {
        PlayerManager.PlayerType playerType = PlayerManager.obj.GetActivePlayerType();
        return PlayerManager.obj != null && PlayerManager.obj.IsFrozen(playerType);
    }

    // Calculate the Screen Y value based on the distance between camera and player
    // If player is below camera center (viewport Y < 0.5), we add the distance to 0.5
    // If player is above camera center (viewport Y > 0.5), we subtract the distance from 0.5
    // This gives us a starting Screen Y that will make the camera react immediately
    private float CalculateEffectiveScreenY()
    {
        if (vcam == null || framing == null)
            return 0.5f;

        Transform followTarget = vcam.Follow;
        if (followTarget == null)
            return 0.5f;

        // Get the actual camera position (not the virtual camera, but the real Unity camera)
        Camera mainCam = Camera.main;
        if (mainCam == null)
            return 0.5f;

        // Convert player world position to viewport position (0-1 range)
        Vector3 viewportPos = mainCam.WorldToViewportPoint(followTarget.position);
        
        // Calculate offset from center (0.5)
        // Positive if player is above center, negative if below
        float offsetFromCenter = viewportPos.y - 0.5f;
        
        // Starting Screen Y = 0.5 - offset
        // If player is below (offset negative), we add to 0.5 (higher Screen Y)
        // If player is above (offset positive), we subtract from 0.5 (lower Screen Y)
        return 0.5f - offsetFromCenter;
    }

    void OnDestroy()
    {
        UnsubscribeFromCameraLookInput();
    }
}
