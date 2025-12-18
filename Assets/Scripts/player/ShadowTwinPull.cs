using System.Collections;
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

    private AudioSource _forcePushStartChargingAudioSource;

    private bool _isPullDisabled = false;
    private bool _startedChargeAnimation = false;
    private bool _startedFullyChargedAnimation = false;

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
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if(ShadowTwinPlayer.obj.hasCrown && !_isPullDisabled) {
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
        _startedChargeAnimation = false;
        _startedFullyChargedAnimation = false;

        if(_forcePushStartChargingAudioSource != null && _forcePushStartChargingAudioSource.isPlaying) {
            SoundFXManager.obj.FadeOutAndStopSound(_forcePushStartChargingAudioSource, 0.05f);
            _forcePushStartChargingAudioSource = null;
        }

        ShadowTwinPlayer.obj.RestorePlayerPullLight();

        _buildingUpPower = false;
        _buildUpPower = defaultPower;
        _buildUpPowerTime = 0;
        Player.obj.EndChargeFlash();
    }

    public void CancelPulling() {
        _isPullingObject = false;

        ShadowTwinMovement.obj.EndAnchorPull();
        ShadowTwinPlayer.obj.ResetGravity();
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
        }
    }

    private void Update()
    {
        if (_isPullingObject)
        {
            // Continuosly detect new/existing pullable
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
    }

    private void FixedUpdate()
    {
        if (!_isPullingObject)
            return;

        if (_targetRb == null || _pulledPullable == null || _pulledPullable.IsHeavy())
            return;

        Vector2 playerPosition = transform.position;
        Vector2 closestPoint = _pulledCollider != null ? _pulledCollider.ClosestPoint(playerPosition) : _targetRb.position;
        Vector2 toPlayer = playerPosition - closestPoint;
        float distance = toPlayer.magnitude;

        // If close enough, bring the object to a halt in front of the player
        if (distance <= stopDistanceFromPlayer && distance > 0.01f)
        {
            Vector2 directionToPlayer = toPlayer / distance;
            float currentSpeedTowardsPlayer = Vector2.Dot(_targetRb.velocity, directionToPlayer);

            if (_targetRb.bodyType == RigidbodyType2D.Dynamic)
            {
                if (currentSpeedTowardsPlayer > 0f)
                {
                    // Remove the velocity component toward the player so it comes to rest here
                    _targetRb.velocity -= directionToPlayer * currentSpeedTowardsPlayer;
                }
            }
            else if (_targetRb.bodyType == RigidbodyType2D.Kinematic)
            {
                // For kinematic bodies, explicitly place them at the stop distance and clear velocity
                _targetRb.velocity = Vector2.zero;
            }

            return;
        }

        if (distance <= stopDistanceFromPlayer)
            return;

        // Only use horizontal component for direction to ensure purely horizontal pulling
        Vector2 direction = new Vector2(Mathf.Sign(toPlayer.x), 0f).normalized;

        // Only consider horizontal velocity when calculating speed towards player
        float currentSpeedTowardsPlayerX = _targetRb.velocity.x * direction.x;
        float clampedCurrentSpeed = Mathf.Max(0f, currentSpeedTowardsPlayerX);
        float speedRatio = maxPullSpeed > 0f ? Mathf.Clamp01(clampedCurrentSpeed / maxPullSpeed) : 0f;

        float desiredAcceleration = _pulledPullable.GetPullForce() * (1f - speedRatio);
        if (desiredAcceleration <= 0f)
        {
            // Safety clamp: if we somehow exceeded max speed, bring it back down.
            if (_targetRb.bodyType == RigidbodyType2D.Dynamic && maxPullSpeed > 0f)
            {
                float currentTowards = _targetRb.velocity.x * direction.x;
                if (currentTowards > maxPullSpeed)
                {
                    float excess = currentTowards - maxPullSpeed;
                    _targetRb.velocity -= new Vector2(direction.x * excess, 0f);
                }
            }
            return;
        }

        if (_targetRb.bodyType == RigidbodyType2D.Dynamic)
        {
            float forceMagnitude = desiredAcceleration * _targetRb.mass;
            _targetRb.AddForce(direction * forceMagnitude);

            // Clamp after applying force to avoid overshoot from large accumulated forces.
            if (maxPullSpeed > 0f)
            {
                float newTowards = _targetRb.velocity.x * direction.x;
                if (newTowards > maxPullSpeed)
                {
                    float excess = newTowards - maxPullSpeed;
                    _targetRb.velocity -= new Vector2(direction.x * excess, 0f);
                }
            }
        }
        else if (_targetRb.bodyType == RigidbodyType2D.Kinematic)
        {
            // For kinematic bodies, simulate acceleration by updating velocity manually
            float newSpeed = clampedCurrentSpeed + desiredAcceleration * Time.fixedDeltaTime;
            newSpeed = maxPullSpeed > 0f ? Mathf.Min(newSpeed, maxPullSpeed) : newSpeed;
            Vector2 newVelocity = direction * newSpeed;
            _targetRb.velocity = newVelocity;
        }
    }

    public float projectileDelay = 0.1f;

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
            SoundFXManager.obj.PlayForcePushStartCharging(transform);
        } else {
            _isPullingObject = true;
            PullDetectionResult pullable = DetectPullable();
            if(pullable.collider != null) {
                SetPullable(pullable.collider);
            }
            pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().HardCancel();
            _forcePushStartChargingAudioSource = SoundFXManager.obj.PlayForcePushStartCharging(transform);
            ShadowTwinPlayer.obj.StartChargeFlash();
            ShadowTwinPlayer.obj.PlayerPullLight();
            ShadowTwinMovement.obj.IsPulling = true;
            ShadowTwinMovement.obj.TriggerForcePullAnimation();
            PullPowerType chargePowerType = GetChargePowerType();
            ExecuteForcePushVfx(chargePowerType);
        }
    }

    void Dash() {
        PullPowerType chargePowerType = GetChargePowerType();
        //PlayerMovement.obj.ExecuteDash(chargePowerType);
        ExecuteDashVfx(chargePowerType);
        SoundFXManager.obj.PlayForcePushExecute(transform);
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

    private IEnumerator DelayedMovePlatform(float delay, float power) {
        yield return new WaitForSeconds(delay);
        platform.MovePlatform(PlayerMovement.obj.isFacingLeft(), power);
    }

    public float playerOffsetY = 0.1f;
    private IEnumerator DelayedProjectile(float delay, float power, PullPowerType chargePowerType) {
        yield return new WaitForSeconds(delay);
        SoundFXManager.obj.PlayForcePushExecute(transform);
        int playerFacingDirection = PlayerMovement.obj.isFacingLeft() ? -1 : 1;
        ProjectileManager.obj.shootProjectile(
            new Vector3(_collider.bounds.center.x + (_playerOffset * playerFacingDirection) , gameObject.transform.position.y - playerOffsetY, gameObject.transform.position.z),
            playerFacingDirection,
            power,
            Player.obj.hasPowerUp);
        
        //PlayerMovement.obj.ExecuteForcePushWithProjectile(chargePowerType);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 direction = new Vector2(-1, 0f);

        Vector2 origin1 = (Vector2)transform.position + new Vector2(0,  raySpacing);
        Vector2 origin2 = (Vector2)transform.position + new Vector2(0, 0);
        Vector2 origin3 = (Vector2)transform.position + new Vector2(0, -raySpacing);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin1, origin1 + direction * pullRange);
        Gizmos.DrawLine(origin2, origin2 + direction * pullRange);
        Gizmos.DrawLine(origin3, origin3 + direction * pullRange);
    }


    private void OnDestroy()
    {
        obj = null;
    }
}
