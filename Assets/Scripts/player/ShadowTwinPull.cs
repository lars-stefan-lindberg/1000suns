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

    private BoxCollider2D _collider;

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
    private Rigidbody2D _targetRb;
    private Pullable _pulledPullable;
    private Collider2D _pulledCollider;
    private bool _isPulling = false;

    public enum PullPowerType {
        Full,
        Powered
    }

    private void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if(ShadowTwinPlayer.obj.hasCrown && !_isPullDisabled) {
            if (context.performed)
            {                
                Pull();
            }
            if(context.canceled) {
                CancelPulling();
                ShadowTwinMovement.obj.TriggerEndForcePullAnimation();
                ShadowTwinMovement.obj.IsPulling = false;
                ShadowTwinMovement.obj.EndDash();
                ShadowTwinPlayer.obj.RestorePlayerPullLight();
            }
        }
    }

    public float pullRange = 6f;
    public LayerMask pullableMask;
    public LayerMask blockingMask;
    public float raySpacing = 0.2f; // space between the rays

    private Collider2D DetectPullable()
    {
        Vector2 direction = new Vector2(ShadowTwinMovement.obj.isFacingLeft() ? -1 : 1, 0f);

        Vector2 origin1 = (Vector2)transform.position + new Vector2(0,  raySpacing);
        Vector2 origin2 = (Vector2)transform.position + new Vector2(0, -raySpacing);

        // Cast both rays
        RaycastHit2D hit1 = Physics2D.Raycast(origin1, direction, pullRange, pullableMask);
        RaycastHit2D hit2 = Physics2D.Raycast(origin2, direction, pullRange, pullableMask);

        //Check if the hit is very close to the player. If so stop pulling
        if(hit1.collider != null && hit1.distance < 1f) {
            return null;
        }
        if(hit2.collider != null && hit2.distance < 1f) {
            return null;
        }

        // Blocked? If a wall lies before the object, cancel
        if (IsBlocked(origin1, direction, hit1)) hit1 = new RaycastHit2D();
        if (IsBlocked(origin2, direction, hit2)) hit2 = new RaycastHit2D();

        return hit1.collider ? hit1.collider : hit2.collider ? hit2.collider : null;
    }

    private bool DetectBlockable()
    {
        Vector2 direction = new Vector2(ShadowTwinMovement.obj.isFacingLeft() ? -1 : 1, 0f);

        Vector2 origin1 = (Vector2)transform.position + new Vector2(0,  raySpacing);
        Vector2 origin2 = (Vector2)transform.position + new Vector2(0, -raySpacing);

        // Cast both rays
        RaycastHit2D hit1 = Physics2D.Raycast(origin1, direction, pullRange, blockingMask);
        RaycastHit2D hit2 = Physics2D.Raycast(origin2, direction, pullRange, blockingMask);

        if(hit1.collider != null || hit2.collider != null)
        {
            return true;
        }
        return false;
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
        _isPulling = false;
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

    private void FixedUpdate()
    {
        if (_isPulling)
        {
            // Continuosly detect new/existing pullable
            Collider2D pullable = DetectPullable();
            if(pullable == null) {
                ResetPullableObject();
            } else if (pullable != _pulledCollider)
            {
                // New pullable detected. Start pulling that pullable instead
                ResetPullableObject();
                SetPullable(pullable);
            }
            if(_targetRb != null) {
                Vector2 toPlayer = (Vector2)transform.position - _targetRb.position;
                float distance = toPlayer.magnitude;

                // If close enough, bring the object to a halt in front of the player
                if (distance <= stopDistanceFromPlayer && distance > 0.01f)
                {
                    Vector2 direction = toPlayer / distance;
                    float currentSpeedTowardsPlayer = Vector2.Dot(_targetRb.velocity, direction);

                    if (currentSpeedTowardsPlayer > 0f)
                    {
                        // Remove the velocity component toward the player so it comes to rest here
                        _targetRb.velocity -= direction * currentSpeedTowardsPlayer;
                    }

                    return;
                }
                if (distance > stopDistanceFromPlayer)
                {
                    Vector2 direction = toPlayer / distance;

                    float currentSpeedTowardsPlayer = Vector2.Dot(_targetRb.velocity, direction);
                    float clampedCurrentSpeed = Mathf.Max(0f, currentSpeedTowardsPlayer);
                    float speedRatio = maxPullSpeed > 0f ? Mathf.Clamp01(clampedCurrentSpeed / maxPullSpeed) : 0f;

                    float desiredAcceleration = _pulledPullable.GetPullForce() * (1f - speedRatio);

                    if (desiredAcceleration > 0f)
                    {
                        float forceMagnitude = desiredAcceleration * _targetRb.mass;
                        _targetRb.AddForce(direction * forceMagnitude);
                    }
                }
            }
        }
    }

    public float projectileDelay = 0.1f;

    void Pull()
    {
        _isPulling = true;
        Collider2D pullable = DetectPullable();
        if(pullable != null) {
            SetPullable(pullable);
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
        float facing = Mathf.Sign(transform.localScale.x);
        Vector2 direction = new Vector2(facing, 0f);

        Vector2 origin1 = (Vector2)transform.position + new Vector2(0,  raySpacing);
        Vector2 origin2 = (Vector2)transform.position + new Vector2(0, -raySpacing);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin1, origin1 + direction * pullRange);
        Gizmos.DrawLine(origin2, origin2 + direction * pullRange);
    }


    private void OnDestroy()
    {
        obj = null;
    }
}
