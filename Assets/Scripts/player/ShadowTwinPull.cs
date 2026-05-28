using System.Collections;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.InputSystem;

/*
Design pattern:
- Use trigger-based detection to find pullables in radius
- Highlight closest pullable with outline effect
- When pull button pressed, grab the highlighted pullable
- Set "isPulled" state on pullable so that the pullable game object can pick up in their script
*/
public class ShadowTwinPull : MonoBehaviour
{
    public static ShadowTwinPull obj;
    private AnchorPointDetector _anchorPointDetector;
    private PullableDetector _pullableDetector;

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

    [Header("Pull Range Configuration")]
    [SerializeField] private float _pullControlRange = 6f;
    [SerializeField] private float _detectionMargin = 0.5f;
    
    public float pullForce = 10f;
    public float maxPullSpeed = 7f;
    public float stopDistanceFromPlayer = 1f;
    public bool HoldPull = false;  //Used in single-player co-op scenarios where we need to hold pull while switching character
    private Rigidbody2D _targetRb;
    private Pullable _pulledPullable;
    private Collider2D _pulledCollider;
    private bool _isPullingObject = false;
    private Pullable _highlightedPullable;
    private DeeAudio _deeAudio;
    
    private float _detectionInterval = 0.1f;
    private float _lastDetectionTime = 0f;
    private Pullable _cachedClosestPullable;

    [Header("Controllable Object Movement")]
    public float controlledObjectAcceleration = 15f;
    public float controlledObjectMaxSpeed = 5f;
    public float controlledObjectDeceleration = 10f;
    private Vector2 _controlledObjectVelocity = Vector2.zero;
    private float _originalGravityScale = 0f;
    private bool _isControllingObject = false;

    public bool IsControllingObject => _isControllingObject;
    public Rigidbody2D GetControlledObject() => _targetRb;
    
    public float GetHighlightRange() => Mathf.Max(0, _pullControlRange - _detectionMargin);
    public float GetControlRange() => _pullControlRange;

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
        _pullableDetector = GetComponentInChildren<PullableDetector>();
        _deeAudio = GetComponent<DeeAudio>();
    }
    
    private void OnValidate()
    {
        // Clamp detection margin to valid range
        _detectionMargin = Mathf.Clamp(_detectionMargin, 0f, _pullControlRange);
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

    private void Update()
    {
        // Enable control when player lands while holding a grabbed pullable
        if (_isPullingObject && !_isControllingObject && _pulledPullable != null && ShadowTwinMovement.obj.isGrounded)
        {
            _isControllingObject = true;
        }
        
        // Only skip detection if actively controlling an object
        if (_isPullingObject && _isControllingObject)
            return;
        
        // Throttle detection to reduce CPU overhead
        if (Time.time - _lastDetectionTime >= _detectionInterval)
        {
            bool isFacingLeft = ShadowTwinMovement.obj.isFacingLeft();
            _cachedClosestPullable = _pullableDetector.GetClosestPullable(isFacingLeft);
            _lastDetectionTime = Time.time;
        }
        
        Pullable closestPullable = _cachedClosestPullable;
        
        // Auto-grab: If player is pulling without a grabbed object and a pullable enters range, grab it
        if (_isPullingObject && !_isControllingObject && closestPullable != null && _pulledPullable == null)
        {
            CameraShakeManager.obj.ForcePushShake();
            SetPullable(closestPullable);
            return;
        }
        
        if (closestPullable != _highlightedPullable)
        {
            if (_highlightedPullable != null)
                _highlightedPullable.StopHighlight();
            
            if (closestPullable != null)
                closestPullable.StartHighlight();
            
            _highlightedPullable = closestPullable;
        }
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

        ShadowTwinMovement.obj.IsPulling = false;
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
        if(_pulledPullable != null) {
            _pulledPullable.IsPulled = false;
            _pulledPullable.StopGrabbed();
        }
        _pulledPullable = null;
        _pulledCollider = null;
    }

    private void SetPullable(Pullable pullable) {
        if(pullable != null) {
            _pulledPullable = pullable;
            _targetRb = _pulledPullable.GetRigidbody();
            _pulledPullable.IsPulled = true;
            _pulledPullable.StartGrabbed();
            _pulledCollider = pullable.GetComponent<Collider2D>();
            
            // Clear highlighted reference since this pullable is now grabbed, not highlighted
            _highlightedPullable = null;
            
            // Store original gravity and set to 0 for control
            _originalGravityScale = _targetRb.gravityScale;
            _targetRb.gravityScale = 0f;
            _controlledObjectVelocity = Vector2.zero;
            
            // Only enable control if player is grounded
            _isControllingObject = ShadowTwinMovement.obj.isGrounded;
        }
    }

    private void FixedUpdate()
    {
        if (!_isPullingObject || !_isControllingObject)
            return;

        if (_targetRb == null || _pulledPullable == null || _pulledPullable.IsHeavy())
            return;
        
        // Only allow control movement when player is grounded
        if (!ShadowTwinMovement.obj.isGrounded)
            return;

        Vector2 playerPosition = transform.position;
        Vector2 objectPosition = _targetRb.position;
        Vector2 offsetFromPlayer = objectPosition - playerPosition;
        float distanceToPlayer = offsetFromPlayer.magnitude;
        
        // Check if pullable is out of range - release it but keep player in pulling state
        if (distanceToPlayer > _pullControlRange)
        {
            _isControllingObject = false;
            
            // Restore object's gravity
            if (_targetRb != null)
            {
                _targetRb.gravityScale = _originalGravityScale;
                _controlledObjectVelocity = Vector2.zero;
            }
            
            ResetPullableObject();
            return;
        }
        
        // Safety margin to prevent drifting beyond range
        float safetyMargin = 0.2f;
        float effectiveMaxRange = _pullControlRange - safetyMargin;

        // Get movement input from ShadowTwinMovement
        Vector2 movementInput = ShadowTwinMovement.obj.GetMovementInput();
        
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

            // Check constraints
            bool exceedsRadius = potentialDistance > effectiveMaxRange;
            bool tooCloseToPlayer = potentialDistance < stopDistanceFromPlayer;
            
            if (exceedsRadius || tooCloseToPlayer)
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
                else if (tooCloseToPlayer)
                {
                    // Too close to player - only allow movement away from player
                    Vector2 directionAwayFromPlayer = offsetFromPlayer.normalized;
                    float dotProduct = Vector2.Dot(movementInput.normalized, directionAwayFromPlayer);
                    
                    if (dotProduct > 0.01f)
                    {
                        // Moving away from player - allow it
                        _controlledObjectVelocity = potentialVelocity;
                    }
                    else
                    {
                        // Moving toward or tangential to player - hard stop
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
            bool atConstraintBoundary = distanceToPlayer >= effectiveMaxRange;
            
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
            //_ghostTrail.ShowGhosts();
            _deeAudio.PlayForcePullStart(ref _forcePullStartSfxInstance);
        } else {
            _isPullingObject = true;
            if(_highlightedPullable != null) {
                CameraShakeManager.obj.ForcePushShake();
                SetPullable(_highlightedPullable);
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
            // Draw circle constraint when controlling an object
            Gizmos.color = Color.yellow;
            Vector3 playerPos = transform.position;
            
            // Draw the full circle
            int segments = 50;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = 2 * Mathf.PI * i / segments; // 0 to 2*PI (360 degrees)
                float angle2 = 2 * Mathf.PI * (i + 1) / segments;
                
                Vector3 point1 = playerPos + new Vector3(Mathf.Cos(angle1) * _pullControlRange, Mathf.Sin(angle1) * _pullControlRange, 0);
                Vector3 point2 = playerPos + new Vector3(Mathf.Cos(angle2) * _pullControlRange, Mathf.Sin(angle2) * _pullControlRange, 0);
                
                Gizmos.DrawLine(point1, point2);
            }
            
            // Draw line to controlled object
            Gizmos.color = Color.green;
            Gizmos.DrawLine(playerPos, _targetRb.position);
        }
        else
        {
            Vector3 playerPos = transform.position;
            int segments = 50;
            
            // Draw control range circle (yellow)
            Gizmos.color = Color.yellow;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = 2 * Mathf.PI * i / segments;
                float angle2 = 2 * Mathf.PI * (i + 1) / segments;
                
                Vector3 point1 = playerPos + new Vector3(Mathf.Cos(angle1) * _pullControlRange, Mathf.Sin(angle1) * _pullControlRange, 0);
                Vector3 point2 = playerPos + new Vector3(Mathf.Cos(angle2) * _pullControlRange, Mathf.Sin(angle2) * _pullControlRange, 0);
                
                Gizmos.DrawLine(point1, point2);
            }
            
            // Draw highlight range circle (cyan)
            Gizmos.color = Color.cyan;
            float highlightRange = GetHighlightRange();
            for (int i = 0; i < segments; i++)
            {
                float angle1 = 2 * Mathf.PI * i / segments;
                float angle2 = 2 * Mathf.PI * (i + 1) / segments;
                
                Vector3 point1 = playerPos + new Vector3(Mathf.Cos(angle1) * highlightRange, Mathf.Sin(angle1) * highlightRange, 0);
                Vector3 point2 = playerPos + new Vector3(Mathf.Cos(angle2) * highlightRange, Mathf.Sin(angle2) * highlightRange, 0);
                
                Gizmos.DrawLine(point1, point2);
            }
            
            // Draw line to highlighted pullable
            if (_highlightedPullable != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(playerPos, _highlightedPullable.transform.position);
            }
        }
    }


    private void OnDestroy()
    {
        obj = null;
    }
}
