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

    [Header("Pull Box Configuration")]
    [SerializeField] private Vector2 _pullBoxSize = new Vector2(12f, 8f);
    [SerializeField] private Vector2 _pullBoxOffset = Vector2.zero;
    [SerializeField] private float _detectionMargin = 0.5f;
    
    public float pullForce = 10f;
    public float maxPullSpeed = 7f;
    [SerializeField] private float _minColliderGap = 0.05f;
    private float _proximityCheckRadiusSqr = 0f;
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

    [Header("Pull Range Guide")]
    [SerializeField] private SpriteRenderer _pullRangeGuide;
    [SerializeField] private float _pullRangeGuideMaxAlpha = 1f;
    [SerializeField] private float _pullRangeGuideFadeSpeed = 4f;
    [SerializeField] private float _pullRangeGuideMinScale = 0.5f;
    private Coroutine _pullRangeGuideFadeCoroutine;

    [Header("Controllable Object Movement")]
    public float controlledObjectAcceleration = 15f;
    public float controlledObjectMaxSpeed = 5f;
    public float controlledObjectDeceleration = 10f;
    private Vector2 _controlledObjectVelocity = Vector2.zero;
    private float _originalGravityScale = 0f;
    private bool _isControllingObject = false;
    private bool _isOutOfRange = false;

    public bool IsControllingObject => _isControllingObject;
    public Rigidbody2D GetControlledObject() => _targetRb;
    
    public Vector2 GetControlBoxSize() => _pullBoxSize;
    public Vector2 GetHighlightBoxSize() => Vector2.Max(Vector2.zero, _pullBoxSize - Vector2.one * _detectionMargin * 2f);
    public Vector2 GetBoxOffset() => _pullBoxOffset;

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

        // if (_pullRangeGuide != null)
        // {
        //     Color c = _pullRangeGuide.color;
        //     c.a = 0f;
        //     _pullRangeGuide.color = c;
        //     _pullRangeGuide.transform.localScale = new Vector3(_pullRangeGuideMinScale, _pullRangeGuideMinScale, 1f);
        // }
    }
    
    private void OnValidate()
    {
        // Clamp detection margin to valid range
        float minBoxDimension = Mathf.Min(_pullBoxSize.x, _pullBoxSize.y);
        _detectionMargin = Mathf.Clamp(_detectionMargin, 0f, minBoxDimension * 0.5f);
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
        CancelPulling();
        ShadowTwinPlayer.obj.RestorePlayerPullLight();
        ShadowTwinPlayer.obj.EndFullyChargedVfx();
    }

    private void Update()
    {
        // Enable control when player lands while holding a grabbed pullable
        if (_isPullingObject && !_isControllingObject && _pulledPullable != null && ShadowTwinMovement.obj.isGrounded)
        {
            _isControllingObject = true;
        }
        
        // Skip detection if we have a grabbed pullable (whether controlling it or not)
        if (_isPullingObject && _pulledPullable != null)
            return;
        
        // Throttle detection to reduce CPU overhead
        if (Time.time - _lastDetectionTime >= _detectionInterval)
        {
            bool isFacingLeft = ShadowTwinMovement.obj.IsFacingLeft();
            _cachedClosestPullable = _pullableDetector.GetClosestPullable(isFacingLeft);
            _lastDetectionTime = Time.time;
        }
        
        Pullable closestPullable = _cachedClosestPullable;
        
        // Auto-grab: If player is pulling without a grabbed object and a pullable enters range, grab it
        if (_isPullingObject && !_isControllingObject && closestPullable != null && _pulledPullable == null)
        {
            // Ensure the pullable is highlighted before grabbing
            if (closestPullable != _highlightedPullable)
            {
                if (_highlightedPullable != null)
                    _highlightedPullable.StopHighlight();
                closestPullable.StartHighlight();
                _highlightedPullable = closestPullable;
            }
            
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
        _isOutOfRange = false;

        ShadowTwinMovement.obj.IsPulling = false;
        ShadowTwinMovement.obj.UpdateAnimatorIsPulling(false);
        ShadowTwinMovement.obj.EndAnchorPull();
        ShadowTwinPlayer.obj.ResetGravity();
        
        // Restore object's gravity
        if (_targetRb != null)
        {
            _targetRb.gravityScale = _originalGravityScale;
            _controlledObjectVelocity = Vector2.zero;
        }
        
        ResetPullableObject();

        //HidePullRangeGuide();
    }

    private void ResetPullableObject() {
        _targetRb = null;
        if(_pulledPullable != null) {
            _pulledPullable.IsPulled = false;
            _pulledPullable.StopGrabbed();
            
            // Check if the released pullable is still the closest one
            bool isFacingLeft = ShadowTwinMovement.obj.IsFacingLeft();
            Pullable closestPullable = _pullableDetector.GetClosestPullable(isFacingLeft);
            
            // Only fade out outline if it's not the closest pullable anymore
            if (closestPullable != _pulledPullable)
            {
                _pulledPullable.StopHighlight();
            }
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
            
            // Cache a conservative broad-phase radius so we can skip the precise collider
            // Distance check when the pullable is clearly far from the player. Using each
            // collider's bounding diagonal (extents.magnitude) overestimates reach, so the
            // gate never skips a genuinely-close case.
            if (_pulledCollider != null && _collider != null)
            {
                float combinedReach = _collider.bounds.extents.magnitude
                                     + _pulledCollider.bounds.extents.magnitude
                                     + _minColliderGap;
                _proximityCheckRadiusSqr = combinedReach * combinedReach;
            }
            
            // Keep the grabbed pullable as highlighted (outline stays visible)
            _highlightedPullable = pullable;
            
            // Store original gravity and set to 0 for control
            _originalGravityScale = _targetRb.gravityScale;
            _targetRb.gravityScale = 0f;
            _controlledObjectVelocity = Vector2.zero;
            
            // Only enable control if player is grounded
            _isControllingObject = ShadowTwinMovement.obj.isGrounded;

            //ShowPullRangeGuide();
        }
    }

    private void ShowPullRangeGuide()
    {
        // if (_pullRangeGuide == null)
        //     return;

        // if (_pullRangeGuideFadeCoroutine != null)
        //     StopCoroutine(_pullRangeGuideFadeCoroutine);
        // _pullRangeGuideFadeCoroutine = StartCoroutine(FadePullRangeGuide(_pullRangeGuideMaxAlpha));
    }

    private void HidePullRangeGuide()
    {
        // if (_pullRangeGuide == null)
        //     return;

        // if (_pullRangeGuideFadeCoroutine != null)
        //     StopCoroutine(_pullRangeGuideFadeCoroutine);
        // _pullRangeGuideFadeCoroutine = StartCoroutine(FadePullRangeGuide(0f));
    }

    private IEnumerator FadePullRangeGuide(float targetAlpha)
    {
        Color color = _pullRangeGuide.color;
        Transform guideTransform = _pullRangeGuide.transform;
        bool fadingIn = targetAlpha > 0f;
        while (!Mathf.Approximately(color.a, targetAlpha))
        {
            color.a = Mathf.MoveTowards(color.a, targetAlpha, _pullRangeGuideFadeSpeed * Time.deltaTime);
            _pullRangeGuide.color = color;
            // Only scale up while fading in; leave scale untouched while fading out
            if (fadingIn)
                ApplyPullRangeGuideScale(guideTransform, color.a);
            yield return null;
        }
        color.a = targetAlpha;
        _pullRangeGuide.color = color;

        if (fadingIn)
            ApplyPullRangeGuideScale(guideTransform, color.a);
        else
            // After fully fading out, reset scale to minimum (hidden, so no pop visible)
            guideTransform.localScale = new Vector3(_pullRangeGuideMinScale, _pullRangeGuideMinScale, 1f);

        _pullRangeGuideFadeCoroutine = null;
    }

    private void ApplyPullRangeGuideScale(Transform guideTransform, float alpha)
    {
        // float t = _pullRangeGuideMaxAlpha > 0f ? Mathf.Clamp01(alpha / _pullRangeGuideMaxAlpha) : 1f;
        // float scale = Mathf.Lerp(_pullRangeGuideMinScale, 1f, t);
        // guideTransform.localScale = new Vector3(scale, scale, 1f);
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
        Vector2 boxCenter = playerPosition + _pullBoxOffset;
        Vector2 offsetFromPlayer = objectPosition - boxCenter;
        
        // Safety margin to prevent jitter at the boundary
        float safetyMargin = 0.2f;
        Vector2 effectiveBoxHalfSize = (_pullBoxSize - Vector2.one * safetyMargin * 2f) * 0.5f;
        
        // Track if pullable is out of range (outside box bounds), but keep it grabbed
        _isOutOfRange = Mathf.Abs(offsetFromPlayer.x) > effectiveBoxHalfSize.x || 
                        Mathf.Abs(offsetFromPlayer.y) > effectiveBoxHalfSize.y;

        // Get movement input from ShadowTwinMovement
        Vector2 movementInput = ShadowTwinMovement.obj.GetMovementInput();
        
        // Check proximity using actual collider geometry so it adapts to each pullable's size.
        // This lets pullables pass tightly around the player without their colliders touching.
        // Broad-phase gate: skip the precise (costlier) Distance call unless the centers are
        // close enough that the colliders could possibly be within the gap. Computed regardless
        // of input so the no-input deceleration branch can also respect the dead zone.
        Vector2 separationOutward = Vector2.zero;
        bool tooCloseToPlayer = false;
        if (_pulledCollider != null && _collider != null &&
            (objectPosition - playerPosition).sqrMagnitude <= _proximityCheckRadiusSqr)
        {
            ColliderDistance2D sep = _collider.Distance(_pulledCollider);
            tooCloseToPlayer = sep.distance <= _minColliderGap;
            
            // Resolve outward normal (player -> pullable); fix sign using center direction
            separationOutward = sep.normal;
            Vector2 centerDir = (objectPosition - playerPosition).normalized;
            if (Vector2.Dot(separationOutward, centerDir) < 0f)
                separationOutward = -separationOutward;
        }
        
        // Apply acceleration or deceleration based on input
        if (movementInput.magnitude > 0.01f)
        {
            Vector2 targetVelocity = movementInput.normalized * controlledObjectMaxSpeed;
            
            // If out of range, restrict target velocity to prevent moving further away
            if (_isOutOfRange)
            {
                Vector2 restrictedVelocity = targetVelocity;
                
                // Block velocity that would increase distance from box on each axis independently
                if (offsetFromPlayer.x < -effectiveBoxHalfSize.x && targetVelocity.x < 0)
                    restrictedVelocity.x = 0;  // too far left, don't allow further left movement
                if (offsetFromPlayer.x > effectiveBoxHalfSize.x && targetVelocity.x > 0)
                    restrictedVelocity.x = 0;  // too far right, don't allow further right movement
                if (offsetFromPlayer.y < -effectiveBoxHalfSize.y && targetVelocity.y < 0)
                    restrictedVelocity.y = 0;  // too far down, don't allow further down movement
                if (offsetFromPlayer.y > effectiveBoxHalfSize.y && targetVelocity.y > 0)
                    restrictedVelocity.y = 0;  // too far up, don't allow further up movement
                
                targetVelocity = restrictedVelocity;
            }
            Vector2 potentialVelocity = Vector2.MoveTowards(
                _controlledObjectVelocity,
                targetVelocity,
                controlledObjectAcceleration * Time.fixedDeltaTime
            );
            
            // Calculate potential new position
            Vector2 potentialNewPosition = objectPosition + potentialVelocity * Time.fixedDeltaTime;
            Vector2 potentialOffset = potentialNewPosition - boxCenter;

            // Check box constraints (only enforce when NOT out of range, to allow recovery)
            bool exceedsLeft = !_isOutOfRange && potentialOffset.x < -effectiveBoxHalfSize.x;
            bool exceedsRight = !_isOutOfRange && potentialOffset.x > effectiveBoxHalfSize.x;
            bool exceedsTop = !_isOutOfRange && potentialOffset.y > effectiveBoxHalfSize.y;
            bool exceedsBottom = !_isOutOfRange && potentialOffset.y < -effectiveBoxHalfSize.y;
            bool exceedsBox = exceedsLeft || exceedsRight || exceedsTop || exceedsBottom;
            
            if (exceedsBox || tooCloseToPlayer)
            {
                // Movement would violate constraints
                if (exceedsBox)
                {
                    // At box edge - only allow movement that doesn't push further out
                    Vector2 allowedVelocity = potentialVelocity;
                    
                    // Clamp X velocity if at horizontal edges
                    if (exceedsLeft && potentialVelocity.x < 0)
                        allowedVelocity.x = 0;
                    else if (exceedsRight && potentialVelocity.x > 0)
                        allowedVelocity.x = 0;
                    
                    // Clamp Y velocity if at vertical edges
                    if (exceedsBottom && potentialVelocity.y < 0)
                        allowedVelocity.y = 0;
                    else if (exceedsTop && potentialVelocity.y > 0)
                        allowedVelocity.y = 0;
                    
                    _controlledObjectVelocity = allowedVelocity;
                }
                else if (tooCloseToPlayer)
                {
                    // Too close to player - strip only the velocity component pushing the colliders together,
                    // measured along the actual surface separation normal. Tangential movement is preserved,
                    // so the pullable can slide tightly around the player without the colliders touching.
                    float inwardComponent = Vector2.Dot(potentialVelocity, -separationOutward);
                    
                    if (inwardComponent > 0f)
                    {
                        // Remove the inward (toward-player) component, keep tangential motion
                        _controlledObjectVelocity = potentialVelocity + inwardComponent * separationOutward;
                    }
                    else
                    {
                        // Already moving apart or tangential - allow full movement
                        _controlledObjectVelocity = potentialVelocity;
                    }
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
            if (_isOutOfRange)
            {
                // Out of range - apply smooth deceleration but immediately stop movement that would go further away
                Vector2 deceleratedVelocity = Vector2.MoveTowards(
                    _controlledObjectVelocity,
                    Vector2.zero,
                    controlledObjectDeceleration * Time.fixedDeltaTime
                );
                
                // Block any velocity that would increase distance from box on each axis
                if (offsetFromPlayer.x < -effectiveBoxHalfSize.x && deceleratedVelocity.x < 0)
                    deceleratedVelocity.x = 0;  // too far left, stop leftward drift
                if (offsetFromPlayer.x > effectiveBoxHalfSize.x && deceleratedVelocity.x > 0)
                    deceleratedVelocity.x = 0;  // too far right, stop rightward drift
                if (offsetFromPlayer.y < -effectiveBoxHalfSize.y && deceleratedVelocity.y < 0)
                    deceleratedVelocity.y = 0;  // too far down, stop downward drift
                if (offsetFromPlayer.y > effectiveBoxHalfSize.y && deceleratedVelocity.y > 0)
                    deceleratedVelocity.y = 0;  // too far up, stop upward drift
                
                _controlledObjectVelocity = deceleratedVelocity;
            }
            else
            {
                // Check if at box boundary
                bool atBoundary = Mathf.Abs(offsetFromPlayer.x) >= effectiveBoxHalfSize.x || 
                                  Mathf.Abs(offsetFromPlayer.y) >= effectiveBoxHalfSize.y;
                
                if (atBoundary)
                {
                    // At boundary - hard stop
                    _controlledObjectVelocity = Vector2.zero;
                }
                else
                {
                    // Within valid area - normal deceleration
                    Vector2 deceleratedVelocity = Vector2.MoveTowards(
                        _controlledObjectVelocity,
                        Vector2.zero,
                        controlledObjectDeceleration * Time.fixedDeltaTime
                    );
                    
                    // If too close to the player, strip any residual inward component so the
                    // graceful stop can't carry the pullable into the player. Tangential drift
                    // is preserved so it still decelerates naturally along the surface.
                    if (tooCloseToPlayer)
                    {
                        float inwardComponent = Vector2.Dot(deceleratedVelocity, -separationOutward);
                        if (inwardComponent > 0f)
                            deceleratedVelocity += inwardComponent * separationOutward;
                    }
                    
                    _controlledObjectVelocity = deceleratedVelocity;
                }
            }
        }

        // Apply velocity to the rigidbody
        if (_targetRb.bodyType == RigidbodyType2D.Dynamic || _targetRb.bodyType == RigidbodyType2D.Kinematic)
        {
            _targetRb.velocity = _controlledObjectVelocity;
            
            // Sync our internal velocity with actual rigidbody velocity after physics resolution
            // This prevents desync when the rigidbody is blocked by walls/collisions
            // We check this on the next frame by comparing what we set vs what actually happened
            StartCoroutine(SyncVelocityAfterPhysics());
        }
    }
    
    private IEnumerator SyncVelocityAfterPhysics()
    {
        // Wait for physics to resolve
        yield return new WaitForFixedUpdate();
        
        if (_targetRb != null && _isControllingObject)
        {
            // If actual velocity is significantly different from our intended velocity,
            // it means physics blocked the movement (wall collision, etc.)
            Vector2 actualVelocity = _targetRb.velocity;
            float velocityDifference = (_controlledObjectVelocity - actualVelocity).sqrMagnitude;
            float threshold = 0.1f; // Small threshold to account for minor physics variations
            
            if (velocityDifference > threshold)
            {
                // Sync our internal state to match reality
                _controlledObjectVelocity = actualVelocity;
            }
        }
    }

    void Pull()
    {
        CircleCollider2D closestFacingAnchorPoint = null;
        if(_anchorPointDetector.isAnchorPointDetected) {
            closestFacingAnchorPoint = _anchorPointDetector.GetClosestFacingAnchorPoint(transform, ShadowTwinMovement.obj.IsFacingLeft());
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
            // Show the range guide whenever pull starts, even with no pullable grabbed
            //ShowPullRangeGuide();
            pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().HardCancel();
            _deeAudio.PlayForcePullStart(ref _forcePullStartSfxInstance);
            ShadowTwinPlayer.obj.StartChargeFlash();
            ShadowTwinPlayer.obj.PlayerPullLight();
            ShadowTwinPlayer.obj.StartFullyChargedVfx();
            ShadowTwinMovement.obj.IsPulling = true;
            ShadowTwinMovement.obj.UpdateAnimatorIsPulling(true);
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

    private void DrawBoxGizmo(Vector3 center, Vector2 size, Vector2 offset, Color color)
    {
        Gizmos.color = color;
        Vector2 halfSize = size * 0.5f;
        Vector3 actualCenter = center + (Vector3)offset;
        
        Vector3 topLeft = actualCenter + new Vector3(-halfSize.x, halfSize.y, 0);
        Vector3 topRight = actualCenter + new Vector3(halfSize.x, halfSize.y, 0);
        Vector3 bottomRight = actualCenter + new Vector3(halfSize.x, -halfSize.y, 0);
        Vector3 bottomLeft = actualCenter + new Vector3(-halfSize.x, -halfSize.y, 0);
        
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
    
    private void OnDrawGizmosSelected()
    {
        Vector3 playerPos = transform.position;
        
        if (_isControllingObject && _targetRb != null)
        {
            // Draw box constraint when controlling an object
            DrawBoxGizmo(playerPos, _pullBoxSize, _pullBoxOffset, Color.yellow);
            
            // Draw line to controlled object
            Gizmos.color = Color.green;
            Gizmos.DrawLine(playerPos, _targetRb.position);
        }
        else
        {
            // Draw control range box (yellow)
            DrawBoxGizmo(playerPos, _pullBoxSize, _pullBoxOffset, Color.yellow);
            
            // Draw highlight range box (cyan)
            Vector2 highlightBoxSize = GetHighlightBoxSize();
            DrawBoxGizmo(playerPos, highlightBoxSize, _pullBoxOffset, Color.cyan);
            
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
