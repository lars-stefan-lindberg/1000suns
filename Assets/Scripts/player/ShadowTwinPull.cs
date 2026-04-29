using System.Collections;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.InputSystem;

/*
Design pattern:
- When pulling, shoot out rays to detect "pullable"
- If a pullable is hit, save the pullable rigidbody and apply force to it
- Set "isPulled" state on pullable so that the pullable game object can pick up in their script
- Constantly detect if the pullable is still in line of fire. If not, cancel pulling
*/
public class ShadowTwinPull : MonoBehaviour
{
    public static ShadowTwinPull obj;
    private AnchorPointDetector _anchorPointDetector;

    private BoxCollider2D _collider;
    [SerializeField] private GhostTrailManager _ghostTrail;

    public float minBuildUpPowerTime = 0.3f;

    private float _playerOffset = 0.5f;
    public float maxForce = 3;
    public float powerUpMaxForce = 4;
    public float powerBuildUpPerFixedUpdate = 1.2f;

    public float defaultPower = 1;

    [Header("Dependencies")]
    public GameObject pushPowerUpAnimation;

    private float _buildUpPower = 0;
    private bool _buildingUpPower = false;
    private float _buildUpPowerTime = 0;
    
    public FloatyPlatform platform;

    private EventInstance _forcePullStartSfxInstance;

    private bool _isPullDisabled = false;

    public float pullForce = 10f;
    public float maxPullSpeed = 7f;
    public float stopDistanceFromPlayer = 1f;
    public bool HoldPull = false;  //Used in single-player co-op scenarios where we need to hold pull while switching character
    private Rigidbody2D _targetRb;
    private Pullable _pulledPullable;
    private Collider2D _pulledCollider;
    private bool _isPullingObject = false;
    public float pullRange = 6f;
    public LayerMask pullableMask;
    public LayerMask blockingMask;
    public float raySpacing = 0.2f; // space between the rays
    private DeeAudio _deeAudio;

    [Header("Controllable Object Movement")]
    public float controlledObjectAcceleration = 15f;
    public float controlledObjectMaxSpeed = 5f;
    public float controlledObjectDeceleration = 10f;
    private Vector2 _controlledObjectVelocity = Vector2.zero;
    private float _originalGravityScale = 0f;
    private bool _isControllingObject = false;

    public bool IsControllingObject => _isControllingObject;
    public Rigidbody2D GetControlledObject() => _targetRb;

    public enum PullPowerType {
        Full,
        Powered
    }

    public enum PullType {
        None,
        Pullable
    }

    public struct PullDetectionResult
    {
        public Collider2D collider;
        public PullType pullType;
        public float distance;
        
        public static readonly PullDetectionResult None = new PullDetectionResult { collider = null, pullType = PullType.None, distance = float.MaxValue };
        
        public bool HasHit => collider != null;
        
        public static bool operator >(PullDetectionResult a, PullDetectionResult b) => a.distance > b.distance;
        public static bool operator <(PullDetectionResult a, PullDetectionResult b) => a.distance < b.distance;
    }

    private void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
        _anchorPointDetector = GetComponentInChildren<AnchorPointDetector>();
        _deeAudio = GetComponent<DeeAudio>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if(PlayerPowersManager.obj.DeeCanForcePull && !_isPullDisabled) {
            if (context.performed)
            {                
                Pull();
            }
            if(context.canceled) {
                OnShootButtonCanceled();
            }
        }
    }

    public void OnShootButtonCanceled() {
        if(HoldPull)
            return;
        if(_isPullingObject)
            ShadowTwinMovement.obj.TriggerEndForcePullAnimation();
        CancelPulling();
        ShadowTwinPlayer.obj.RestorePlayerPullLight();
    }

    private PullDetectionResult DetectPullable()
    {
        Vector2 direction = new Vector2(ShadowTwinMovement.obj.isFacingLeft() ? -1 : 1, 0f);

        Vector2 origin1 = (Vector2)transform.position + new Vector2(0,  raySpacing);
        Vector2 origin2 = (Vector2)transform.position + new Vector2(0, -raySpacing);
        Vector2 origin3 = (Vector2)transform.position + new Vector2(0, 0);

        // Cast rays for pullable objects
        RaycastHit2D pullableHit1 = Physics2D.Raycast(origin1, direction, pullRange, pullableMask);
        RaycastHit2D pullableHit2 = Physics2D.Raycast(origin2, direction, pullRange, pullableMask);
        RaycastHit2D pullableHit3 = Physics2D.Raycast(origin3, direction, pullRange, pullableMask);


        // Check if any hit is too close to the player
        if ((pullableHit1.collider != null && pullableHit1.distance < 1f) ||
            (pullableHit2.collider != null && pullableHit2.distance < 1f) ||
            (pullableHit3.collider != null && pullableHit3.distance < 1f))
        {
            return PullDetectionResult.None;
        }

        // Check blocking for pullable hits
        if (IsBlocked(origin1, direction, pullableHit1)) pullableHit1 = new RaycastHit2D();
        if (IsBlocked(origin2, direction, pullableHit2)) pullableHit2 = new RaycastHit2D();
        if (IsBlocked(origin3, direction, pullableHit3)) pullableHit3 = new RaycastHit2D();

        // Find closest pullable hit
        PullDetectionResult closestPullable = PullDetectionResult.None;
        if (pullableHit1.collider != null && pullableHit1.distance < closestPullable.distance)
            closestPullable = new PullDetectionResult { collider = pullableHit1.collider, pullType = PullType.Pullable, distance = pullableHit1.distance };
        if (pullableHit2.collider != null && pullableHit2.distance < closestPullable.distance)
            closestPullable = new PullDetectionResult { collider = pullableHit2.collider, pullType = PullType.Pullable, distance = pullableHit2.distance };
        if (pullableHit3.collider != null && pullableHit3.distance < closestPullable.distance)
            closestPullable = new PullDetectionResult { collider = pullableHit3.collider, pullType = PullType.Pullable, distance = pullableHit3.distance };

        return closestPullable;
    }

    private bool IsBlocked(Vector2 origin, Vector2 direction, RaycastHit2D targetHit)
    {
        if (!targetHit.collider) return false;

        RaycastHit2D blockCheck = Physics2D.Raycast(
            origin, direction, targetHit.distance, blockingMask
        );

        return blockCheck.collider != null; // true = blocked
    }

    public void DisablePull() {
        _isPullDisabled = true;
    }

    public void EnablePull() {
        _isPullDisabled = false;
    }

    public void DisablePullFor(float duration) {
        _isPullDisabled = true;
        StartCoroutine(EnablePullAfterDelay(duration));
    }

    private IEnumerator EnablePullAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        _isPullDisabled = false;
    }

    public void ResetBuiltUpPower() {
        pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().Cancel();
        Player.obj.AbortFlash();
        Player.obj.EndFullyChargedVfx();

        if(AudioUtils.IsPlaying(_forcePullStartSfxInstance))
            AudioUtils.SafeStop(ref _forcePullStartSfxInstance);

        ShadowTwinPlayer.obj.RestorePlayerPullLight();

        _buildingUpPower = false;
        _buildUpPower = defaultPower;
        _buildUpPowerTime = 0;
        Player.obj.EndChargeFlash();
    }

    public void CancelPulling() {
        _isPullingObject = false;
        _isControllingObject = false;

        ShadowTwinMovement.obj.EndAnchorPull();
        ShadowTwinPlayer.obj.ResetGravity();
        
        // Restore object's gravity
        if (_targetRb != null)
        {
            _targetRb.gravityScale = _originalGravityScale;
            _controlledObjectVelocity = Vector2.zero;
        }
        
        ResetPullableObject();
    }

    private void ResetPullableObject() {
        _targetRb = null;
        if(_pulledPullable != null)
            _pulledPullable.IsPulled = false;
        _pulledPullable = null;
        _pulledCollider = null;
    }

    private void SetPullable(Collider2D pullableCollider) {
        if(pullableCollider != null) {
            _pulledPullable = pullableCollider.GetComponent<Pullable>();
            _targetRb = _pulledPullable.GetRigidbody();
            _pulledPullable.IsPulled = true;
            _pulledCollider = pullableCollider;
            
            // Store original gravity and set to 0 for control
            _originalGravityScale = _targetRb.gravityScale;
            _targetRb.gravityScale = 0f;
            _controlledObjectVelocity = Vector2.zero;
            _isControllingObject = true;
        }
    }

    private void Update()
    {
        if (_isPullingObject && !_isControllingObject)
        {
            // Only detect pullables when not already controlling one
            PullDetectionResult pullable = DetectPullable();
            if(pullable.collider == null) {
                ResetPullableObject();
            } else {
                if (pullable.collider != _pulledCollider)
                {
                    // New pullable detected. Start pulling that pullable instead
                    ResetPullableObject();
                    SetPullable(pullable.collider);
                }
            } 
        }
        // When controlling an object, we don't need ray detection - use radius constraint instead
    }

    private void FixedUpdate()
    {
        if (!_isPullingObject || !_isControllingObject)
            return;

        if (_targetRb == null || _pulledPullable == null || _pulledPullable.IsHeavy())
            return;

        Vector2 playerPosition = transform.position;
        Vector2 objectPosition = _targetRb.position;
        Vector2 offsetFromPlayer = objectPosition - playerPosition;
        float distanceToPlayer = offsetFromPlayer.magnitude;
        
        // Safety margin to prevent drifting beyond range
        float safetyMargin = 0.2f;
        float effectiveMaxRange = pullRange - safetyMargin;

        // Get movement input from ShadowTwinMovement
        Vector2 movementInput = ShadowTwinMovement.obj.GetMovementInput();

        // Check if object is in valid semicircle (top half)
        bool isInTopHalf = offsetFromPlayer.y >= 0f;
        
        // Apply acceleration or deceleration based on input
        if (movementInput.magnitude > 0.01f)
        {
            Vector2 targetVelocity = movementInput.normalized * controlledObjectMaxSpeed;
            Vector2 potentialVelocity = Vector2.MoveTowards(
                _controlledObjectVelocity,
                targetVelocity,
                controlledObjectAcceleration * Time.fixedDeltaTime
            );
            
            // Calculate potential new position
            Vector2 potentialNewPosition = objectPosition + potentialVelocity * Time.fixedDeltaTime;
            Vector2 potentialOffset = potentialNewPosition - playerPosition;
            float potentialDistance = potentialOffset.magnitude;
            bool potentialInTopHalf = potentialOffset.y >= 0f;

            // Check constraints
            bool exceedsRadius = potentialDistance > effectiveMaxRange;
            bool leavesTopHalf = isInTopHalf && !potentialInTopHalf;
            
            if (exceedsRadius || leavesTopHalf)
            {
                // Movement would violate constraints
                if (exceedsRadius && distanceToPlayer >= effectiveMaxRange)
                {
                    // At max radius - only allow movement toward player or tangential
                    Vector2 directionToPlayer = offsetFromPlayer.normalized;
                    float dotProduct = Vector2.Dot(movementInput.normalized, directionToPlayer);
                    
                    if (dotProduct > 0.01f)
                    {
                        // Moving toward player - allow it
                        _controlledObjectVelocity = potentialVelocity;
                    }
                    else
                    {
                        // Moving away - hard stop
                        _controlledObjectVelocity = Vector2.zero;
                    }
                }
                else if (leavesTopHalf)
                {
                    // Trying to go below player - only allow horizontal/upward movement
                    if (movementInput.y >= -0.01f)
                    {
                        // Allow horizontal or upward movement
                        _controlledObjectVelocity = potentialVelocity;
                    }
                    else
                    {
                        // Trying to move down - hard stop
                        _controlledObjectVelocity = Vector2.zero;
                    }
                }
                else
                {
                    // Would exceed radius - hard stop
                    _controlledObjectVelocity = Vector2.zero;
                }
            }
            else
            {
                // Normal movement within constraints
                _controlledObjectVelocity = potentialVelocity;
            }
        }
        else
        {
            // No input - decelerate
            bool atConstraintBoundary = distanceToPlayer >= effectiveMaxRange || !isInTopHalf;
            
            if (atConstraintBoundary)
            {
                // At boundary - hard stop
                _controlledObjectVelocity = Vector2.zero;
            }
            else
            {
                // Within valid area - normal deceleration
                _controlledObjectVelocity = Vector2.MoveTowards(
                    _controlledObjectVelocity,
                    Vector2.zero,
                    controlledObjectDeceleration * Time.fixedDeltaTime
                );
            }
        }

        // Apply velocity to the rigidbody
        if (_targetRb.bodyType == RigidbodyType2D.Dynamic || _targetRb.bodyType == RigidbodyType2D.Kinematic)
        {
            _targetRb.velocity = _controlledObjectVelocity;
        }
    }

    void Pull()
    {
        CircleCollider2D closestFacingAnchorPoint = null;
        if(_anchorPointDetector.isAnchorPointDetected) {
            closestFacingAnchorPoint = _anchorPointDetector.GetClosestFacingAnchorPoint(transform, ShadowTwinMovement.obj.isFacingLeft());
        }
        if(closestFacingAnchorPoint != null && !ShadowTwinMovement.obj.isGrounded) {
            Vector3 anchorPosition = closestFacingAnchorPoint.bounds.center;
            anchorPosition.y -= closestFacingAnchorPoint.bounds.extents.y;
            
            ShadowTwinMovement.obj.anchorPosition = anchorPosition;
            ShadowTwinMovement.obj.StartAnchorPull();
            ShadowTwinPlayer.obj.DisableGravity();
            _ghostTrail.ShowGhosts();
            _deeAudio.PlayForcePullStart(ref _forcePullStartSfxInstance);
        } else {
            _isPullingObject = true;
            PullDetectionResult pullable = DetectPullable();
            if(pullable.collider != null) {
                SetPullable(pullable.collider);
            }
            pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().HardCancel();
            _deeAudio.PlayForcePullStart(ref _forcePullStartSfxInstance);
            ShadowTwinPlayer.obj.StartChargeFlash();
            ShadowTwinPlayer.obj.PlayerPullLight();
            ShadowTwinMovement.obj.IsPulling = true;
            ShadowTwinMovement.obj.TriggerForcePullAnimation();
            PullPowerType chargePowerType = GetChargePowerType();
            ExecuteForcePushVfx(chargePowerType);
        }
    }

    private PullPowerType GetChargePowerType() {
        if(ShadowTwinPlayer.obj.hasPowerUp) 
            return PullPowerType.Powered;
        else
            return PullPowerType.Full;
    }

    public void ExecuteDashVfx(PullPowerType chargePower) {
        if(chargePower == PullPowerType.Powered || chargePower == PullPowerType.Full) {    
            ShockWaveManager.obj.CallShockWave(_collider.bounds.center, 0.2f, 0.05f, 0.15f);
            CameraShakeManager.obj.ForcePushShake();
        }
        Player.obj.ForcePushFlash();
    }

    public void ExecuteForcePushVfx(PullPowerType chargePowerType) {
        ShadowTwinPlayer.obj.ForcePushFlash();
    }

    private void OnDrawGizmosSelected()
    {
        if (_isControllingObject && _targetRb != null)
        {
            // Draw semicircle constraint when controlling an object
            Gizmos.color = Color.yellow;
            Vector3 playerPos = transform.position;
            
            // Draw the semicircle arc (top half)
            int segments = 30;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = Mathf.PI * i / segments; // 0 to PI (180 degrees)
                float angle2 = Mathf.PI * (i + 1) / segments;
                
                Vector3 point1 = playerPos + new Vector3(Mathf.Cos(angle1) * pullRange, Mathf.Sin(angle1) * pullRange, 0);
                Vector3 point2 = playerPos + new Vector3(Mathf.Cos(angle2) * pullRange, Mathf.Sin(angle2) * pullRange, 0);
                
                Gizmos.DrawLine(point1, point2);
            }
            
            // Draw the base line (horizontal through player)
            Gizmos.DrawLine(playerPos + Vector3.left * pullRange, playerPos + Vector3.right * pullRange);
            
            // Draw line to controlled object
            Gizmos.color = Color.green;
            Gizmos.DrawLine(playerPos, _targetRb.position);
        }
        else
        {
            // Draw detection rays when not controlling
            Vector2 direction = new Vector2(-1, 0f);

            Vector2 origin1 = (Vector2)transform.position + new Vector2(0,  raySpacing);
            Vector2 origin2 = (Vector2)transform.position + new Vector2(0, 0);
            Vector2 origin3 = (Vector2)transform.position + new Vector2(0, -raySpacing);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin1, origin1 + direction * pullRange);
            Gizmos.DrawLine(origin2, origin2 + direction * pullRange);
            Gizmos.DrawLine(origin3, origin3 + direction * pullRange);
        }
    }


    private void OnDestroy()
    {
        obj = null;
    }
}
