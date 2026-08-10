using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShadowTwinLash : MonoBehaviour
{
    public static ShadowTwinLash obj;
    private bool _isLashDisabled = false;

    [Header("Lash Pause Configuration")]
    [SerializeField] private float _lashPauseDuration = 0.3f;
    
    private Vector2 _pausedVelocity;

    private void Awake()
    {
        obj = this;
    }

    private void OnDestroy()
    {
        obj = null;
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
                    Vector2 movementInput = ShadowTwinMovement.obj.GetMovementInput();
                    Vector2 latchDirection;
                    
                    if (movementInput.x != 0 || movementInput.y != 0)
                    {
                        // Use movement input direction
                        latchDirection = GetLatchDirection(movementInput);
                    }
                    else
                    {
                        // No movement input - use facing direction (horizontal)
                        bool isFacingLeft = ShadowTwinMovement.obj.IsFacingLeft();
                        latchDirection = isFacingLeft ? Vector2.left : Vector2.right;
                    }
                    
                    if (latchDirection != Vector2.zero)
                    {
                        StartCoroutine(PerformLashWithPause(latchDirection));
                    }
                }
            }
            if(context.canceled)
            {
                OnLashButtonCanceled();
            }
        }
    }

    private IEnumerator PerformLashWithPause(Vector2 latchDirection)
    {
        // Pause the player in the air
        _pausedVelocity = ShadowTwinPlayer.obj.rigidBody.velocity;
        ShadowTwinMovement.obj.PauseInAir();
        ShadowTwinPlayer.obj.DisableGravity();

        // Wait for the pause duration
        yield return new WaitForSeconds(_lashPauseDuration);

        // Unpause the player
        ShadowTwinMovement.obj.UnpauseInAir();

        // Try to latch to surface
        bool surfaceFound = ShadowTwinMovement.obj.TryLatchToSurface(latchDirection);

        // If no surface was found, restore gravity and velocity
        if (!surfaceFound)
        {
            ShadowTwinPlayer.obj.ResetGravity();
            ShadowTwinPlayer.obj.rigidBody.velocity = _pausedVelocity;
        }
        // If surface was found, gravity will be handled by the latch pull system
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

    public void OnLashButtonCanceled()
    {
        if(ShadowTwinMovement.obj.IsLatchedToSurface())
        {
            ShadowTwinMovement.obj.EndLatchPull();
        }
    }

    public void DisableLash()
    {
        _isLashDisabled = true;
    }

    public void EnableLash()
    {
        _isLashDisabled = false;
    }
}
