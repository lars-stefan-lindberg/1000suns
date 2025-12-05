using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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

    bool CanUsePoweredForcePush => PlayerMovement.obj.isGrounded && Player.obj.hasPowerUp && _buildUpPower >= maxForce;

    private AudioSource _forcePushStartChargingAudioSource;

    private bool _isPullDisabled = false;
    private bool _startedChargeAnimation = false;
    private bool _startedFullyChargedAnimation = false;

    public float pullForce = 10f;
    private Rigidbody2D _targetRb;
    private Block _pulledBlock;
    private Collider2D _pulledCollider;

    public enum PullPowerType {
        Partial,
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
                Collider2D pullable = DetectPullable();
                if(pullable != null) {
                    SetPullable(pullable);
                } else {
                    if(DetectBlockable()) {
                        ShadowTwinMovement.obj.ExecuteDash(PullPowerType.Full);
                    }
                }
                pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().HardCancel();
                _forcePushStartChargingAudioSource = SoundFXManager.obj.PlayForcePushStartCharging(transform);
                ShadowTwinPlayer.obj.StartChargeFlash();
                ShadowTwinPlayer.obj.PlayerPullLight();

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

    public bool IsFullyCharged() {
        return _buildUpPower >= maxForce;
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
        _targetRb = null;
        if(_pulledBlock != null)
            _pulledBlock.SetIsBeingPulled(false);
        _pulledBlock = null;
    }

    private void SetPullable(Collider2D pullable) {
        if(pullable != null) {
            GameObject blockParent = pullable.gameObject.transform.parent.gameObject;
            if(blockParent != null && blockParent.CompareTag("Block")) {
                _targetRb = blockParent.GetComponent<Rigidbody2D>();
                _pulledBlock = blockParent.GetComponent<Block>();
                _pulledBlock.SetIsBeingPulled(true);
                _pulledCollider = pullable;
            }
        }
    }

    private void FixedUpdate()
    {
        if (_targetRb != null)
        {
            //Is pulling. Check if what's being pulled is still in line of fire
            Collider2D pullable = DetectPullable();
            if(pullable != _pulledCollider) {
                //New pullable detected. Start pulling that pullable instead
                CancelPulling();
                SetPullable(pullable);                
            } else {
                Vector2 direction = ((Vector2)transform.position - _targetRb.position).normalized;
                _targetRb.AddForce(direction * pullForce);
            }
        }
        //Charge animation
        // if(_buildingUpPower && _buildUpPower < maxForce && !_startedChargeAnimation) {
        //     pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().Charge();
        //     Player.obj.StartChargeFlash();
        //     _startedChargeAnimation = true;
        // } else if(_buildUpPower >= maxForce && !_startedFullyChargedAnimation) {
        //     // if(Player.obj.hasPowerUp)
        //     //     pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().FullyChargedPoweredUp();
        //     // else
        //     pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().FullyCharged();
        //     Player.obj.StartFullyChargedVfx();
        //     _startedFullyChargedAnimation = true;
        // }

        // if(_buildingUpPower) {
        //     _buildUpPowerTime += Time.deltaTime;
        //     if(_buildUpPower < maxForce && _buildUpPowerTime > minBuildUpPowerTime) {
        //         _buildUpPower *= powerBuildUpPerFixedUpdate;
        //     }
        // }
    }

    public float projectileDelay = 0.1f;

    void Pull()
    {
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
        bool isFullyCharged = IsFullyCharged();
        if(isFullyCharged && Player.obj.hasPowerUp) 
            return PullPowerType.Powered;
        else if(isFullyCharged)
            return PullPowerType.Full;
        else
            return PullPowerType.Partial;
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
