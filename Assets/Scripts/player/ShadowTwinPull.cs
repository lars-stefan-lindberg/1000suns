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
    private PullableDetector _pullableDetector;

    private BoxCollider2D _collider;
    [SerializeField] private GhostTrailManager _ghostTrail;

    public float minBuildUpPowerTime = 0.3f;

    public float maxForce = 3;
    public float powerUpMaxForce = 4;
    public float powerBuildUpPerFixedUpdate = 1.2f;

    public float defaultPower = 1;

    [Header("Dependencies")]
    public GameObject pushPowerUpAnimation;

    private float _buildUpPower = 0;
    
    public FloatyPlatform platform;

    private EventInstance _forcePullStartSfxInstance;
    private EventInstance _shadowPullObjectMovingLoopInstance;
    private EventInstance _shadowPullLoopInstance;

    private bool _isPullDisabled = false;
    private bool _isObectMovingLoopPlaying = false;

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
    private Coroutine _pullRangeGuideFadeCoroutine;
    
    public enum PullRangeGuideMode
    {
        Off,
        AlwaysOn,
        OnlyOnGrab
    }
    
    private PullRangeGuideMode _pullRangeGuideMode = PullRangeGuideMode.Off;

    [Header("Controllable Object Movement")]
    public float controlledObjectAcceleration = 15f;
    public float controlledObjectMaxSpeed = 5f;
    public float controlledObjectDeceleration = 10f;
    // Steering velocity of the held object. X is relative to Dee (a lagged copy of her horizontal velocity is added
    // on top so the object follows her sideways). Y is world space, so Dee jumping or falling doesn't move the object vertically.
    private Vector2 _controlledObjectVelocity = Vector2.zero;
    // World-space vertical velocity the object gets while Dee drags it along at the vertical range edge
    private float _verticalFollowVelocity = 0f;
    // Velocity set on the object last physics step, used to detect when geometry blocked it
    private Vector2 _lastAppliedObjectVelocity = Vector2.zero;
    private float _originalGravityScale = 0f;
    private bool _isControllingObject = false;
    private bool _isOutOfRange = false;
    [Tooltip("How far past the pull box edge the held object can end up before the grip is released. Applies horizontally, and vertically only when geometry blocks the object.")]
    [SerializeField] private float _releaseDistanceBeyondRange = 0.25f;
    [Tooltip("How quickly the held object speeds up when following Dee vertically at the range edge (units/s²). Keep it below Dee's fall acceleration so the object eases into the fall instead of dropping in sync with her.")]
    [SerializeField] private float _verticalFollowAcceleration = 35f;
    [Tooltip("How quickly the vertical follow slows down again, e.g. when Dee lands (units/s²).")]
    [SerializeField] private float _verticalFollowDeceleration = 80f;
    [Tooltip("How strongly an object that lagged past the vertical range edge is pulled back to it (1/s).")]
    [SerializeField] private float _verticalFollowCatchUpRate = 8f;
    private bool _isVerticallyBlocked = false;

    [Header("Horizontal Follow Lag")]
    [Tooltip("How far the held object trails behind while Dee runs, as seconds of her speed (0.1 at a run speed of 9 trails by 0.9 units).")]
    [SerializeField] private float _horizontalLagTime = 0.1f;
    [Tooltip("How quickly the held object speeds up to follow Dee sideways (units/s²). Lower values make it trail further behind when she starts running.")]
    [SerializeField] private float _horizontalFollowAcceleration = 35f;
    [Tooltip("How quickly the sideways follow slows down, e.g. when Dee stops and the object catches up (units/s²).")]
    [SerializeField] private float _horizontalFollowDeceleration = 60f;
    [Tooltip("How strongly a trailing object catches back up to its position relative to Dee (1/s).")]
    [SerializeField] private float _horizontalFollowCatchUpRate = 6f;
    [Tooltip("The furthest the held object may trail behind its position relative to Dee (units).")]
    [SerializeField] private float _maxHorizontalLag = 1.25f;
    // World-space horizontal velocity the object follows Dee with (steering is added on top)
    private float _horizontalFollowVelocity = 0f;
    // How far the object trails its resting position relative to Dee (positive = to the right of it)
    private float _horizontalLag = 0f;

    [Header("Object Steering Input")]
    [SerializeField] private float _mouseAimSensitivity = 1f;
    [SerializeField] private float _mouseAimSteeringGain = 6f;
    [SerializeField] private SpriteRenderer _mouseAimIndicator;

    private const string SHADOW_GRIP_AIM_ACTION_NAME = "ShadowGripAim";
    private const string SHADOW_GRIP_ACTION_NAME = "ShadowGrip";
    private const string SHADOW_GRIP_POINTER_ACTION_NAME = "ShadowGripPointer";
    private const string KEYBOARD_CONTROL_SCHEME_NAME = "Keyboard";
    private const float STICK_AIM_DEAD_ZONE = 0.15f;
    private const float MOUSE_AIM_ARRIVAL_DISTANCE = 0.05f;
    private const float FALLBACK_WORLD_UNITS_PER_PIXEL = 0.01f;
    private const float RANGE_SAFETY_MARGIN = 0.2f;
    private const float MOVING_VELOCITY_EPSILON = 0.01f;
    private const float VELOCITY_SYNC_THRESHOLD = 0.3f;
    private const float VERTICAL_EDGE_TOLERANCE = 0.05f;

    private PlayerInput _playerInput;
    private InputAction _shadowGripAimAction;
    private InputAction _shadowGripPointerAction;
    // Mouse aim point: X is relative to the pull box center (follows Dee sideways), Y is world space
    // (only dragged along vertically when Dee moves far enough to push it to the range edge)
    private float _mouseAimOffsetX = 0f;
    private float _mouseAimWorldY = 0f;
    private bool _wasIdleGroundedWhilePulling = false;
    //Set when the grip was started by an automatic switch, so the button release has to be polled
    private bool _pollShadowGripButtonRelease = false;

    public bool IsControllingObject => _isControllingObject;

    /// <summary>True for the whole duration of a shadow grip, whether or not an object is held.</summary>
    public bool IsGripping => _isPullingObject;
    public Rigidbody2D GetControlledObject() => _targetRb;

    /// <summary>Collider of the object Dee is currently steering, or null when she isn't holding one.</summary>
    public Collider2D HeldCollider => _isControllingObject ? _pulledCollider : null;
    
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
        _pullableDetector = GetComponentInChildren<PullableDetector>();
        _deeAudio = GetComponent<DeeAudio>();
        _playerInput = GetComponent<PlayerInput>();
        SetMouseAimIndicatorVisible(false);

        if (_pullRangeGuide != null)
        {
            Color c = _pullRangeGuide.color;
            c.a = 0f;
            _pullRangeGuide.color = c;
        }
    }
    
    private void OnValidate()
    {
        // Clamp detection margin to valid range
        float minBoxDimension = Mathf.Min(_pullBoxSize.x, _pullBoxSize.y);
        _detectionMargin = Mathf.Clamp(_detectionMargin, 0f, minBoxDimension * 0.5f);
    }

    /// <summary>Input callback for Dee's ShadowGrip action (wired from PlayerInput events).</summary>
    public void OnShadowGrip(InputAction.CallbackContext context)
    {
        //After an automatic switch the held button (e.g. an analog trigger) can still send started/performed.
        //The grip is already running from that press, so keep polling and only react to the release
        if (_pollShadowGripButtonRelease && !context.canceled)
            return;
        //A real input event takes over from the polling started by an automatic switch
        _pollShadowGripButtonRelease = false;
        // Prevent shadow pull during shadow lash
        if (ShadowTwinLash.obj != null && ShadowTwinLash.obj.IsShadowLashing())
        {
            return;
        }
        if(PlayerPowersManager.obj.DeeCanForcePull && !_isPullDisabled) {
            if (context.performed)
            {
                Pull();
            }
            if(context.canceled) {
                OnShadowGripButtonReleased();
            }
        }
    }

    /// <summary>True if Dee is allowed to start a shadow grip right now.</summary>
    public bool CanStartGrip() {
        bool isLashing = ShadowTwinLash.obj != null && ShadowTwinLash.obj.IsShadowLashing();
        return PlayerPowersManager.obj.DeeCanForcePull && !_isPullDisabled && !isLashing && !_isPullingObject;
    }

    /// <summary>
    /// Starts a shadow grip after an automatic switch from Eli, where the ShadowGrip button is already held.
    /// The release is polled, since a button held when the PlayerInput got enabled never sends canceled.
    /// </summary>
    public void StartGripFromSwitch() {
        if(!CanStartGrip())
            return;
        //Highlighting doesn't run while Dee is inactive. Force a fresh detection next frame so the existing auto-grab picks up a pullable in range right away
        _lastDetectionTime = float.MinValue;
        Pull();
        _pollShadowGripButtonRelease = true;
    }

    /// <summary>Ends the grip when the button is released, unless the grip is held for a character switch.</summary>
    public void OnShadowGripButtonReleased() {
        if(HoldPull)
            return;
        CancelPulling();
        ShadowTwinPlayer.obj.RestorePlayerPullLight();
        ShadowTwinPlayer.obj.EndFullyChargedVfx();
    }

    /// <summary>Force-ends an active grip (lash, room transition, landing on the held object), ignoring HoldPull.</summary>
    public void ReleaseGrip() {
        if (!_isPullingObject)
            return;
        HoldPull = false;
        OnShadowGripButtonReleased();
    }

    private void Update()
    {
        if (_pollShadowGripButtonRelease && !PlayerManager.IsActionPressed(_playerInput, SHADOW_GRIP_ACTION_NAME))
        {
            _pollShadowGripButtonRelease = false;
            OnShadowGripButtonReleased();
        }

        // Check if currently pulled pullable became immune - release it immediately
        if (_isPullingObject && _pulledPullable != null && _pulledPullable.IsImmune)
        {
            OnShadowGripButtonReleased();
            return;
        }

        UpdatePullStartAnimationReplay();
        UpdateMouseAim();
        
        // Skip detection if we have a grabbed pullable (whether controlling it or not)
        if (_isPullingObject && _pulledPullable != null)
            return;
        
        if (!IsDeeControlled())
        {
            ClearHighlight();
            return;
        }
        
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

    private void OnDisable()
    {
        ClearHighlight();
    }

    private bool IsDeeControlled()
    {
        return PlayerManager.obj == null || PlayerManager.obj.IsCoopActive
            || PlayerSwitcher.obj == null || PlayerSwitcher.obj.IsDeeActive();
    }

    private void ClearHighlight()
    {
        if (_highlightedPullable != null && _highlightedPullable != _pulledPullable)
            _highlightedPullable.StopHighlight();
        _highlightedPullable = null;
        _cachedClosestPullable = null;
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

        _buildUpPower = defaultPower;
        Player.obj.EndChargeFlash();
    }

    public void CancelPulling() {
        _pollShadowGripButtonRelease = false;
        _isPullingObject = false;
        _isControllingObject = false;
        _isOutOfRange = false;
        _wasIdleGroundedWhilePulling = false;
        _lastAppliedObjectVelocity = Vector2.zero;
        _verticalFollowVelocity = 0f;
        _horizontalFollowVelocity = 0f;
        _horizontalLag = 0f;
        _isVerticallyBlocked = false;
        SetMouseAimIndicatorVisible(false);

        ShadowTwinMovement.obj.IsPulling = false;
        ShadowTwinMovement.obj.UpdateAnimatorIsPulling(false);
        ShadowTwinPlayer.obj.ResetGravity();

        //For instance tough prisoners can break themselves free and cancel pulling, resulting in no pulled pullable
        if(_pulledPullable != null)
            _deeAudio.PlayShadowPullRelease(_pulledPullable.transform);
        
        // Restore object's gravity
        if (_targetRb != null)
        {
            _targetRb.gravityScale = _originalGravityScale;
            _controlledObjectVelocity = Vector2.zero;
        }
        
        // Stop shadow pull object moving loop
        if (_isObectMovingLoopPlaying)
        {
            _deeAudio.StopInstanceWithFadeOut(ref _shadowPullObjectMovingLoopInstance);
            _isObectMovingLoopPlaying = false;
        }
        
        // Stop shadow pull loop
        _deeAudio.StopInstanceWithFadeOut(ref _shadowPullLoopInstance);
        
        ResetPullableObject();

        HidePullRangeGuide();
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
                if (_highlightedPullable == _pulledPullable)
                    _highlightedPullable = null;
            }
        }
        _pulledPullable = null;
        _pulledCollider = null;
    }

    private void SetPullable(Pullable pullable) {
        //Grabbing the object that's already held would store its zeroed grip gravity as the original gravity,
        //and the object would float away when released
        if(pullable != null && pullable == _pulledPullable)
            return;
        if(pullable != null) {
            _pulledPullable = pullable;
            _targetRb = _pulledPullable.GetRigidbody();
            _pulledPullable.IsPulled = true;
            _pulledPullable.StartGrabbed();
            _pulledCollider = pullable.GetComponent<Collider2D>();
            
            // Play grab sound when object is grabbed
            _deeAudio.PlayShadowPullGrab(ref _forcePullStartSfxInstance);
            
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
            _verticalFollowVelocity = 0f;
            // The object starts at rest, so grabbing it while running makes it trail behind and then catch up
            _horizontalFollowVelocity = 0f;
            _horizontalLag = 0f;
            _isVerticallyBlocked = false;
            _lastAppliedObjectVelocity = Vector2.zero;
            
            // Dee can steer and carry the object right away, grounded or airborne
            _isControllingObject = true;

            // Start the mouse aim point on the grabbed object so it doesn't jump on grab
            SetMouseAimPoint(_targetRb.position);

            // Reset loop tracking
            _isObectMovingLoopPlaying = false;
        }
    }

    private void ShowPullRangeGuide()
    {
        if (_pullRangeGuide == null)
            return;

        // Only show/hide during pull if mode is OnlyOnGrab
        if (_pullRangeGuideMode != PullRangeGuideMode.OnlyOnGrab)
            return;

        if (_pullRangeGuideFadeCoroutine != null)
            StopCoroutine(_pullRangeGuideFadeCoroutine);
        _pullRangeGuideFadeCoroutine = StartCoroutine(FadePullRangeGuide(_pullRangeGuideMaxAlpha, _pullRangeGuideFadeSpeed));
    }

    private void HidePullRangeGuide()
    {
        if (_pullRangeGuide == null)
            return;

        // Only show/hide during pull if mode is OnlyOnGrab
        if (_pullRangeGuideMode != PullRangeGuideMode.OnlyOnGrab)
            return;

        if (_pullRangeGuideFadeCoroutine != null)
            StopCoroutine(_pullRangeGuideFadeCoroutine);
        _pullRangeGuideFadeCoroutine = StartCoroutine(FadePullRangeGuide(0f, _pullRangeGuideFadeSpeed));
    }

    // Public methods for tutorial control
    public void SetPullRangeGuideMode(PullRangeGuideMode mode)
    {
        _pullRangeGuideMode = mode;
        
        if (_pullRangeGuide == null)
            return;
        
        // Stop any ongoing fade
        if (_pullRangeGuideFadeCoroutine != null)
        {
            StopCoroutine(_pullRangeGuideFadeCoroutine);
            _pullRangeGuideFadeCoroutine = null;
        }
        
        // Instantly set alpha based on mode
        Color color = _pullRangeGuide.color;
        if (mode == PullRangeGuideMode.Off)
        {
            color.a = 0f;
        }
        else if (mode == PullRangeGuideMode.AlwaysOn)
        {
            color.a = _pullRangeGuideMaxAlpha;
        }
        // OnlyOnGrab mode doesn't change the current state
        _pullRangeGuide.color = color;
    }

    public void FadeInPullRangeGuide(float duration)
    {
        if (_pullRangeGuide == null)
            return;

        if (_pullRangeGuideFadeCoroutine != null)
            StopCoroutine(_pullRangeGuideFadeCoroutine);
        
        float fadeSpeed = duration > 0f ? _pullRangeGuideMaxAlpha / duration : _pullRangeGuideFadeSpeed;
        _pullRangeGuideFadeCoroutine = StartCoroutine(FadePullRangeGuide(_pullRangeGuideMaxAlpha, fadeSpeed));
    }

    public void FadeOutPullRangeGuide(float duration)
    {
        if (_pullRangeGuide == null)
            return;

        if (_pullRangeGuideFadeCoroutine != null)
            StopCoroutine(_pullRangeGuideFadeCoroutine);
        
        float fadeSpeed = duration > 0f ? _pullRangeGuideMaxAlpha / duration : _pullRangeGuideFadeSpeed;
        _pullRangeGuideFadeCoroutine = StartCoroutine(FadePullRangeGuide(0f, fadeSpeed));
    }

    private IEnumerator FadePullRangeGuide(float targetAlpha, float fadeSpeed)
    {
        Color color = _pullRangeGuide.color;
        while (!Mathf.Approximately(color.a, targetAlpha))
        {
            color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
            _pullRangeGuide.color = color;
            yield return null;
        }
        color.a = targetAlpha;
        _pullRangeGuide.color = color;

        _pullRangeGuideFadeCoroutine = null;
    }

    private void FixedUpdate()
    {
        if (!_isPullingObject || !_isControllingObject)
            return;

        if (_targetRb == null || _pulledPullable == null || _pulledPullable.IsHeavy())
            return;
        
        // Check if pullable became immune - stop controlling immediately
        if (_pulledPullable.IsImmune)
        {
            OnShadowGripButtonReleased();
            return;
        }

        // Standing on the held object would let Dee ride it, so let go of it
        if (IsDeeStandingOnHeldObject())
        {
            ReleaseGrip();
            return;
        }

        Vector2 playerPosition = transform.position;
        Vector2 objectPosition = _targetRb.position;
        Vector2 offsetFromPlayer = objectPosition - GetBoxCenter();
        Vector2 effectiveBoxHalfSize = GetEffectiveBoxHalfSize();
        Vector2 deeVelocity = ShadowTwinPlayer.obj.rigidBody.velocity;
        
        _isOutOfRange = Mathf.Abs(offsetFromPlayer.x) > effectiveBoxHalfSize.x || 
                        Mathf.Abs(offsetFromPlayer.y) > effectiveBoxHalfSize.y;

        // The object got left outside the pull range (blocked by geometry, or Dee moved away faster than it
        // can follow), so let go of it
        if (IsBeyondReleaseRange(offsetFromPlayer))
        {
            ReleaseGrip();
            return;
        }

        // Steering comes from the right stick (gamepad) or the mouse aim point (keyboard + mouse)
        Vector2 steerTargetVelocity = GetSteerTargetVelocity(objectPosition);
        bool hasSteerInput = steerTargetVelocity.sqrMagnitude > MOVING_VELOCITY_EPSILON * MOVING_VELOCITY_EPSILON;
        
        // Check proximity using actual collider geometry so it adapts to each pullable's size.
        // Broad-phase gate: skip the precise (costlier) Distance call unless the centers are close.
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

        // Accelerate towards the steering velocity, or decelerate when there is no steering input
        float velocityChangeRate = hasSteerInput ? controlledObjectAcceleration : controlledObjectDeceleration;
        Vector2 steerVelocity = Vector2.MoveTowards(
            _controlledObjectVelocity,
            steerTargetVelocity,
            velocityChangeRate * Time.fixedDeltaTime
        );

        // Horizontally the object follows Dee with a slight lag. Vertically it only moves from steering, plus a
        // smoothed follow velocity that kicks in when Dee's jump/fall drags it against the vertical range edge.
        _horizontalFollowVelocity = GetSmoothedHorizontalFollowVelocity(
            _horizontalFollowVelocity, deeVelocity.x, hasSteerInput);
        _verticalFollowVelocity = GetSmoothedVerticalFollowVelocity(
            _verticalFollowVelocity, deeVelocity.y, offsetFromPlayer.y, effectiveBoxHalfSize.y);

        // Range limit per axis: block steering that would take the object further out of the box.
        // Motion back towards the box is always allowed, so an object left behind by geometry can recover.
        float predictedOffsetX = offsetFromPlayer.x
            + (steerVelocity.x + _horizontalFollowVelocity - deeVelocity.x) * Time.fixedDeltaTime;
        steerVelocity.x = LimitOutwardAxisVelocity(steerVelocity.x, predictedOffsetX, effectiveBoxHalfSize.x);
        // The follow lag must never trail the object out of range either, so at the edge it keeps pace with Dee
        float laggedOffsetX = offsetFromPlayer.x
            + (steerVelocity.x + _horizontalFollowVelocity - deeVelocity.x) * Time.fixedDeltaTime;
        if (Mathf.Abs(laggedOffsetX) > effectiveBoxHalfSize.x
            && (_horizontalFollowVelocity - deeVelocity.x) * laggedOffsetX > 0f)
        {
            _horizontalFollowVelocity = deeVelocity.x;
        }
        float predictedOffsetY = offsetFromPlayer.y
            + (steerVelocity.y + _verticalFollowVelocity - deeVelocity.y) * Time.fixedDeltaTime;
        steerVelocity.y = LimitOutwardAxisVelocity(steerVelocity.y, predictedOffsetY, effectiveBoxHalfSize.y);

        Vector2 objectVelocity = new Vector2(
            steerVelocity.x + _horizontalFollowVelocity, steerVelocity.y + _verticalFollowVelocity);

        // Too close to player - strip only the part of the object's own motion (steering, follow lag and vertical
        // follow) that pushes it into Dee, so it can slide tightly around her without the colliders touching. Dee's
        // own motion is not passed on, so jumping into the object makes her bump her head on it instead of carrying it.
        if (tooCloseToPlayer)
        {
            Vector2 objectOwnVelocity = new Vector2(
                steerVelocity.x + _horizontalFollowVelocity - deeVelocity.x, steerVelocity.y + _verticalFollowVelocity);
            float inwardComponent = Vector2.Dot(objectOwnVelocity, -separationOutward);
            if (inwardComponent > 0f)
                objectVelocity += inwardComponent * separationOutward;
        }

        // Track how far the object trails behind its resting position relative to Dee. Steering re-bases that
        // position, so lag built up before steering is dropped instead of being caught up later.
        _horizontalFollowVelocity = objectVelocity.x - steerVelocity.x;
        if (hasSteerInput)
            _horizontalLag = 0f;
        else
            _horizontalLag += (_horizontalFollowVelocity - deeVelocity.x) * Time.fixedDeltaTime;

        _controlledObjectVelocity = steerVelocity;
        _lastAppliedObjectVelocity = objectVelocity;

        // Apply velocity to the rigidbody until geometry blocks it
        if (_targetRb.bodyType == RigidbodyType2D.Dynamic || _targetRb.bodyType == RigidbodyType2D.Kinematic)
        {
            _targetRb.velocity = objectVelocity;
            
            // Sync our internal velocity with actual rigidbody velocity after physics resolution.
            // This prevents desync when the rigidbody is blocked by walls/collisions.
            StartCoroutine(SyncVelocityAfterPhysics());
        }
        
        // Control the object moving loop: only play it while the player is steering the object. Motion that
        // comes purely from following Dee (horizontal follow lag, vertical follow at the range edge) stays silent.
        // steerVelocity has already been range limited, so steering into the range edge doesn't play it either.
        bool isSteeringObject = hasSteerInput && steerVelocity.magnitude > MOVING_VELOCITY_EPSILON;
        if (isSteeringObject)
        {
            if (!_isObectMovingLoopPlaying)
            {
                _shadowPullObjectMovingLoopInstance = _deeAudio.StartShadowPullMoveLoop();
                _isObectMovingLoopPlaying = true;
            }
        }
        else if (_isObectMovingLoopPlaying)
        {
            _deeAudio.StopInstanceWithFadeOut(ref _shadowPullObjectMovingLoopInstance);
            _isObectMovingLoopPlaying = false;
        }
    }

    /// <summary>Zeroes an axis velocity that would move the object past the box edge on that axis.</summary>
    private static float LimitOutwardAxisVelocity(float axisVelocity, float predictedAxisOffset, float axisHalfSize)
    {
        if (predictedAxisOffset > axisHalfSize && axisVelocity > 0f)
            return 0f;
        if (predictedAxisOffset < -axisHalfSize && axisVelocity < 0f)
            return 0f;
        return axisVelocity;
    }

    /// <summary>
    /// Smooths the sideways follow so the object trails a little behind Dee while she runs and catches up when she
    /// slows down or stops. The catch-up speed is capped by the braking distance so it settles at its resting
    /// position without swinging past it. While the player steers, it just matches Dee's velocity.
    /// </summary>
    private float GetSmoothedHorizontalFollowVelocity(float currentFollowVelocity, float deeVelocityX,
        bool hasSteerInput)
    {
        if (hasSteerInput)
        {
            float steeringRate = Mathf.Max(_horizontalFollowAcceleration, _horizontalFollowDeceleration);
            return Mathf.MoveTowards(currentFollowVelocity, deeVelocityX, steeringRate * Time.fixedDeltaTime);
        }

        // Already trailing as far as allowed, and falling further behind: keep pace with Dee
        bool isFallingFurtherBehind = (currentFollowVelocity - deeVelocityX) * _horizontalLag > 0f;
        if (Mathf.Abs(_horizontalLag) >= _maxHorizontalLag && isFallingFurtherBehind)
            return deeVelocityX;

        // The object trails by an amount that scales with Dee's speed, so it hangs back while she runs and catches
        // up when she stops
        float targetLag = Mathf.Clamp(-deeVelocityX * _horizontalLagTime, -_maxHorizontalLag, _maxHorizontalLag);
        float lagError = _horizontalLag - targetLag;
        float lagErrorDistance = Mathf.Abs(lagError);
        float catchUpSpeed = Mathf.Min(lagErrorDistance * _horizontalFollowCatchUpRate,
            Mathf.Sqrt(2f * _horizontalFollowDeceleration * lagErrorDistance));
        float targetFollowVelocity = deeVelocityX - Mathf.Sign(lagError) * catchUpSpeed;

        bool isSpeedingUp = targetFollowVelocity * currentFollowVelocity >= 0f
            && Mathf.Abs(targetFollowVelocity) > Mathf.Abs(currentFollowVelocity);
        float changeRate = isSpeedingUp ? _horizontalFollowAcceleration : _horizontalFollowDeceleration;
        return Mathf.MoveTowards(currentFollowVelocity, targetFollowVelocity, changeRate * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Smooths the vertical follow velocity. While the object is at (or past) the vertical range edge and Dee moves
    /// away from it, it targets Dee's vertical velocity, plus a catch-up term that pulls a lagging object back to
    /// max range. Otherwise it eases back to zero. Speeding up is gentler than slowing down.
    /// </summary>
    private float GetSmoothedVerticalFollowVelocity(float currentFollowVelocity, float deeVelocityY, float offsetY,
        float halfSizeY)
    {
        bool isAtVerticalEdge = Mathf.Abs(offsetY) >= halfSizeY - VERTICAL_EDGE_TOLERANCE;
        // Dee moving away from the object means her vertical velocity points opposite to the object's offset.
        // Ignored while grounded so the small ground-snapping velocity doesn't drag the object down.
        bool isDeeMovingAway = !ShadowTwinMovement.obj.isGrounded && deeVelocityY * offsetY < 0f;

        float targetFollowVelocity = 0f;
        if (isAtVerticalEdge && isDeeMovingAway)
            targetFollowVelocity = deeVelocityY;

        // An object that lagged past the edge is pulled back towards it (towards Dee)
        float distanceBeyondEdge = Mathf.Abs(offsetY) - halfSizeY;
        if (distanceBeyondEdge > 0f)
            targetFollowVelocity -= Mathf.Sign(offsetY) * distanceBeyondEdge * _verticalFollowCatchUpRate;

        bool isSpeedingUp = targetFollowVelocity * currentFollowVelocity >= 0f
            && Mathf.Abs(targetFollowVelocity) > Mathf.Abs(currentFollowVelocity);
        float changeRate = isSpeedingUp ? _verticalFollowAcceleration : _verticalFollowDeceleration;
        float followVelocity = Mathf.MoveTowards(currentFollowVelocity, targetFollowVelocity,
            changeRate * Time.fixedDeltaTime);

        // Once Dee stops dragging it, the follow may only bring the object back to the range edge, never past it
        // towards Dee. This stops leftover follow speed (e.g. when she lands) from overshooting into the range.
        if (!isDeeMovingAway)
        {
            float speedTowardsDee = -Mathf.Sign(offsetY) * followVelocity;
            float maxSpeedTowardsDee = Mathf.Max(0f, distanceBeyondEdge) / Time.fixedDeltaTime;
            if (speedTowardsDee > maxSpeedTowardsDee)
                followVelocity = -Mathf.Sign(offsetY) * maxSpeedTowardsDee;
        }
        return followVelocity;
    }

    /// <summary>
    /// True when the object should be let go: it's outside the full pull box by more than the release margin
    /// horizontally, or vertically while geometry is blocking it (a vertical lag alone is caught up instead).
    /// </summary>
    private bool IsBeyondReleaseRange(Vector2 offsetFromBoxCenter)
    {
        Vector2 releaseHalfSize = _pullBoxSize * 0.5f + Vector2.one * _releaseDistanceBeyondRange;
        bool isBeyondHorizontally = Mathf.Abs(offsetFromBoxCenter.x) > releaseHalfSize.x;
        bool isBeyondVertically = Mathf.Abs(offsetFromBoxCenter.y) > releaseHalfSize.y;
        return isBeyondHorizontally || (isBeyondVertically && _isVerticallyBlocked);
    }
    
    private IEnumerator SyncVelocityAfterPhysics()
    {
        // Wait for physics to resolve
        yield return new WaitForFixedUpdate();
        
        if (_targetRb != null && _isControllingObject)
        {
            // If physics blocked the movement on an axis (wall, floor, ceiling), drop the built-up velocity on that
            // axis so the object doesn't shoot off once it's free again
            Vector2 blockedVelocity = _lastAppliedObjectVelocity - _targetRb.velocity;
            if (Mathf.Abs(blockedVelocity.x) > VELOCITY_SYNC_THRESHOLD)
            {
                _controlledObjectVelocity.x = 0f;
                // A blocked object can't keep its lag, so follow from where it actually ended up
                _horizontalFollowVelocity = _targetRb.velocity.x;
                _horizontalLag = 0f;
            }
            _isVerticallyBlocked = Mathf.Abs(blockedVelocity.y) > VELOCITY_SYNC_THRESHOLD;
            if (_isVerticallyBlocked)
            {
                _controlledObjectVelocity.y = 0f;
            }
        }
    }

    private Vector2 GetBoxCenter() => (Vector2)transform.position + _pullBoxOffset;

    private Vector2 GetEffectiveBoxHalfSize() => (_pullBoxSize - Vector2.one * RANGE_SAFETY_MARGIN * 2f) * 0.5f;

    private Vector2 ClampToEffectiveBox(Vector2 offsetFromBoxCenter)
    {
        Vector2 halfSize = GetEffectiveBoxHalfSize();
        return new Vector2(
            Mathf.Clamp(offsetFromBoxCenter.x, -halfSize.x, halfSize.x),
            Mathf.Clamp(offsetFromBoxCenter.y, -halfSize.y, halfSize.y));
    }

    private bool IsDeeStandingOnHeldObject()
    {
        Collider2D groundCollider = ShadowTwinMovement.obj.GroundCollider;
        return ShadowTwinMovement.obj.isGrounded && groundCollider != null
            && (groundCollider == _pulledCollider || groundCollider.attachedRigidbody == _targetRb);
    }

    private bool IsUsingMouseAim()
    {
        return _playerInput != null && _playerInput.currentControlScheme == KEYBOARD_CONTROL_SCHEME_NAME;
    }

    private InputAction GetPlayerAction(ref InputAction cachedAction, string actionName)
    {
        if (cachedAction == null && _playerInput != null && _playerInput.actions != null)
            cachedAction = _playerInput.actions.FindAction(actionName);
        return cachedAction;
    }

    /// <summary>Desired object velocity relative to Dee, from the right stick or the mouse aim point.</summary>
    private Vector2 GetSteerTargetVelocity(Vector2 objectPosition)
    {
        if (IsUsingMouseAim())
        {
            Vector2 toAimPoint = GetMouseAimPoint() - objectPosition;
            if (toAimPoint.magnitude <= MOUSE_AIM_ARRIVAL_DISTANCE)
                return Vector2.zero;
            return Vector2.ClampMagnitude(toAimPoint * _mouseAimSteeringGain, controlledObjectMaxSpeed);
        }

        InputAction aimAction = GetPlayerAction(ref _shadowGripAimAction, SHADOW_GRIP_AIM_ACTION_NAME);
        Vector2 stickInput = aimAction != null ? aimAction.ReadValue<Vector2>() : Vector2.zero;
        if (stickInput.magnitude < STICK_AIM_DEAD_ZONE)
            return Vector2.zero;
        return Vector2.ClampMagnitude(stickInput, 1f) * controlledObjectMaxSpeed;
    }

    /// <summary>Moves the mouse aim point with mouse movement and keeps the indicator on it.</summary>
    private void UpdateMouseAim()
    {
        bool isAimingWithMouse = _isControllingObject && _targetRb != null && _pulledPullable != null
            && !_pulledPullable.IsHeavy() && IsUsingMouseAim();
        SetMouseAimIndicatorVisible(isAimingWithMouse);
        if (!isAimingWithMouse)
            return;

        InputAction pointerAction = GetPlayerAction(ref _shadowGripPointerAction, SHADOW_GRIP_POINTER_ACTION_NAME);
        Vector2 mouseDelta = pointerAction != null ? pointerAction.ReadValue<Vector2>() : Vector2.zero;
        SetMouseAimPoint(GetMouseAimPoint() + mouseDelta * GetWorldUnitsPerScreenPixel() * _mouseAimSensitivity);
    }

    /// <summary>World position of the mouse aim point, kept inside the pull range around Dee.</summary>
    private Vector2 GetMouseAimPoint()
    {
        Vector2 boxCenter = GetBoxCenter();
        Vector2 offsetFromBoxCenter = new Vector2(_mouseAimOffsetX, _mouseAimWorldY - boxCenter.y);
        return boxCenter + ClampToEffectiveBox(offsetFromBoxCenter);
    }

    /// <summary>Stores a world aim point, clamped to the pull range around Dee.</summary>
    private void SetMouseAimPoint(Vector2 worldPoint)
    {
        Vector2 boxCenter = GetBoxCenter();
        Vector2 clampedOffset = ClampToEffectiveBox(worldPoint - boxCenter);
        _mouseAimOffsetX = clampedOffset.x;
        _mouseAimWorldY = boxCenter.y + clampedOffset.y;
    }

    private void LateUpdate()
    {
        // Positioned after Dee has moved this frame so the indicator doesn't lag behind
        if (_mouseAimIndicator != null && _mouseAimIndicator.enabled)
            _mouseAimIndicator.transform.position = GetMouseAimPoint();
    }

    private static float GetWorldUnitsPerScreenPixel()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic || Screen.height <= 0)
            return FALLBACK_WORLD_UNITS_PER_PIXEL;
        return mainCamera.orthographicSize * 2f / Screen.height;
    }

    private void SetMouseAimIndicatorVisible(bool isVisible)
    {
        if (_mouseAimIndicator != null && _mouseAimIndicator.enabled != isVisible)
            _mouseAimIndicator.enabled = isVisible;
    }

    /// <summary>Replays the pull-start animation whenever Dee comes to a grounded stop while gripping.</summary>
    private void UpdatePullStartAnimationReplay()
    {
        if (!_isPullingObject)
            return;
        bool isIdleGrounded = ShadowTwinMovement.obj.IsIdleGrounded();
        if (isIdleGrounded && !_wasIdleGroundedWhilePulling)
            ShadowTwinMovement.obj.TriggerForcePullAnimation();
        _wasIdleGroundedWhilePulling = isIdleGrounded;
    }

    void Pull()
    {
        _isPullingObject = true;
        
        // Start shadow pull loop (plays regardless of pullable). Stop any previous loop first so its handle is never lost
        _deeAudio.StopInstanceWithFadeOut(ref _shadowPullLoopInstance);
        _shadowPullLoopInstance = _deeAudio.StartShadowPullLoop();
        
        if(_highlightedPullable != null) {
            CameraShakeManager.obj.ForcePushShake();
            SetPullable(_highlightedPullable);
        }
        // Show the range guide whenever pull starts, even with no pullable grabbed
        ShowPullRangeGuide();
        pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().HardCancel();
        ShadowTwinPlayer.obj.StartChargeFlash();
        ShadowTwinPlayer.obj.PlayerPullLight();
        ShadowTwinPlayer.obj.StartFullyChargedVfx();
        ShadowTwinMovement.obj.IsPulling = true;
        ShadowTwinMovement.obj.UpdateAnimatorIsPulling(true);
        // Only play the pull-start animation when Dee is standing still on the ground; while running, jumping or
        // falling, the movement animation keeps playing. UpdatePullStartAnimationReplay plays it once Dee comes to a grounded stop.
        bool isIdleGrounded = ShadowTwinMovement.obj.IsIdleGrounded();
        if (isIdleGrounded)
            ShadowTwinMovement.obj.TriggerForcePullAnimation();
        _wasIdleGroundedWhilePulling = isIdleGrounded;
        PullPowerType chargePowerType = GetChargePowerType();
        ExecuteForcePushVfx(chargePowerType);
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
