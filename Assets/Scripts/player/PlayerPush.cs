using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Cinemachine;

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
    public float pushTiltPower = 2000;
    public float fallTiltPower = 10000;

    [Header("Dependencies")]
    public GameObject pushPowerUpAnimation;

    private float _buildUpPower = 0;
    private bool _buildingUpPower = false;
    private float _buildUpPowerTime = 0;
    
    public FloatyPlatform platform;

    bool CanUsePoweredForcePush => PlayerMovement.obj.isGrounded && Player.obj.hasPowerUp && _buildUpPower >= maxForce;

    private AudioSource _forcePushStartChargingAudioSource;
    private AudioSource _forcePushChargeLoopAudioSource;
    private bool _startedForcePushChargeLoop = false;

    private void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if(Player.obj.hasCape) {
            if (context.performed)
            {
                if (defaultPower < StaminaMgr.obj.GetCurrentStamina()) {
                    pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().HardCancel();
                    _forcePushStartChargingAudioSource = SoundFXManager.obj.PlayForcePushStartCharging(transform);
                    _buildUpPower = defaultPower;
                    _buildingUpPower = true;
                    _buildUpPowerTime = 0;
                    Player.obj.StartChargeFlash();
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
                    //Should tilt the player slightly backwards in air
                    if(!PlayerMovement.obj.isGrounded && !PlayerMovement.obj.isFalling && IsFullyCharged())
                    {
                        if(Player.obj.hasPowerUp) {
                            PlayerMovement.obj.ExecuteFallDash(true, false);
                        } else {
                            float power = PlayerMovement.obj.isFacingLeft() ? pushTiltPower : -pushTiltPower;
                            Player.obj.rigidBody.AddForce(new Vector2(power, 0));
                        }
                    } else if(PlayerMovement.obj.isFalling && Player.obj.CanFallDash) {
                        PlayerMovement.obj.ExecuteFallDash(Player.obj.hasPowerUp && IsFullyCharged(), true);
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
        if(_forcePushStartChargingAudioSource != null && _forcePushStartChargingAudioSource.isPlaying) {
            SoundFXManager.obj.FadeOutAndStopSound(_forcePushStartChargingAudioSource, 0.05f);
            _forcePushStartChargingAudioSource = null;
        }
        if(_forcePushChargeLoopAudioSource != null && _forcePushChargeLoopAudioSource.isPlaying) {
            SoundFXManager.obj.FadeOutAndStopSound(_forcePushChargeLoopAudioSource, 0.05f);
            _startedForcePushChargeLoop = false;
            _forcePushChargeLoopAudioSource = null;
        }

        _buildingUpPower = false;
        _buildUpPower = defaultPower;
        _buildUpPowerTime = 0;
        pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().Cancel();
        Player.obj.EndChargeFlash();
    }

    private void FixedUpdate()
    {
        //Charge animation
        if(_buildingUpPower && _buildUpPower < maxForce)
            pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().Charge();
        else if(_buildUpPower >= maxForce) {
            // if(Player.obj.hasPowerUp)
            //     pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().FullyChargedPoweredUp();
            // else
            pushPowerUpAnimation.GetComponent<ChargeAnimationMgr>().FullyCharged();
        }

        if(_buildingUpPower) {
            _buildUpPowerTime += Time.deltaTime;
            if(_buildUpPower < maxForce && _buildUpPowerTime > minBuildUpPowerTime) {
                _buildUpPower *= powerBuildUpPerFixedUpdate;
            }
        }

        if(IsFullyCharged() && !_startedForcePushChargeLoop) {
            _startedForcePushChargeLoop = true;
            _forcePushChargeLoopAudioSource = SoundFXManager.obj.PlayForcePushChargeLoop(transform);
        }
    }

    public float projectileDelay = 0.1f;

    void PoweredForcePush(float power) {
        Push(power, true);
    }

    void ForcePush(float power) {
        Push(power, false);
    }

    void Push(float power, bool forcePushJump)
    {
        PlayerMovement.obj.TriggerForcePushAnimation();
        ExecuteForcePushVfx();
        StartCoroutine(DelayedProjectile(projectileDelay, power, forcePushJump));
    }

    public void ExecuteForcePushVfx() {
        ShockWaveManager.obj.CallShockWave(_collider.bounds.center);
        Player.obj.ForcePushFlash();
        CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.transform.DOShakePosition(0.13f, new Vector3(0.15f, 0.15f, 0), 30, 90, false, true, ShakeRandomnessMode.Harmonic);
    }

    private IEnumerator DelayedMovePlatform(float delay, float power) {
        yield return new WaitForSeconds(delay);
        platform.MovePlatform(PlayerMovement.obj.isFacingLeft(), power);
    }

    public float playerOffsetY = 0.1f;
    private IEnumerator DelayedProjectile(float delay, float power, bool forcePushJump) {
        yield return new WaitForSeconds(delay);
        SoundFXManager.obj.PlayForcePushExecute(transform);
        int playerFacingDirection = PlayerMovement.obj.isFacingLeft() ? -1 : 1;
        ProjectileManager.obj.shootProjectile(
            new Vector3(_collider.bounds.center.x + (_playerOffset * playerFacingDirection) , gameObject.transform.position.y - playerOffsetY, gameObject.transform.position.z),
            playerFacingDirection,
            power,
            Player.obj.hasPowerUp);
        if(forcePushJump) {
           PlayerMovement.obj.ExecutePoweredForcePushWithProjectile();
           Player.obj.SetHasPowerUp(false);
        }
    }

    private void OnDestroy()
    {
        obj = null;
    }
}
