using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShadowTwinLash : MonoBehaviour
{
    public static ShadowTwinLash obj;
    private bool _isLashDisabled = false;
    private bool _isShadowLashing = false;
    private bool _lashButtonReleased = false;

    [Header("Lash Pause Configuration")]
    [SerializeField] private float _lashPauseDuration = 0.3f;
    
    [Header("Directional Input Buffer")]
    [SerializeField] private float _directionalInputBufferWindow = 0.1f;
    
    private Vector2 _pausedVelocity;
    private Coroutine _activeDirectionalBufferCoroutine;
    private Coroutine _activeLashCoroutine;

    private void Awake()
    {
        obj = this;
    }

    private void OnDestroy()
    {
        obj = null;
    }

    private void OnDisable()
    {
        // Clean up any running coroutines when component is disabled
        CleanupCoroutines();
    }

    private void CleanupCoroutines()
    {
        // Stop all active coroutines
        if (_activeDirectionalBufferCoroutine != null)
        {
            StopCoroutine(_activeDirectionalBufferCoroutine);
            _activeDirectionalBufferCoroutine = null;
        }

        if (_activeLashCoroutine != null)
        {
            StopCoroutine(_activeLashCoroutine);
            _activeLashCoroutine = null;
        }

        // Reset state
        _isShadowLashing = false;
        _lashButtonReleased = false;

        // Restore gravity if it was disabled
        if (ShadowTwinPlayer.obj != null)
        {
            ShadowTwinPlayer.obj.ResetGravity();
        }
    }

    public void OnLash(InputAction.CallbackContext context)
    {
        if(PlayerPowersManager.obj.DeeCanShadowLash && !_isLashDisabled)
        {
            if (context.performed)
            {
                if(ShadowTwinMovement.obj.IsLatchedToSurface())
                {
                    ShadowTwinMovement.obj.EndLatchPull();
                }
                else
                {
                    // Cancel any existing directional buffer coroutine
                    if (_activeDirectionalBufferCoroutine != null)
                    {
                        StopCoroutine(_activeDirectionalBufferCoroutine);
                    }
                    
                    // Immediately freeze the player for instant response
                    _pausedVelocity = ShadowTwinPlayer.obj.rigidBody.velocity;
                    ShadowTwinMovement.obj.PauseInAir();
                    ShadowTwinPlayer.obj.DisableGravity();
                    _isShadowLashing = true;
                    _lashButtonReleased = false;

                    // Start directional input buffer
                    _activeDirectionalBufferCoroutine = StartCoroutine(DirectionalInputBuffer());
                }
            }
            if(context.canceled)
            {
                OnLashButtonCanceled();
            }
        }
    }

    public void ShootShadowLashBeam() {
        int direction = ShadowTwinMovement.obj.IsFacingLeft() ? -1 : 1;
        ShadowLashBeamManager.obj.ShootBeam(transform.position + new Vector3(0, 0.125f, 0), direction);
    }

    private IEnumerator DirectionalInputBuffer()
    {
        Vector2 initialMovementInput = ShadowTwinMovement.obj.GetMovementInput();
        Vector2 bufferedDirection = Vector2.zero;
        float elapsedTime = 0f;
        
        // Wait for the buffer window, continuously checking for vertical input
        // Note: Player is already frozen at this point for instant response
        while (elapsedTime < _directionalInputBufferWindow)
        {
            Vector2 currentInput = ShadowTwinMovement.obj.GetMovementInput();
            
            // If we detect vertical input during the buffer, prioritize it
            if (currentInput.y != 0)
            {
                bufferedDirection = new Vector2(0, Mathf.Sign(currentInput.y));
                break; // Exit early if vertical input detected
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // After buffer window, determine final latch direction
        Vector2 finalMovementInput = ShadowTwinMovement.obj.GetMovementInput();
        Vector2 latchDirection;
        
        // If we captured vertical input during buffer, use it
        if (bufferedDirection != Vector2.zero)
        {
            latchDirection = bufferedDirection;
        }
        // During wall jump, override horizontal input to prevent lashing back to the wall
        else if (ShadowTwinMovement.obj.IsWallJumping())
        {
            latchDirection = GetLatchDirectionDuringWallJump(finalMovementInput);
        }
        else if (finalMovementInput.x != 0 || finalMovementInput.y != 0)
        {
            // Use movement input direction
            latchDirection = GetLatchDirection(finalMovementInput);
        }
        else
        {
            // No movement input - use facing direction (horizontal)
            bool isFacingLeft = ShadowTwinMovement.obj.IsFacingLeft();
            latchDirection = isFacingLeft ? Vector2.left : Vector2.right;
        }
        
        if (latchDirection != Vector2.zero)
        {
            _activeLashCoroutine = StartCoroutine(PerformLashWithPause(latchDirection, true));
        }
        else
        {
            // No valid direction - unfreeze player
            ShadowTwinPlayer.obj.ResetGravity();
            ShadowTwinPlayer.obj.rigidBody.velocity = _pausedVelocity;
            _isShadowLashing = false;
        }
        
        _activeDirectionalBufferCoroutine = null;
    }

    private IEnumerator PerformLashWithPause(Vector2 latchDirection, bool alreadyFrozen = false)
    {
        // Set facing direction based on lash direction (for horizontal lashes)
        if (latchDirection.x != 0)
        {
            // Face the direction we're lashing
            if (latchDirection.x < 0)
            {
                ShadowTwinMovement.obj.spriteRenderer.flipX = true; // Face left
            }
            else
            {
                ShadowTwinMovement.obj.spriteRenderer.flipX = false; // Face right
            }
        }
        
        // Only freeze if not already frozen (for instant response)
        if (!alreadyFrozen)
        {
            // Start lashing - lock flip player and reset button released flag
            _lashButtonReleased = false;
            
            // Pause the player in the air
            _pausedVelocity = ShadowTwinPlayer.obj.rigidBody.velocity;
            ShadowTwinMovement.obj.PauseInAir();
            ShadowTwinPlayer.obj.DisableGravity();
        }

        // Wait for the pause duration
        yield return new WaitForSeconds(_lashPauseDuration);

        // Unpause the player
        ShadowTwinMovement.obj.UnpauseInAir();

        // Try to latch to surface (even if button was released - we'll drop after latching)
        bool surfaceFound = ShadowTwinMovement.obj.TryLatchToSurface(latchDirection);

        // If no surface was found, restore gravity and velocity and unlock flip
        if (!surfaceFound)
        {
            ShadowTwinMovement.obj.EndLatchPull();
            ShadowTwinPlayer.obj.rigidBody.velocity = _pausedVelocity;
        }

        // Coroutine finished - clear reference
        _activeLashCoroutine = null;
    }

    private Vector2 GetLatchDirection(Vector2 movementInput)
    {
        // Prioritize vertical input over horizontal
        if (movementInput.y != 0)
        {
            return new Vector2(0, Mathf.Sign(movementInput.y));
        }
        else if (movementInput.x != 0)
        {
            return new Vector2(Mathf.Sign(movementInput.x), 0);
        }
        return Vector2.zero;
    }

    private Vector2 GetLatchDirectionDuringWallJump(Vector2 movementInput)
    {
        // During wall jump, vertical input works normally
        if (movementInput.y != 0)
        {
            return new Vector2(0, Mathf.Sign(movementInput.y));
        }
        // For horizontal input, force it to match the wall jump direction
        else if (movementInput.x != 0)
        {
            float wallJumpDir = ShadowTwinMovement.obj.GetWallJumpDirection();
            // Always use the wall jump direction, ignoring the actual input direction
            return new Vector2(Mathf.Sign(wallJumpDir), 0);
        }
        else
        {
            // No input - use wall jump direction
            float wallJumpDir = ShadowTwinMovement.obj.GetWallJumpDirection();
            return new Vector2(Mathf.Sign(wallJumpDir), 0);
        }
    }

    public void OnLashButtonCanceled()
    {
        // Mark that button was released (for dropping after latch if released during pull)
        _lashButtonReleased = true;
        
        // Only cancel if already latched, not if still pulling towards surface
        // If pulling, let it complete and drop after latching (handled in HandleLatchPullVelocity)
        if(ShadowTwinMovement.obj.IsLatchedToSurface())
        {
            ShadowTwinMovement.obj.EndLatchPull();
        }
    }

    public void DisableLash()
    {
        _isLashDisabled = true;
        
        // Clean up any active coroutines when lash is disabled
        CleanupCoroutines();
    }

    public void EnableLash()
    {
        _isLashDisabled = false;
    }

    public bool IsShadowLashing()
    {
        return _isShadowLashing;
    }

    public void SetIsShadowLashing(bool value)
    {
        _isShadowLashing = value;
    }

    public bool WasLashButtonReleased()
    {
        return _lashButtonReleased;
    }
}
