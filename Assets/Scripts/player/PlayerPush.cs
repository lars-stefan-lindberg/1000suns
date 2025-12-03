using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPush : MonoBehaviour
{
    public static PlayerPush obj;

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

    //Is used to disable charge while transforming to blob
    private bool _isChargeDisabled = false;
    private bool _startedChargeAnimation = false;
    private bool _startedFullyChargedAnimation = false;

    public enum ChargePowerType {
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
        if(Player.obj.hasCape && !_isChargeDisabled) {
            if (context.performed)
            {
                if (defaultPower < StaminaMgr.obj.GetCurrentStamina()) {
                    pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().HardCancel();
                    _forcePushStartChargingAudioSource = SoundFXManager.obj.PlayForcePushStartCharging(transform);
                    _buildUpPower = defaultPower;
                    _buildingUpPower = true;
                    _buildUpPowerTime = 0;
                    Player.obj.StartChargeFlash();
                    Player.obj.PlayerPushLight();
                }
            }
            if(context.canceled) {
                if(PlayerMovement.obj.IsTransitioningBetweenLevels()) {
                    ResetBuiltUpPower();
                    return;
                }
                //Need to check that we are building power before we can push. If not the push will be executed on button release.
                if(_buildingUpPower && _buildUpPowerTime >= minBuildUpPowerTime)
                {
                    if(PlayerPowersManager.obj.CanShadowDash && !PlayerMovement.obj.isGrounded && PlayerMovement.obj.IsHorizontalInput())
                    {
                        Dash();
                        ResetBuiltUpPower();
                        return;
                    }

                    if(platform != null) {
                        float power = Player.obj.hasPowerUp && IsFullyCharged() ? powerUpMaxForce : _buildUpPower;
                        StartCoroutine(DelayedMovePlatform(projectileDelay, power));
                    }

                    if(CanUsePoweredForcePush) 
                        PoweredForcePush(powerUpMaxForce);
                    else
                        ForcePush(_buildUpPower);
                }
                
                ResetBuiltUpPower();
            }
        }
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

        Player.obj.RestorePlayerPushLight();

        _buildingUpPower = false;
        _buildUpPower = defaultPower;
        _buildUpPowerTime = 0;
        Player.obj.EndChargeFlash();
    }

    public void DisableCharge() {
        _isChargeDisabled = true;
    }

    public void EnableCharge() {
        _isChargeDisabled = false;
    }

    public void DisableChargeFor(float duration) {
        _isChargeDisabled = true;
        StartCoroutine(EnableChargeAfterDelay(duration));
    }

    private IEnumerator EnableChargeAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        _isChargeDisabled = false;
    }

    private void FixedUpdate()
    {
        //Charge animation
        if(_buildingUpPower && _buildUpPower < maxForce && !_startedChargeAnimation) {
            pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().Charge();
            Player.obj.StartChargeFlash();
            _startedChargeAnimation = true;
        } else if(_buildUpPower >= maxForce && !_startedFullyChargedAnimation) {
            // if(Player.obj.hasPowerUp)
            //     pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().FullyChargedPoweredUp();
            // else
            pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().FullyCharged();
            Player.obj.StartFullyChargedVfx();
            _startedFullyChargedAnimation = true;
        }

        if(_buildingUpPower) {
            _buildUpPowerTime += Time.deltaTime;
            if(_buildUpPower < maxForce && _buildUpPowerTime > minBuildUpPowerTime) {
                _buildUpPower *= powerBuildUpPerFixedUpdate;
            }
        }
    }

    public float projectileDelay = 0.1f;

    void PoweredForcePush(float power) {
        Push(power);
    }

    void ForcePush(float power) {
        Push(power);
    }

    void Push(float power)
    {
        PlayerMovement.obj.TriggerForcePushAnimation();
        ChargePowerType chargePowerType = GetChargePowerType();
        ExecuteForcePushVfx(chargePowerType);
        StartCoroutine(DelayedProjectile(projectileDelay, power, chargePowerType));
    }

    void Dash() {
        ChargePowerType chargePowerType = GetChargePowerType();
        PlayerMovement.obj.ExecuteDash(chargePowerType);
        ExecuteDashVfx(chargePowerType);
        SoundFXManager.obj.PlayForcePushExecute(transform);
    }

    private ChargePowerType GetChargePowerType() {
        bool isFullyCharged = IsFullyCharged();
        if(isFullyCharged && Player.obj.hasPowerUp) 
            return ChargePowerType.Powered;
        else if(isFullyCharged)
            return ChargePowerType.Full;
        else
            return ChargePowerType.Partial;
    }

    public void ExecuteDashVfx(ChargePowerType chargePower) {
        if(chargePower == ChargePowerType.Powered || chargePower == ChargePowerType.Full) {    
            ShockWaveManager.obj.CallShockWave(_collider.bounds.center, 0.2f, 0.05f, 0.15f);
            CameraShakeManager.obj.ForcePushShake();
        }
        Player.obj.ForcePushFlash();
    }

    public void ExecuteForcePushVfx(ChargePowerType chargePowerType) {
        if(chargePowerType == ChargePowerType.Powered || chargePowerType == ChargePowerType.Full) {
            ShockWaveManager.obj.CallShockWave(_collider.bounds.center, 0.2f, 0.05f, 0.15f);
            CameraShakeManager.obj.ForcePushShake();
        }
        Player.obj.ForcePushFlash();
    }

    private IEnumerator DelayedMovePlatform(float delay, float power) {
        yield return new WaitForSeconds(delay);
        platform.MovePlatform(PlayerMovement.obj.isFacingLeft(), power);
    }

    public float playerOffsetY = 0.1f;
    private IEnumerator DelayedProjectile(float delay, float power, ChargePowerType chargePowerType) {
        yield return new WaitForSeconds(delay);
        SoundFXManager.obj.PlayForcePushExecute(transform);
        int playerFacingDirection = PlayerMovement.obj.isFacingLeft() ? -1 : 1;
        ProjectileManager.obj.shootProjectile(
            new Vector3(_collider.bounds.center.x + (_playerOffset * playerFacingDirection) , gameObject.transform.position.y - playerOffsetY, gameObject.transform.position.z),
            playerFacingDirection,
            power,
            Player.obj.hasPowerUp);
        
        PlayerMovement.obj.ExecuteForcePushWithProjectile(chargePowerType);
    }

    private void OnDestroy()
    {
        obj = null;
    }
}
