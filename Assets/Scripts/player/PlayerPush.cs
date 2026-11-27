using System.Collections;
using FMOD.Studio;
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

    private EventInstance _forcePushStartSfxInstance;

    //Is used to disable charge while transforming to blob
    private bool _isChargeDisabled = false;
    private bool _startedChargeAnimation = false;
    private bool _startedFullyChargedAnimation = false;
    private EliAudio _eliAudio;
    private PlayerInput _playerInput;

    private const string SHOOT_ACTION_NAME = "Shoot";
    //Set when charging was started by an automatic switch, so the button release has to be polled
    private bool _pollShootButtonRelease = false;

    public enum ChargePowerType {
        Partial,
        Full,
        Powered
    }

    private void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
        _eliAudio = GetComponent<EliAudio>();
        _playerInput = GetComponent<PlayerInput>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        //After an automatic switch the held button (e.g. an analog trigger) can still send started/performed.
        //The charge is already running from that press, so keep polling and only react to the release
        if(_pollShootButtonRelease && !context.canceled)
            return;
        //A real input event takes over from the polling started by an automatic switch
        _pollShootButtonRelease = false;
        if(PlayerPowersManager.obj.EliCanForcePush && !_isChargeDisabled) {
            if (context.performed)
            {
                StartCharge();
            }
            if(context.canceled) {
                ReleaseCharge();
            }
        }
    }

    /// <summary>True if Eli is allowed to start charging the force push right now.</summary>
    public bool CanStartCharge() {
        return PlayerPowersManager.obj.EliCanForcePush && !_isChargeDisabled;
    }

    /// <summary>
    /// Starts charging after an automatic switch from Dee, where the Shoot button is already held.
    /// The release is polled, since a button held when the PlayerInput got enabled never sends canceled.
    /// </summary>
    public void StartChargeFromSwitch() {
        if(!CanStartCharge())
            return;
        StartCharge();
        _pollShootButtonRelease = true;
    }

    private void Update()
    {
        if(_pollShootButtonRelease && !PlayerManager.IsActionPressed(_playerInput, SHOOT_ACTION_NAME)) {
            _pollShootButtonRelease = false;
            ReleaseCharge();
        }
    }

    private void StartCharge() {
        pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().HardCancel();
        _eliAudio.PlayForcePushStart(ref _forcePushStartSfxInstance);
        _buildUpPower = defaultPower;
        _buildingUpPower = true;
        _buildUpPowerTime = 0;
        Player.obj.StartChargeFlash();
        Player.obj.PlayerPushLight();
    }

    private void ReleaseCharge() {
        if(PlayerMovement.obj.IsTransitioningBetweenLevels()) {
            ResetBuiltUpPower();
            return;
        }
        //Need to check that we are building power before we can push. If not the push will be executed on button release.
        if(_buildingUpPower && _buildUpPowerTime >= minBuildUpPowerTime)
        {
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

    public bool IsFullyCharged() {
        return _buildUpPower >= maxForce;
    }

    public bool IsChargedEnoughForShadowJump(float chargeMargin) {
        float requiredCharge = maxForce * (1f - chargeMargin);
        return _buildUpPower >= requiredCharge;
    }

    public void ResetBuiltUpPower() {
        _pollShootButtonRelease = false;
        pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().Cancel();
        Player.obj.AbortFlash();
        Player.obj.EndFullyChargedVfx();
        _startedChargeAnimation = false;
        _startedFullyChargedAnimation = false;

        if(AudioUtils.IsPlaying(_forcePushStartSfxInstance))
            AudioUtils.SafeStop(ref _forcePushStartSfxInstance);

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

    private ChargePowerType GetChargePowerType() {
        bool isFullyCharged = IsFullyCharged();
        if(isFullyCharged && Player.obj.hasPowerUp) 
            return ChargePowerType.Powered;
        else if(isFullyCharged)
            return ChargePowerType.Full;
        else
            return ChargePowerType.Partial;
    }

    public void ExecuteForcePushVfx(ChargePowerType chargePowerType) {
        if(chargePowerType == ChargePowerType.Powered || chargePowerType == ChargePowerType.Full) {
            ShockWaveManager.obj.CallShockWave(_collider.bounds.center, 0.2f, 0.05f, 0.15f);
            CameraShakeManager.obj.ForcePushShake();
        }
        Player.obj.ForcePushFlash();
    }

    public void ExecuteShadowJumpVfx() {
        ShockWaveManager.obj.CallShockWave(_collider.bounds.center, 0.2f, 0.05f, 0.15f);
        //CameraShakeManager.obj.ForcePushShake();
        //Player.obj.ShadowJumpFlash();
    }

    private IEnumerator DelayedMovePlatform(float delay, float power) {
        yield return new WaitForSeconds(delay);
        platform.MovePlatform(PlayerMovement.obj.IsFacingLeft(), power);
    }

    public float playerOffsetY = 0.1f;
    private IEnumerator DelayedProjectile(float delay, float power, ChargePowerType chargePowerType) {
        yield return new WaitForSeconds(delay);
        _eliAudio.PlayForcePushRelease();
        int playerFacingDirection = PlayerMovement.obj.IsFacingLeft() ? -1 : 1;
        ProjectileManager.obj.ShootProjectile(
            new Vector3(_collider.bounds.center.x + (_playerOffset * playerFacingDirection) , gameObject.transform.position.y - playerOffsetY, gameObject.transform.position.z),
            playerFacingDirection,
            power,
            Player.obj.hasPowerUp);
        
        PlayerMovement.obj.ExecuteForcePushWithProjectile(chargePowerType);
    }

    public void SimulateShootHold() {
        pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().HardCancel();
        _eliAudio.PlayForcePushStart(ref _forcePushStartSfxInstance);
        _buildUpPower = defaultPower;
        _buildingUpPower = true;
        _buildUpPowerTime = 0;
        Player.obj.StartChargeFlash();
        Player.obj.PlayerPushLight();
    }

    public void AbortShoot() {
        ResetBuiltUpPower();
    }

    public void SimulateShootRelease() {

            if(_buildingUpPower && _buildUpPowerTime >= minBuildUpPowerTime)
            {
                if(platform != null) {
                    float power = Player.obj.hasPowerUp && IsFullyCharged() ? powerUpMaxForce : _buildUpPower;
                    StartCoroutine(DelayedMovePlatform(projectileDelay, power));
                }

                ForcePush(_buildUpPower);
            }
            
            ResetBuiltUpPower();
    }

    private void OnDestroy()
    {
        obj = null;
    }
}
