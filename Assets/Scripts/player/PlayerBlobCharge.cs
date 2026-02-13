using FMOD.Studio;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBlobCharge : MonoBehaviour
{
public static PlayerBlobCharge obj;

    private BoxCollider2D _collider;

    public float maxForce = 15;
    public float powerBuildUpPerFixedUpdate = 1.1f;

    private float _defaultPower = 1;

    private float _buildUpPower = 0;
    private bool _buildingUpPower = false;
    
    private EventInstance _chargeStartSfxInstance;
    private EliBlobAudio _eliBlobAudio;

    private void Awake()
    {
        obj = this;
        _collider = GetComponent<BoxCollider2D>();
        _eliBlobAudio = GetComponent<EliBlobAudio>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        return;
        // if(!PlayerPowersManager.obj.BlobCanExtraJump) {
        //     return;
        // }
        // if (context.performed)
        // {
        //     if (_defaultPower < StaminaMgr.obj.GetCurrentStamina()) {
        //         _forcePushStartChargingAudioSource = SoundFXManager.obj.PlayForcePushStartCharging(transform);
        //         _buildUpPower = _defaultPower;
        //         _buildingUpPower = true;
        //         PlayerBlob.obj.StartChargeFlash();
        //     }
        // }
        // if(context.canceled) {
        //     if(PlayerBlobMovement.obj.IsTransitioningBetweenLevels()) {
        //         ResetBuiltUpPower();
        //         return;
        //     }
        //     //Need to check that we are building power before we can push. If not the push will be executed on button release.
        //     if(_buildingUpPower && IsFullyCharged())
        //     {
        //         ForcePush();
        //     }
            
        //     ResetBuiltUpPower();
        // }
    }

    public bool IsFullyCharged() {
        return _buildUpPower >= maxForce;
    }

    public void ResetBuiltUpPower() {
        if(AudioUtils.IsPlaying(_chargeStartSfxInstance))
            AudioUtils.SafeStop(ref _chargeStartSfxInstance);

        _buildingUpPower = false;
        _buildUpPower = _defaultPower;
        PlayerBlob.obj.EndChargeFlash();
    }

    private void FixedUpdate()
    {
        if(_buildingUpPower) {
            if(_buildUpPower < maxForce) {
                _buildUpPower *= powerBuildUpPerFixedUpdate;
            }
        }
    }

    public float projectileDelay = 0.1f;

    void ForcePush() {
        Push();
    }

    public float testPower = 10;
    void Push()
    {
        ExecuteForcePushVfx();
        _eliBlobAudio.PlayChargeRelease();
        //PlayerBlobMovement.obj.ExecuteChargedJump();
    }

    public void ExecuteForcePushVfx() {
        ShockWaveManager.obj.CallShockWave(_collider.bounds.center, 0.2f, 0.05f, 0.15f);
        PlayerBlob.obj.ForcePushFlash();
        CameraShakeManager.obj.ForcePushShake();
    }

    private void OnDestroy()
    {
        obj = null;
    }
}
